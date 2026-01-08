using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LiteUI.Common.Logger;
using LiteUI.Common.Extensions;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using static LiteUI.Common.Preconditions;
using Object = UnityEngine.Object;

namespace LiteUI.Addressable.Service
{
  public sealed class AddressableManager : IDisposable
  {
    private static readonly IUILogger _logger = LoggerFactory.GetLogger<AddressableManager>();

    private const int LOAD_CATALOG_TRY_COUNT = 3;
    private static readonly TimeSpan LOAD_CATALOG_TRY_DELAY = TimeSpan.FromSeconds(1f);

    private readonly Dictionary<int, List<AsyncOperationHandle>> _objectHandles = new();
    private readonly Dictionary<int, object> _objectKeys = new();
    private readonly Dictionary<Sprite, Texture2D> _textures = new();

    private bool _disposed;
    private static bool _catalogInited;

    void IDisposable.Dispose()
    {
      _disposed = true;
    }

    public async UniTask InitCatalog()
    {
      if (_catalogInited) {
        return;
      }
      for (int i = 0; i < LOAD_CATALOG_TRY_COUNT; i++) {
        bool lastTry = i == LOAD_CATALOG_TRY_COUNT - 1;
        bool initSuccess = await InitCatalog(lastTry);
        if (initSuccess) {
          _catalogInited = true;
          return;
        }
        await UniTask.Delay(LOAD_CATALOG_TRY_DELAY);
      }
      throw new Exception("Addressables init exception. See logs early");
    }

    [UsedImplicitly]
    public GameObject? Instantiate(GameObject prefab, Vector3 position, Quaternion rotation,
                                   Transform? parent = null)
    {
      return Instantiate(prefab, new InstantiationParameters(position, rotation, parent!));
    }

    public GameObject? Instantiate(GameObject prefab,
                                   InstantiationParameters instantiationParameters = new())
    {
      if (_disposed) {
        return null;
      }

      int prefabInstanceId = prefab.GetInstanceID();
      if (!_objectKeys.ContainsKey(prefabInstanceId)) {
        _logger.Error("Asset is unloaded so can't instantiate");
        return null;
      }

      object key = _objectKeys[prefabInstanceId];
      AsyncOperationHandle<GameObject> operationHandle =
          Addressables.InstantiateAsync(key, instantiationParameters);
      if (!operationHandle.IsDone) {
        _logger.Error("Operation not synchronous");
        return null;
      }

      GameObject result = operationHandle.Result;
      result.transform.localPosition = instantiationParameters.Position;
      result.transform.localRotation = instantiationParameters.Rotation;
      int resultInstanceId = result.GetInstanceID();

      RegisterHandle(resultInstanceId, key, operationHandle);

      return result;
    }

    public UniTask<GameObject> InstantiateAsync(object key, Transform parent,
                                                bool instantiateInWorldSpace = false,
                                                CancellationToken cancellationToken = default)
    {
      return InstantiateAsync(key, new InstantiationParameters(parent, instantiateInWorldSpace),
                              cancellationToken);
    }

    public UniTask<GameObject> InstantiateAsync(object key, Vector3 position, Quaternion rotation,
                                                Transform? parent = null,
                                                CancellationToken cancellationToken = default)
    {
      return InstantiateAsync(key, new InstantiationParameters(position, rotation, parent!),
                              cancellationToken);
    }

    public async UniTask<GameObject> InstantiateAsync(object key,
                                                      InstantiationParameters
                                                          instantiationParameters = new(),
                                                      CancellationToken cancellationToken = default)
    {
      if (_disposed) {
        throw new OperationCanceledException();
      }

      _logger.Trace($"Before instantiating asset, key={key}");
      AsyncOperationHandle<GameObject> operationHandle =
          Addressables.InstantiateAsync(key, instantiationParameters);
      GameObject obj = await operationHandle.ToUniTaskWithResult(cancellationToken);
      if (cancellationToken.IsCancellationRequested) {
        Addressables.ReleaseInstance(operationHandle);
        throw new OperationCanceledException($"Instantiate cancel. Id={key}");
      }
      CheckNotNull(obj);

      int instanceId = obj.GetInstanceID();
      RegisterHandle(instanceId, key, operationHandle);

      if (cancellationToken.IsCancellationRequested) {
        ReleaseInstance(obj);
        cancellationToken.ThrowIfCancellationRequested();
      }

      _logger.Trace($"After instantiated asset, key={key}");
      return obj;
    }

    public async UniTask<TObject> LoadAssetAsync<TObject>(object key,
                                                          CancellationToken cancellationToken =
                                                              default)
        where TObject : Object
    {
      if (_disposed) {
        throw new OperationCanceledException();
      }

      if (typeof(TObject) == typeof(Sprite)) {
        TObject result = (await LoadSpriteAsync(key) as TObject)!;
        return result;
      }
      _logger.Trace($"Loading asset, key={key}");
      AsyncOperationHandle<TObject> operationHandle = Addressables.LoadAssetAsync<TObject>(key);
      TObject obj = await operationHandle.ToUniTaskWithResult(cancellationToken);
      CheckNotNull(obj, $"Loaded asset is null for key={key}");
      int instanceId = obj.GetInstanceID();

      RegisterHandle(instanceId, key, operationHandle);
      if (cancellationToken.IsCancellationRequested) {
        Release(obj);
        cancellationToken.ThrowIfCancellationRequested();
      }

      _logger.Trace($"Loaded asset, name={obj.name}, instanceId={instanceId}");
      return obj;
    }

    public bool HasHandler(GameObject gameObject)
    {
      int instanceId = gameObject.GetInstanceID();
      return _objectHandles.ContainsKey(instanceId);
    }

    public void ReleaseInstance(GameObject gameObject)
    {
      if (_disposed) {
        return;
      }

      CheckNotNull(gameObject);

      string name = gameObject.name;
      int instanceId = gameObject.GetInstanceID();
      if (!_objectHandles.ContainsKey(instanceId)) {
        _logger.Error(
          $"Can't release instance due to handle not registered, name={name}, instanceId={instanceId}");
        return;
      }
      AsyncOperationHandle operationHandle = _objectHandles[instanceId].First();

      UnregisterHandle(instanceId, operationHandle);

      if (!Addressables.ReleaseInstance(operationHandle)) {
        _logger.Error($"Can't release instance, name={name}");
      }
      else {
        _logger.Trace($"Released instance, name={name}, instanceId={instanceId}");
      }
    }

    public void Release(Object obj)
    {
      if (_disposed) {
        return;
      }

      if (obj is Sprite spriteInstance && _textures.TryGetValue(spriteInstance, out Texture2D? texture)) {
        Release(texture);
        Object.Destroy(spriteInstance);
        return;
      }
      CheckNotNull(obj);

      string name = obj.name;
      int instanceId = obj.GetInstanceID();
      if (!_objectHandles.ContainsKey(obj.GetInstanceID())) {
        _logger.Error(
          $"Can't release asset due to handle not registered, name={name}, instanceId={instanceId}");
        return;
      }
      AsyncOperationHandle operationHandle = _objectHandles[instanceId].First();

      UnregisterHandle(instanceId, operationHandle);

      Addressables.Release(operationHandle);
      _logger.Trace($"Released asset, name={name}, instanceId={instanceId}");
    }

    public async UniTask<bool> IsRemote(string skinAssetId)
    {
      var getDownloadSize = Addressables.GetDownloadSizeAsync(skinAssetId);
      long size = await getDownloadSize.ToUniTaskWithResult();
      return size > 0;
    }

    private async UniTask<Sprite> LoadSpriteAsync(object key)
    {
      Object? loadedObject = await LoadAssetAsync<Object>(key);
      switch (loadedObject) {
        case Sprite loadedSprite:
          return loadedSprite;
        case Texture2D loadedTexture:
        {
          Sprite? resultObject = Sprite.Create(loadedTexture,
                                               new Rect(0, 0, loadedTexture.width,
                                                        loadedTexture.height),
                                               new Vector2(0.5f, 0.5f));
          _textures.Add(resultObject, loadedTexture);
          return resultObject!;
        }
        default:
          throw new TypeLoadException(
            $"Unexpected type of loaded object = {loadedObject.GetType()}");
      }
    }

    private void RegisterHandle(int instanceId, object key, AsyncOperationHandle operationHandle)
    {
      if (!_objectHandles.ContainsKey(instanceId)) {
        _objectHandles[instanceId] = new List<AsyncOperationHandle>();
      }
      _objectHandles[instanceId].Add(operationHandle);

      _objectKeys[instanceId] = key;
    }

    private void UnregisterHandle(int instanceId, AsyncOperationHandle operationHandle)
    {
      List<AsyncOperationHandle> operationHandles = _objectHandles[instanceId];
      operationHandles.Remove(operationHandle);
      if (operationHandles.Count == 0) {
        _objectHandles.Remove(instanceId);
        _objectKeys.Remove(instanceId);
      }
    }

    private async UniTask<bool> InitCatalog(bool logExceptionAsError)
    {
      AsyncOperationHandle<UnityEngine.AddressableAssets.ResourceLocators.IResourceLocator>? addressablesInitHandle = null;
      try {
        addressablesInitHandle = Addressables.InitializeAsync();
        await addressablesInitHandle.Value.Task;
        return true;
      }
      catch (OperationCanceledException) {
        throw;
      }
      catch (ArgumentOutOfRangeException e) {
        if (addressablesInitHandle != null) {
          Addressables.Release(addressablesInitHandle);
        }
        if (logExceptionAsError) {
          _logger.Error("Error while init addressables. ArgumentOutOfRangeException", e);
        }
        else {
          _logger.Debug("Error while init addressables. ArgumentOutOfRangeException", e);
        }
        return false;
      }
      catch (Exception e) {
        if (logExceptionAsError) {
          _logger.Error("Error while init addressables", e);
        }
        else {
          _logger.Debug("Error while init addressables", e);
        }
        return false;
      }
    }
  }
}
