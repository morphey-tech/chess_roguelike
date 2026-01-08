using JetBrains.Annotations;
using UnityEngine;
using static LiteUI.Common.Preconditions;

namespace LiteUI.UI.Model
{
    /// <summary>
    /// Пример использования:
    /// _uiService.Create(UiModel<AvatarMakeupItemPanel>.Create(viewModel, _iconDataSource)
    ///                                .Container(_makeup)
    ///                                .Name("panel1");
    /// </summary>
    [PublicAPI]
    public class UIModel<TC>
            where TC : MonoBehaviour
    {
        internal UIModel.UiData Data { get; private set; }

        private UIModel()
        {
            Data = new UIModel.UiData();
        }

        public static UIModel<TC> Create(params object?[]? initParameters)
        {
            UIModel<TC> result = new() {
                    Data = {
                            Controller = typeof(TC),
                            InitParameters = initParameters
                    }
            };
            return result;
        }

        public UIModel<TC> Container(Transform? container)
        {
            string containerName = container != null ? container.name : "empty container";
            CheckState(Data.Container == null,
                       $"Container already set '{(Data.Container != null ? Data.Container.name : "")}' new '{containerName}'");
            Data.Container = container;
            return this;
        }

        public UIModel<TC> Container(GameObject container)
        {
            return Container(container.transform);
        }

        public UIModel<TC> Container(MonoBehaviour container)
        {
            return Container(container.transform);
        }

        public UIModel<TC> Name(string name)
        {
            CheckState(Data.Name == null);
            Data.Name = name;
            return this;
        }
    }
}
