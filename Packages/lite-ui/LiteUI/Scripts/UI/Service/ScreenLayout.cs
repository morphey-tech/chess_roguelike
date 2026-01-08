using LiteUI.Common.Logger;
using LiteUI.Common.Model;
using UnityEngine;

namespace LiteUI.UI.Service
{
    public class ScreenLayout
    {
        private readonly IUILogger _logger = LoggerFactory.GetLogger<ScreenLayout>();

        private Canvas _mainCanvas = null!;
        private Camera _mainCamera = null!;

        public void Init(Canvas mainCanvas, Camera mainCamera)
        {
            _mainCanvas = mainCanvas;
            _mainCamera = mainCamera;
        }

        public Vector2 GetObjectScreenPosition(GameObject? target)
        {
            Vector3 objectPosition = target == null ? Vector3.zero : target.transform.position;
            return _mainCamera.WorldToScreenPoint(objectPosition);
        }

        public Vector2 GetPointScreenPosition(Vector3 worldPosition)
        {
            return _mainCamera.WorldToScreenPoint(worldPosition);
        }
        
        private Rect GetSafeArea()
        {
            Rect deviceSafeArea = Screen.safeArea;
            return Application.platform == RuntimePlatform.IPhonePlayer ? 
                           GetIosOptimalSafeArea(deviceSafeArea) : new Rect(deviceSafeArea.x, 0, deviceSafeArea.width, Screen.height);
        }
        
        private Rect GetIosOptimalSafeArea(Rect deviceSafeArea)
        {
            ScreenOrientation orientation = Screen.orientation;
            return orientation switch {
                    ScreenOrientation.PortraitUpsideDown => new Rect(0, deviceSafeArea.y, Screen.width, deviceSafeArea.height),
                    ScreenOrientation.Portrait => new Rect(0, deviceSafeArea.y, Screen.width, deviceSafeArea.height),
                    ScreenOrientation.LandscapeLeft => new Rect(deviceSafeArea.x, 0, deviceSafeArea.width, Screen.height),
                    ScreenOrientation.LandscapeRight => new Rect(deviceSafeArea.x, 0, deviceSafeArea.width, Screen.height),
                    _ => new Rect(0, 0, Screen.width - deviceSafeArea.x, Screen.height)
            };
        }

        public float GetOffset(Direction direction)
        {
            switch (direction) {
                case Direction.LEFT:
                    return SafeAreaScreenOffsetLeft.x;
                case Direction.RIGHT:
                    return -SafeAreaScreenOffsetRight.x;
                case Direction.UP:
                    return -SafeAreaScreenOffsetTop.y;
                case Direction.DOWN:
                    return SafeAreaScreenOffsetBottom.y;
                default:
                    _logger.Warn($"Unsupported direction, {direction}");
                    return 0f;
            }
        }

        public Vector2 ScreenSize
        {
            get
            {
                Rect canvasRect = _mainCanvas.pixelRect;
                float screenWidth = canvasRect.width;
                float screenHeight = canvasRect.height;
                return new Vector2(screenWidth, screenHeight);
            }
        }

        public Vector2 HalfScreenSize => ScreenSize / 2;
        public Vector3 CameraPosition => _mainCamera.transform.position;

        public Rect SafeArea => GetSafeArea();
        
        public float CanvasScale => _mainCanvas.GetComponent<RectTransform>().localScale.x;

        private Vector2 SafeAreaScreenOffsetRight => new(Screen.width - SafeArea.width - SafeArea.x, 0);

        private Vector2 SafeAreaScreenOffsetLeft => new(SafeArea.x, 0);

        private Vector2 SafeAreaScreenOffsetTop => new(0, Screen.height - SafeArea.height - SafeArea.y);

        private Vector2 SafeAreaScreenOffsetBottom => new(0, SafeArea.y);
    }
}
