using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using LiteUI.UI.Service;
using UnityEngine;
using static LiteUI.Common.Preconditions;

namespace LiteUI.UI.Model
{
    /// <summary>
    /// Пример использования:
    /// _uiService.Create(UiCollectionModel<AvatarMakeupItemPanel>.Create(clothes)
    ///               .Container(_makeup)
    ///               .Name<CatalogMakeupViewModel>(m => m.Id + "Panel")
    ///               .ControllerParamConverter<CatalogMakeupViewModel>(c => new object[] {c, _iconDataSource})
    ///               .LoadItemCallback<CatalogMakeupViewModel>((e, p) => OnMakeupItemLoad(e, p.Param)));
    /// </summary>
    [PublicAPI]
    public class UICollectionModel<TC>
            where TC : MonoBehaviour
    {
        internal UICollectionModel.UiData Data { get; private set; }

        private UICollectionModel()
        {
            Data = new UICollectionModel.UiData();
        }

        public static UICollectionModel<TC> Create(List<object[]> initParams)
        {
            return Create(initParams, true);
        }

        public static UICollectionModel<TC> Create(IList initParams, bool arraysInitParams = false)
        {
            UICollectionModel<TC> result = new() {
                    Data = {
                            Controller = typeof(TC),
                            Collection = initParams,
                            ArraysInitParams = arraysInitParams
                    }
            };
            return result;
        }

        public UICollectionModel<TC> ControllerParamConverter<TP>(Func<TP, object[]> convertCallback)
        {
            CheckState(Data.ControllerParamsConvertCallback == null, "Повторная установка конвертера");
            Data.ControllerParamsConvertCallback = p => convertCallback.Invoke((TP) p);
            return this;
        }

        public UICollectionModel<TC> Container(Transform container)
        {
            CheckState(Data.Container == null,
                       "Повторная установка контейнера '" + (Data.Container != null ? Data.Container.name : "") + "' new '" + container.name + "'");
            Data.Container = container;
            return this;
        }

        public UICollectionModel<TC> Container(GameObject container)
        {
            return Container(container.transform);
        }

        public UICollectionModel<TC> Container(MonoBehaviour container)
        {
            return Container(container.transform);
        }

        public UICollectionModel<TC> LoadItemCallback<TP>(UIService.LoadUIItemCallback<TC, TP>? loadItemCallback)
        {
            CheckState(Data.LoadItemCallback == null, "LoadItemCallback already set");
            Data.LoadItemCallback = (m, i, d) => loadItemCallback?.Invoke((TC) m, new UIService.ItemData<TP>(i, (TP) d));
            return this;
        }

        public UICollectionModel<TC> LoadItemCallback<TP>(UIService.LoadUIItemIndexCallback<TC, TP>? loadItemCallback)
        {
            CheckState(Data.LoadItemCallback == null, "LoadItemCallback already set");
            Data.LoadItemCallback = (m, i, d) => { loadItemCallback?.Invoke((TC) m, i, (TP) d); };
            return this;
        }

        public UICollectionModel<TC> LoadItemCallback(Action<TC>? loadItemCallback)
        {
            CheckState(Data.LoadItemCallback == null, "LoadItemCallback already set");
            Data.LoadItemCallback = (m, _, _) => loadItemCallback?.Invoke((TC) m);
            return this;
        }

        public UICollectionModel<TC> LoadItemCallback(Action<MonoBehaviour> loadItemCallback)
        {
            CheckState(Data.LoadItemCallback == null, "LoadItemCallback already set");
            Data.LoadItemCallback = (m, _, _) => loadItemCallback.Invoke(m);
            return this;
        }

        public UICollectionModel<TC> Name<T>(Func<T, string> nameCallback)
        {
            CheckState(Data.NameCallback == null, "Name already set");
            Data.NameCallback = p => nameCallback.Invoke((T) p);
            return this;
        }
    }
}
