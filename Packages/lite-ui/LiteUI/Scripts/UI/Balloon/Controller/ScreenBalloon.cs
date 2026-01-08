using LiteUI.Common.Extensions;
using LiteUI.UI.Service;
using UnityEngine;
using VContainer;

namespace LiteUI.UI.Balloon.Controller
{
    public class ScreenBalloon : BaseBalloon
    {
        private RectTransform _balloonTransform = null!;
        private ScreenLayout _screenLayout = null!;
        private Vector3 _prevCameraPosition;
        private Vector3 _prevTargetPosition;

        [Inject]
        private void Construct(ScreenLayout screenLayout)
        {
            _balloonTransform = transform.GetChild(0).gameObject.RequireComponent<RectTransform>();
            _screenLayout = screenLayout;
        }

        protected void UpdatePosition()
        {
            Vector3 newTargetPosition = Target.position;
            Vector3 newCameraPosition = _screenLayout.CameraPosition;
            if (_prevCameraPosition == newCameraPosition && _prevTargetPosition == newTargetPosition) {
                return;
            }
            _prevCameraPosition = newCameraPosition;
            _prevTargetPosition = newTargetPosition;

            Vector2 screenPosition = _screenLayout.GetPointScreenPosition(_prevTargetPosition);
            _balloonTransform.position = screenPosition + Offset;
        }

        private Transform Target => transform.parent;
    }
}
