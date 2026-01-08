using System;
using Cysharp.Threading.Tasks;
using LiteUI.UI.Model;
using LiteUI.UI.Service;
using UnityEngine;
using VContainer;

namespace LiteUI.Dialog.Service
{
  public class SimpleDialogLoader : IDialogLoader
  {
    private readonly UIService _uiService;

    [Inject]
    public SimpleDialogLoader(UIService uiService)
    {
      _uiService = uiService;
    }

    public UniTask<MonoBehaviour> LoadDialogAsync(Type dialogType, object?[]? parameters,
                                                  GameObject container)
    {
      return _uiService.CreateAsync(UIModel.Create(dialogType, parameters).Container(container));
    }

    public void Unload(MonoBehaviour dialogController)
    {
      _uiService.Release(dialogController.gameObject);
    }
  }
}
