using System;
using System.Collections.Generic;
using Project.Core;
using Project.Core.Window;
using Project.Gameplay.Gameplay.UI;
using Project.Gameplay.Gameplay.UI.Project.Gameplay.Gameplay.UI;
using Project.Unity.UI.Components;
using Project.Unity.UI.Components.Game;
using UnityEngine;
using VContainer;

namespace Project.Gameplay.UI
{
    public class WorldUIWindow : ParameterlessWindow
    {
        private IObjectResolver _resolver = null!;
        private IUIAssetService _uiAssetService = null!;
        
        private readonly List<AnchorToTarget> _anchors = new();
        private static readonly Comparison<AnchorToTarget> _anchorComparer = CompareAnchorDistance;

        [Inject]
        private void Construct(IObjectResolver objectResolver, IUIAssetService uiAssetService)
        {
            _resolver = objectResolver;
            _uiAssetService = uiAssetService;
        }

        public T? Add<T>(T template, Transform followTarget) where T : Component
        {
            T? result = _uiAssetService.Instantiate(template, Vector3.zero,
                    Quaternion.identity)
                ?.GetComponent<T>();
            AddExisting(result, followTarget);
            return result;
        }

        public T? Add<T>(T template, Vector3 position, bool isRect = false) where T : Component
        {
            T? result = _uiAssetService.Instantiate(template, position, Quaternion.identity)
                ?.GetComponent<T>();
            AddExisting(result, position, isRect);
            return result;
        }

        public void AddExisting<T>(T instance, Transform followTarget) where T : Component?
        {
            if (instance == null)
                return;

            instance.transform.SetParent(transform, worldPositionStays: true);
            instance.transform.localScale = Vector3.one;
            instance.transform.localRotation = Quaternion.identity;
            AnchorToTarget anchor = instance.AddComponentOnce<AnchorToTarget>();
            _resolver.Inject(anchor);
            _anchors.Add(anchor);
            anchor.SetTarget(followTarget);

            if (instance.TryGetComponent(out ICompletable completable))
                completable.SetOnCompleteAction(OnComplete);

            SyncAnchorsAvoidanceTargets();
        }

        public void AddExistingParented<T>(T instance, Transform followTarget) where T : Component
        {
            if (instance == null)
                return;

            var anchor = instance.AddComponentOnce<AnchorToTarget>();
            anchor.SetTarget(followTarget);
            
            // Инжектим зависимости
            _resolver.Inject(anchor);
            
            _anchors.Add(anchor);
            SyncAnchorsAvoidanceTargets();
        }

        public void AddExisting<T>(T instance, Vector3 position, bool isRect = false) where T : Component?
        {
            if (instance == null)
                return;

            instance.transform.SetParent(transform, worldPositionStays: true);
            instance.transform.localScale = Vector3.one;
            instance.transform.localRotation = Quaternion.identity;
            var anchor = instance.AddComponentOnce<AnchorToTarget>();
            anchor.SetTarget(position, isRect: isRect);
            
            // Инжектим зависимости
            _resolver.Inject(anchor);
            
            _anchors.Add(anchor);

            if (instance.TryGetComponent(out ICompletable completable))
                completable.SetOnCompleteAction(OnComplete);

            SyncAnchorsAvoidanceTargets();
        }

        private void OnComplete(ICompletable component)
        {
            Remove(component.Value);
        }

        public void Remove<T>(T instance) where T : Component
        {
            if (instance == null)
                return;

            RemoveExisting(instance);
            UnityEngine.Object.Destroy(instance.gameObject);
        }

        public void RemoveExisting<T>(T instance) where T : Component
        {
            if (instance == null)
                return;

            if (instance.TryGetComponent(out AnchorToTarget anchor) && _anchors.Contains(anchor))
            {
                _anchors.Remove(anchor);
                RemoveAnchorFromAvoidanceTargets(anchor);
            }
        }

        private void SyncAnchorsAvoidanceTargets()
        {
            foreach (AnchorToTarget anchor in _anchors)
            {
                if (!anchor.UseAvoidance)
                    continue;

                foreach (AnchorToTarget anchor2 in _anchors)
                {
                    if (anchor2.UseAvoidance)
                        anchor.AddAvoidanceTarget(anchor2);
                }
            }
        }

        private void RemoveAnchorFromAvoidanceTargets(AnchorToTarget target)
        {
            foreach (AnchorToTarget anchor in _anchors)
            {
                anchor.RemoveAvoidanceTarget(target);
            }
        }

        private void LateUpdate()
        {
            SortAnchors();
        }

        private void SortAnchors()
        {
            for (var i = _anchors.Count - 1; i >= 0; i--)
            {
                if (_anchors[i] == null)
                {
                    int lastIndex = _anchors.Count - 1;
                    _anchors[i] = _anchors[lastIndex];
                    _anchors.RemoveAt(lastIndex);
                }
            }

            _anchors.Sort(_anchorComparer);

            for (var i = 0; i < _anchors.Count; i++)
            {
                _anchors[i].transform.SetSiblingIndex(i);
            }
        }

        private static int CompareAnchorDistance(AnchorToTarget a, AnchorToTarget b)
        {
            return a?.CameraDistance.CompareTo(b?.CameraDistance ?? float.MaxValue) * -1 ?? 0;
        }
    }
}
