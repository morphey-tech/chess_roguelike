using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace LiteUI.Dialog.Service
{
  public interface IDialogLoader
  {
    UniTask<MonoBehaviour> LoadDialogAsync(Type dialogType, object?[]? parameters,
                                           GameObject container);

    void Unload(MonoBehaviour dialogController);
  }
}
