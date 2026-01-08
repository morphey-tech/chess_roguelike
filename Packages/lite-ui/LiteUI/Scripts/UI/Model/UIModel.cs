using System;
using JetBrains.Annotations;
using UnityEngine;
using static LiteUI.Common.Preconditions;

namespace LiteUI.UI.Model
{
  /// <summary>
  /// Пример использования:
  /// _uiService.Create(UiModel.Create<AvatarMakeupItemPanel>(viewModel, _iconDataSource)
  ///                                .Container(_makeup)
  ///                                .Name("panel1");
  /// </summary>
  [PublicAPI]
  public class UIModel
  {
    internal UiData Data { get; private set; }

    private UIModel()
    {
      Data = new UiData();
    }

    public static UIModel Create<TC>(params object?[]? initParameters)
    {
      return Create(typeof(TC), initParameters);
    }

    public static UIModel Create(Type controller, params object?[]? initParameters)
    {
      UIModel result = new()
      {
          Data =
          {
              Controller = controller,
              InitParameters = initParameters
          }
      };
      return result;
    }

    public UIModel Container(Transform? container)
    {
      string containerName = container != null ? container.name : "empty container";
      CheckState(Data.Container == null,
                 $"Container already set '{(Data.Container != null ? Data.Container.name : "")}' new '{containerName}'");
      Data.Container = container;
      return this;
    }

    public UIModel Container(GameObject container)
    {
      return Container(container.transform);
    }

    public UIModel Container(MonoBehaviour container)
    {
      return Container(container.transform);
    }

    public UIModel Name(string name)
    {
      CheckState(Data.Name == null);
      Data.Name = name;
      return this;
    }

    internal class UiData
    {
      internal Type Controller { get; set; } = null!;
      internal Transform? Container { get; set; }
      internal object?[]? InitParameters { get; set; }
      internal string? Name { get; set; }
    }
  }
}
