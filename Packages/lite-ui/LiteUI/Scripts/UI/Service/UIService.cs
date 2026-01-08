using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using LiteUI.UI.Component;
using LiteUI.UI.Model;
using UnityEngine;
using VContainer.Unity;

namespace LiteUI.UI.Service
{
  [PublicAPI]
  public sealed partial class UIService : IDisposable
  {
    public delegate void LoadUIItemCallback<in TC, TP>(TC controller, ItemData<TP> itemData)
        where TC : MonoBehaviour;

    public delegate void LoadUIItemIndexCallback<in TC, in TP>(
      TC controller, int index, TP itemData)
        where TC : MonoBehaviour;

    public bool Caching { get; set; } = true;

    public void Dispose()
    {
      FlushCache();
    }

    /**
        * Очищает закешированные префабы
        */
    public void FlushCache()
    {
      _cachedPrefabs.Values.ToList().ForEach(ReleasePrefab);
      _cachedPrefabs.Clear();
      _cachedPrefabsByName.Clear();
    }

    public void AttachRootContainer(GameObject rootContainer)
    {
      _container.InjectGameObject(rootContainer);

      UIIdComponent[] uiIdComponents = rootContainer.GetComponentsInChildren<UIIdComponent>(true);
      foreach (UIIdComponent idComponent in uiIdComponents) {
        UIMetaInfo metaInfo = _uiMetaRegistry.RequireMetaInfo(idComponent.Alias);
        AttachController(metaInfo.Type, idComponent.gameObject, null);
      }
    }

    /**
        * Создает ui-элемент по передаваемой модели
        *
        * @param model модель для создания
        * @return: MonoBehavior контороллера создаваемого ui-элемента
        */
    public UniTask<MonoBehaviour> CreateAsync(UIModel model,
                                              CancellationToken cancellationToken = default)
    {
      return CreateAsync(model.Data, cancellationToken);
    }

    public async UniTask<T> CreateAsync<T>(UIModel model,
                                           CancellationToken cancellationToken = default)
        where T : MonoBehaviour
    {
      return (T)await CreateAsync(model, cancellationToken);
    }

    public UniTask<T> CreateAsync<T>(UIModel<T> model,
                                     CancellationToken cancellationToken = default)
        where T : MonoBehaviour
    {
      return CreateAsync(model.Data, cancellationToken).ContinueWith(m => (T)m);
    }


    /**
        * Создает коллекцию ui-элемент по передаваемой модели
        *
        * @param model модель для создания
        * @return: IPromise, вызывающийся по факту создания все коллекции
        */
    public UniTask<List<TC>> CreateAsync<TC>(UICollectionModel<TC> model,
                                             CancellationToken cancellationToken = default)
        where TC : MonoBehaviour
    {
      return CreateAsync(model.Data, cancellationToken).ContinueWith(m => m.Cast<TC>().ToList());
    }

    public UniTask<List<MonoBehaviour>> CreateAsync(UICollectionModel model,
                                                    CancellationToken cancellationToken = default)
    {
      return CreateAsync(model.Data, cancellationToken);
    }

    public void Release(GameObject instance)
    {
      ReleaseInstance(instance);
    }

    /**
        * Аттачит контроллер к указанному уже загруженному объекту
        *
        * @param TP тип контроллера ui-элемента
        * @param uiObject объект, требуеющий атача контроллера
        * @param initParams список параметров для создания ui-элементов
        */
    public TP AttachController<TP>(GameObject uiObject, params object?[]? initParams)
        where TP : MonoBehaviour
    {
      return (TP)AttachController(typeof(TP), uiObject, initParams);
    }

    /**
        * Аттачит контроллер к указанному уже загруженному объекту
        *
        * @param type тип контроллера ui-элемента
        * @param uiObject объект, требуеющий атача контроллера
        * @param initParams список параметров для создания ui-элементов
        */
    public MonoBehaviour AttachController(Type type, GameObject uiObject,
                                          params object?[]? initParams)
    {
      InitUi(new CreateRequest(type, initParams), uiObject);
      return (MonoBehaviour)uiObject.GetComponent(type);
    }

    /**
         * True, если в данный момент происходит загрузка или инстанцирование UI
         */
    public bool Loading => _requestsCount > 0;

    [PublicAPI]
    public struct ItemData<T>
    {
      public int Index { get; }
      public T Param { get; }

      public ItemData(int index, T param)
      {
        Index = index;
        Param = param;
      }
    }
  }
}
