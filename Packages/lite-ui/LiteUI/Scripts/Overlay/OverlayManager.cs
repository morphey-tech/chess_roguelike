using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using LiteUI.UI.Model;
using LiteUI.UI.Service;
using UnityEngine;
using VContainer;
using Object = UnityEngine.Object;

namespace LiteUI.Overlay
{
    public sealed class OverlayManager : IDisposable
    {
        private UIService _uiService = null!;

        private readonly Dictionary<Type, IOverlayController> _overlays = new();
        private GameObject _root = null!;

        [Inject]
        private void Construct(UIService uiService)
        {
            _uiService = uiService;
        }

        void IDisposable.Dispose()
        {
            _overlays.Clear();
            if (_root == null) {
                return;
            }
            Object.DestroyImmediate(_root.gameObject);
            _root = null!;
        }

        public void AttachRootContainer(GameObject root)
        {
            _root = root;
            _uiService.AttachRootContainer(root);

            foreach (IOverlayController child in root.GetComponentsInChildren<IOverlayController>(true)) {
                _overlays[child.GetType()] = child;
            }
        }

        public async UniTask Preload<T>()
                where T : MonoBehaviour, IOverlayController
        {
            if (_overlays.ContainsKey(typeof(T))) {
                return;
            }
            T overlayController = await _uiService.CreateAsync<T>(UIModel.Create<T>().Container(_root));
            if (_overlays.ContainsKey(typeof(T))) {
                _uiService.Release(overlayController.gameObject);
                return;
            }
            _overlays[typeof(T)] = overlayController;
            overlayController.Hide();
        }

        public async UniTask<T> Show<T>()
                where T : MonoBehaviour, IOverlayController
        {
            if (!_overlays.ContainsKey(typeof(T))) {
                await Preload<T>();
            }

            IOverlayController overlayController = _overlays[typeof(T)];
            overlayController.Show();
            return (T)overlayController;
        }

        public void Hide<T>(bool unload = false)
        {
            IOverlayController overlayController = _overlays[typeof(T)];
            overlayController.Hide();
            if (!unload) {
                return;
            }
            _overlays.Remove(typeof(T));
            GameObject overlayObject = ((MonoBehaviour)(overlayController)).gameObject;
            _uiService.Release(overlayObject);
        }

        public bool IsVisible<T>()
        {
            return _overlays.ContainsKey(typeof(T)) && _overlays[typeof(T)].IsHiding == false;
        }
    }
}
