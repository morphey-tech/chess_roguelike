using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using LiteUI.UI.Model;
using LiteUI.UI.Service;
using UnityEngine;
using VContainer;

namespace LiteUI.Popup.Manager
{
    public class SimplePopupLoader : IPopupLoader
    {
        private readonly UIService _uiService;

        [Inject]
        public SimplePopupLoader(UIService uiService)
        {
            _uiService = uiService;
        }

        public UniTask<MonoBehaviour> LoadPopupAsync(Type dialogType, object?[]? parameters, GameObject container, CancellationToken cancellationToken)
        {
            return _uiService.CreateAsync(UIModel.Create(dialogType, parameters).Container(container), cancellationToken);
        }

        public void Unload(MonoBehaviour popupController)
        {
            _uiService.Release(popupController.gameObject);
        }
    }
}
