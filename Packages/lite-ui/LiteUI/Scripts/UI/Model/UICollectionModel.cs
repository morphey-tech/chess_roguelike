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
    /// _uiService.Create(UiCollectionModel.Create<AvatarMakeupItemPanel>(clothes)
    ///               .Container(_makeup)
    ///               .Name<CatalogMakeupViewModel>(m => m.Id + "Panel")
    ///               .ControllerParamConverter<CatalogMakeupViewModel>(c => new object[] {c, _iconDataSource})
    ///               .LoadItemCallback<AvatarMakeupItemPanel, CatalogMakeupViewModel>((e, p) => OnMakeupItemLoad(e, p.Param)));
    /// </summary>
    [PublicAPI]
    public class UICollectionModel
    {
        internal UiData Data { get; private set; }

        private UICollectionModel()
        {
            Data = new UiData();
        }

        public static UICollectionModel Create<TC>(List<object[]> initParams)
        {
            return Create(typeof(TC), initParams);
        }

        public static UICollectionModel Create(Type controllerType, List<object[]> initParams)
        {
            return Create(controllerType, initParams, true);
        }

        public static UICollectionModel Create<TC>(IList initParams, bool arraysInitParams = false)
        {
            return Create(typeof(TC), initParams, arraysInitParams);
        }

        public static UICollectionModel Create(Type controllerType, IList collection, bool arraysInitParams = false)
        {
            UICollectionModel result = new() {
                    Data = {
                            Controller = controllerType,
                            Collection = collection,
                            ArraysInitParams = arraysInitParams
                    }
            };
            return result;
        }

        public UICollectionModel ControllerParamConverter<TP>(Func<TP, object[]> convertCallback)
        {
            CheckState(Data.ControllerParamsConvertCallback == null, "Повторная установка конвертера");
            Data.ControllerParamsConvertCallback = p => convertCallback.Invoke((TP) p);
            return this;
        }

        public UICollectionModel Container(Transform container)
        {
            CheckState(Data.Container == null,
                       "Повторная установка контейнера '" + (Data.Container != null ? Data.Container.name : "") + "' new '" + container.name + "'");
            Data.Container = container;
            return this;
        }

        public UICollectionModel Container(GameObject container)
        {
            return Container(container.transform);
        }

        public UICollectionModel Container(MonoBehaviour container)
        {
            return Container(container.transform);
        }

        public UICollectionModel LoadItemCallback<TC, TP>(UIService.LoadUIItemCallback<TC, TP>? loadItemCallback)
                where TC : MonoBehaviour
        {
            CheckState(Data.LoadItemCallback == null, "LoadItemCallback already set");
            Data.LoadItemCallback = (m, i, d) => loadItemCallback?.Invoke((TC) m, new UIService.ItemData<TP>(i, (TP) d));
            return this;
        }

        public UICollectionModel LoadItemCallback<TC, TP>(UIService.LoadUIItemIndexCallback<TC, TP>? loadItemCallback)
                where TC : MonoBehaviour
        {
            CheckState(Data.LoadItemCallback == null, "LoadItemCallback already set");
            Data.LoadItemCallback = (m, i, d) => { loadItemCallback?.Invoke((TC) m, i, (TP) d); };
            return this;
        }

        public UICollectionModel LoadItemCallback<TC>(Action<TC>? loadItemCallback)
                where TC : MonoBehaviour
        {
            CheckState(Data.LoadItemCallback == null, "LoadItemCallback already set");
            Data.LoadItemCallback = (m, _, _) => loadItemCallback?.Invoke((TC) m);
            return this;
        }

        public UICollectionModel LoadItemCallback(Action<MonoBehaviour> loadItemCallback)
        {
            CheckState(Data.LoadItemCallback == null, "LoadItemCallback already set");
            Data.LoadItemCallback = (m, _, _) => loadItemCallback.Invoke(m);
            return this;
        }

        public UICollectionModel Name<T>(Func<T, string> nameCallback)
        {
            CheckState(Data.NameCallback == null, "Name already set");
            Data.NameCallback = p => nameCallback.Invoke((T) p);
            return this;
        }

        internal class UiData
        {
            internal Type Controller { get; set; } = null!;
            internal IList? Collection { get; set; }
            internal bool ArraysInitParams { get; set; }
            internal Func<object, object[]>? ControllerParamsConvertCallback { get; set; }
            internal Transform? Container { get; set; }
            internal Action<MonoBehaviour, int, object>? LoadItemCallback { get; set; }
            internal Func<object, string>? NameCallback { get; set; }
        }
    }
}
