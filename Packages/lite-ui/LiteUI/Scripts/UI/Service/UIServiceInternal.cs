using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Cysharp.Threading.Tasks;
using LiteUI.Addressable.Service;
using LiteUI.Binding;
using LiteUI.Binding.Attributes;
using LiteUI.UI.Exceptions;
using LiteUI.UI.Model;
using LiteUI.UI.Registry;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using static LiteUI.Common.Preconditions;

namespace LiteUI.UI.Service
{
  public partial class UIService
  {
    private readonly AddressableManager _addressableManager;
    private readonly BindingService _bindingService;
    private readonly UIMetaRegistry _uiMetaRegistry;
    private readonly IObjectResolver _container;

    private readonly List<CreateRequest> _inProgressRequests = new();
    private readonly List<CreateResponse> _deferredResponses = new();

    private readonly Dictionary<Type, GameObject> _cachedPrefabs = new();
    private readonly Dictionary<string, GameObject> _cachedPrefabsByName = new();

    private int _requestsCount;

    [Inject]
    public UIService(AddressableManager addressableManager, BindingService bindingService,
                     UIMetaRegistry uiMetaRegistry, IObjectResolver container)
    {
      _addressableManager = addressableManager;
      _bindingService = bindingService;
      _uiMetaRegistry = uiMetaRegistry;
      _container = container;
    }

    private async UniTask<GameObject> Create(CreateRequest request,
                                             CancellationToken cancellationToken)
    {
      if (HasSimilarInProgressRequest(request)) {
        CreateResponse deferredResponse = new(request, cancellationToken);
        _deferredResponses.Add(deferredResponse);
        GameObject gameObject = await deferredResponse.CreateCompletionSource.Task;
        cancellationToken.ThrowIfCancellationRequested();
        return gameObject;
      }

      GameObject? cachedObject = GetFromCache(request);
      if (cachedObject != null) {
        return BuildUIObject(request, cachedObject);
      }

      _inProgressRequests.Add(request);
      _requestsCount++;
      string assetId = request.GetAssetId();

      try {
        GameObject prefab = await _addressableManager.LoadAssetAsync<GameObject>(assetId);
        CheckNotNull(prefab, $"Error at load prefab id={assetId}");
        if (cancellationToken.IsCancellationRequested) {
          if (HasSimilarInProgressRequest(request)) {
            SaveToCache(request, prefab);
            ApplySameDeferred(request, prefab);
          }
          else {
            _addressableManager.Release(prefab);
          }
          cancellationToken.ThrowIfCancellationRequested();
        }
        SaveToCache(request, prefab);
        GameObject result = BuildUIObject(request, prefab);
        ApplySameDeferred(request, prefab);
        return result;
      }
      catch (Exception e) {
        AbortSameDeferred(request, e);
        throw;
      } finally {
        _requestsCount--;
        _inProgressRequests.Remove(request);
      }
      
    }

    private async UniTask<MonoBehaviour> CreateAsync(UIModel.UiData uiData,
                                                     CancellationToken cancellationToken)
    {
      CreateRequest createRequest = new(uiData.Controller, uiData.InitParameters);
      GameObject obj = await Create(createRequest, cancellationToken);

      if (!ReferenceEquals(uiData.Container, null)) {
        if (uiData.Container == null) {
          ReleaseInstance(obj);
          string containerName = uiData.Container != null ? uiData.Container.name : "";
          string controllerName = uiData.Controller.Name;
          throw new UICreateCanceledException(
            $"Container destroyed while item loading. Container={containerName} item={controllerName}");
        }
        obj.transform.SetParent(uiData.Container, false);
      }
      if (uiData.Name != null) {
        obj.name = uiData.Name;
      }
      MonoBehaviour controller = (MonoBehaviour)obj.GetComponent(uiData.Controller);
      return controller;
    }

    private async UniTask<List<MonoBehaviour>> CreateAsync(UICollectionModel.UiData uiData,
                                                           CancellationToken cancellationToken)
    {
      if (uiData.Collection == null || uiData.Collection.Count == 0) {
        return new List<MonoBehaviour>();
      }

      List<UniTask> loadTasks = new(uiData.Collection.Count);
      List<UIModel> uiModels = new(uiData.Collection.Count);
      Dictionary<UIModel, Transform> panels = new();
      Dictionary<UIModel, MonoBehaviour> resultDictionary = new();
      for (int i = 0; i < uiData.Collection.Count; i++) {
        object initItemParam = uiData.Collection[i];
        int index = i;

        object[] parameters = uiData.ControllerParamsConvertCallback != null
                                  ? uiData.ControllerParamsConvertCallback.Invoke(initItemParam)
                                  : (uiData.ArraysInitParams ? (object[])initItemParam
                                         : new[] { initItemParam });

        UIModel uiModel = UIModel.Create(uiData.Controller, parameters).Container(uiData.Container)
                                 .Name(uiData.NameCallback?.Invoke(initItemParam)!);
        uiModels.Add(uiModel);

        UniTask task = CreateAsync(uiModel, cancellationToken).ContinueWith(it => {
          resultDictionary.Add(uiModel, it);
          uiData.LoadItemCallback?.Invoke((MonoBehaviour)it.GetComponent(uiData.Controller), index,
                                          initItemParam);
          if (it != null) {
            // объект может задестроиться в коллбеке, например, если он уже не нужен к моменту загрузки
            panels[uiModel] = it.transform;
          }
        });
        loadTasks.Add(task);
      }

      await UniTask.WhenAll(loadTasks);

      List<MonoBehaviour> result = new();
      uiModels.ForEach(m => {
        panels[m].SetAsLastSibling();
        result.Add(resultDictionary[m]);
      });
      return result;
    }

    private bool HasSimilarInProgressRequest(CreateRequest request)
    {
      return _inProgressRequests.Any(r => r.IsSimilarRequest(request));
    }

    private GameObject? GetFromCache(CreateRequest request)
    {
      if (request.ControllerType != null && _cachedPrefabs.ContainsKey(request.ControllerType)) {
        return _cachedPrefabs[request.ControllerType];
      }
      if (request.PrefabName != null && _cachedPrefabsByName.ContainsKey(request.PrefabName)) {
        return _cachedPrefabsByName[request.PrefabName];
      }
      return null;
    }

    private void SaveToCache(CreateRequest request, GameObject prefab)
    {
      if (!Caching) {
        return;
      }
      if (request.ControllerType != null) {
        _cachedPrefabs[request.ControllerType] = prefab;
      }
      else if (request.PrefabName != null) {
        _cachedPrefabsByName[request.PrefabName] = prefab;
      }
    }

    private List<CreateResponse> GetSimilarResponses(CreateRequest request)
    {
      List<CreateResponse> similarRequests = new();
      foreach (CreateResponse createResponse in _deferredResponses) {
        if (request.IsSimilarRequest(createResponse.CreateRequest)) {
          similarRequests.Add(createResponse);
        }
      }
      return similarRequests;
    }

    private void AbortSameDeferred(CreateRequest request, Exception e)
    {
      List<CreateResponse> similarResponses = GetSimilarResponses(request);
      foreach (CreateResponse responses in similarResponses) {
        responses.CreateCompletionSource.TrySetException(
          new NullReferenceException($"Error at load {request.GetAssetId()}", e));
        _deferredResponses.Remove(responses);
      }
    }

    private void ApplySameDeferred(CreateRequest request, GameObject prefab)
    {
      List<CreateResponse> similarResponses = GetSimilarResponses(request);
      foreach (CreateResponse response in similarResponses) {
        try {
          if (response.CancellationToken.IsCancellationRequested) {
            response.CreateCompletionSource.TrySetCanceled();
            continue;
          }
          GameObject uiObject = BuildUIObject(response.CreateRequest, prefab);
          response.CreateCompletionSource.TrySetResult(uiObject);
        }
        catch (Exception e) {
          response.CreateCompletionSource.TrySetException(e);
        } finally {
          _deferredResponses.Remove(response);
        }
      }
    }

    private GameObject BuildUIObject(CreateRequest request, GameObject prefab)
    {
      CheckNotNull(prefab);
      GameObject instantiatedObject = CheckNotNull(_addressableManager.Instantiate(prefab))!;
      instantiatedObject.name = instantiatedObject.name.Replace("(Clone)", "");
      _container.InjectGameObject(instantiatedObject);
      
      try {
        InitUi(request, instantiatedObject);
      }
      catch (Exception) {
        if (instantiatedObject != null) {
          ReleaseInstance(instantiatedObject);
        }
        throw;
      }
      return instantiatedObject;
    }

    private void InitUi(CreateRequest request, GameObject uiObject)
    {
      if (request.ControllerType != null) {
        _bindingService.Bind(uiObject, request.ControllerType, request.InitParams);
      }
    }

    private void ReleaseInstance(GameObject gameObject)
    {
      _addressableManager.ReleaseInstance(gameObject);
    }

    private void ReleasePrefab(GameObject gameObject)
    {
      _addressableManager.Release(gameObject);
    }

    private struct CreateResponse
    {
      public CreateRequest CreateRequest { get; }
      public UniTaskCompletionSource<GameObject> CreateCompletionSource { get; }
      public CancellationToken CancellationToken { get; }

      public CreateResponse(CreateRequest createRequest, CancellationToken cancellationToken)
      {
        CreateRequest = createRequest;
        CreateCompletionSource = new UniTaskCompletionSource<GameObject>();
        CancellationToken = cancellationToken;
      }
    }

    private sealed class CreateRequest
    {
      public string? PrefabName { get; }
      public Type? ControllerType { get; }
      public object?[]? InitParams { get; }

      public CreateRequest(Type controllerType, object?[]? initParams)
      {
        PrefabName = null;
        ControllerType = controllerType;
        InitParams = initParams;
      }

      public string GetAssetId()
      {
        if (PrefabName != null) {
          return PrefabName;
        }
        Type controller = CheckNotNull(ControllerType)!;
        UIControllerAttribute uiControllerAttribute =
            controller.GetCustomAttribute<UIControllerAttribute>();
        return uiControllerAttribute.Id;
      }

      public bool IsSimilarRequest(CreateRequest another)
      {
        return (ControllerType != null && ControllerType == another.ControllerType) ||
               (PrefabName != null && PrefabName == another.PrefabName);
      }
    }
  }
}
