using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace LiteUI.Popup.Manager
{
    public interface IPopupLoader
    {
        UniTask<MonoBehaviour> LoadPopupAsync(Type popupType, object?[]? parameters, GameObject container, CancellationToken cancellationToken);

        void Unload(MonoBehaviour popupController);
    }
}
