using System.Collections.Generic;
using Project.Gameplay.Gameplay.UI;
using Project.Unity.UI.Extensions;
using UnityEngine;
using VContainer;

namespace Project.Unity.UI.Components
{
  [RequireComponent(typeof(RectTransform))]
  public class AnchorToTarget : MonoBehaviour
  {
    private IUIService _uiService = null!;
    private Canvas _canvas = null!;
    
    public struct ClampBorders
    {
      public Vector2 TopBorderA;
      public Vector2 TopBorderB;
      public Vector2 BottomBorderA;
      public Vector2 BottomBorderB;
      public Vector2 LeftBorderA;
      public Vector2 LeftBorderB;
      public Vector2 RightBorderA;
      public Vector2 RightBorderB;
    }

    private enum EnumUpdatePosType
    {
      HARD = 0,
      LERP = 1,
      MOVE_TOWARDS = 2
    }

    [SerializeField] private Vector3 _anchorOffset;
    [SerializeField] private Vector3 _worldLocalOffset;
    [SerializeField] private Vector3 _worldOffset;
    [SerializeField] private Vector3 _nonTargetViewportPosition;
    [SerializeField] private EnumUpdatePosType _updatePosType;

    [SerializeField] private bool _clampInsideScreen;
    [SerializeField] private bool _clampByDirection;
    [SerializeField] private bool _useSize;
    [SerializeField] private Vector2 _screenLeftBottomBorders;
    [SerializeField] private Vector2 _screenRightTopBorders;
    [SerializeField] private bool _useSafeArea = true;
    [SerializeField] private bool _avoidance;
    [SerializeField] private float _lerpSpeed;

    private IAnchorToTargetTicker _ticker = null!;

    private Transform _target;
    private Vector3? _targetPosition;
    private bool _targetIsRect;
    private RectTransform _rectTransform;
    private Rect? _screenRect;
    public Rect ScreenRectRaw;
    private bool _lockOnTarget = true;
    private Vector2 _prevSize;
    private List<AnchorToTarget> _avoidanceTargets = new List<AnchorToTarget>();
    private List<AnchorToTarget> _cachedCollidedAnchors = new List<AnchorToTarget>();
    private int _forceUpdateCount = 0;
    public Vector2 TargetCanvasPosition { get; private set; }
    public CanvasGroup CanvasGroup { get; private set; }
    public bool CanAvoidByX = true;
    public bool CanAvoidByY = true;
    public bool UseAvoidance => _avoidance;

    public bool IsOnScreen { get; private set; }
    public RectTransform RectTransform => _rectTransform;

    public Rect ScreenRect
    {
      get
      {
        if (_screenRect == null || _prevSize != _rectTransform.rect.size)
        {
          Vector3 leftBottomCorner = _useSafeArea
            ? _canvas.ScreenToCanvasPosition(Screen.safeArea.min)
            : _canvas.ViewportToCanvasPosition(new Vector3(0, 0));

          Vector3 rightTopCorner = _useSafeArea
            ? _canvas.ScreenToCanvasPosition(Screen.safeArea.max)
            : _canvas.ViewportToCanvasPosition(new Vector3(1, 1));

          leftBottomCorner += (Vector3)_screenLeftBottomBorders;
          rightTopCorner -= (Vector3)_screenRightTopBorders;

          float width = rightTopCorner.x - leftBottomCorner.x;
          float height = rightTopCorner.y - leftBottomCorner.y;

          _prevSize = _rectTransform.rect.size;
          _screenRect = new Rect(leftBottomCorner.x, leftBottomCorner.y, width, height);
          // TODO: This should not be here
          ScreenRectRaw = new Rect(-Screen.safeArea.max * 0.5f, Screen.safeArea.max);
        }

        return _screenRect.Value;
      }
    }

    public float CameraDistance { get; private set; }

    [Inject]
    private void Construct(IAnchorToTargetTicker ticker, IUIService uiService)
    {
      _ticker = ticker;
      _uiService = uiService;
      _canvas = uiService.Canvas;

      // Регистрируем если объект уже активен
      if (gameObject.activeInHierarchy)
      {
        _ticker.Register(this);
      }
    }

    public ClampBorders GetClampBorders()
    {
      return new()
      {
        TopBorderA = new(ScreenRect.xMin, ScreenRect.yMax),
        TopBorderB = new(ScreenRect.xMax, ScreenRect.yMax),
        BottomBorderA = new(ScreenRect.xMin, ScreenRect.yMin),
        BottomBorderB = new(ScreenRect.xMax, ScreenRect.yMin),
        LeftBorderA = new(ScreenRect.xMin, ScreenRect.yMin),
        LeftBorderB = new(ScreenRect.xMin, ScreenRect.yMax),
        RightBorderA = new(ScreenRect.xMax, ScreenRect.yMin),
        RightBorderB = new(ScreenRect.xMax, ScreenRect.yMax),
      };
    }

    public ClampBorders GetClampBordersRaw()
    {
      return new()
      {
        TopBorderA = new(ScreenRectRaw.xMin, ScreenRectRaw.yMax),
        TopBorderB = new(ScreenRectRaw.xMax, ScreenRectRaw.yMax),
        BottomBorderA = new(ScreenRectRaw.xMin, ScreenRectRaw.yMin),
        BottomBorderB = new(ScreenRectRaw.xMax, ScreenRectRaw.yMin),
        LeftBorderA = new(ScreenRectRaw.xMin, ScreenRectRaw.yMin),
        LeftBorderB = new(ScreenRectRaw.xMin, ScreenRectRaw.yMax),
        RightBorderA = new(ScreenRectRaw.xMax, ScreenRectRaw.yMin),
        RightBorderB = new(ScreenRectRaw.xMax, ScreenRectRaw.yMax),
      };
    }

    private void Start()
    {
      _rectTransform = GetComponent<RectTransform>();
      CanvasGroup = GetComponent<CanvasGroup>();
    }

    public void SetTarget(Transform target, bool needUpdate = true)
    {
      if (target == _target)
        return;

      _target = target;
      _targetIsRect = _target is RectTransform;
      _targetPosition = null;

      if (_rectTransform == null)
        Start();

      if (needUpdate)
        UpdatePosition(true);
    }

    public void SetTarget(Vector3 position, bool needUpdate = true, bool isRect = false)
    {
      _targetPosition = position;
      _targetIsRect = isRect;

      if (_rectTransform == null)
        Start();

      if (needUpdate)
        UpdatePosition(true);
    }

    public void SetWorldLocalOffset(Vector3 offset)
    {
      _worldLocalOffset = offset;
    }

    public void SetWorldOffset(Vector3 offset)
    {
      _worldOffset = offset;
    }

    public void SetForceUpdateCount(int forceUpdateCount)
    {
      _forceUpdateCount = forceUpdateCount;
    }

    public void Tick()
    {
      if (_rectTransform == null)
      {
        return;
      }

      UpdatePosition(_forceUpdateCount > 0);

      if (_forceUpdateCount > 0)
      {
        _forceUpdateCount--;
      }
    }

    public void UpdatePosition(bool ignoreLerp = false)
    {
      if (_rectTransform == null)
        return;

      if (!_targetPosition.HasValue && _target == null)
      {
        _rectTransform.anchoredPosition =
          _canvas.ViewportToCanvasPosition(_nonTargetViewportPosition);
        return;
      }

      Camera main = Camera.main;
      Vector3 targetPosition = Vector3.zero;
      if (_targetIsRect)
      {
        if (_targetPosition.HasValue)
        {
          _rectTransform.anchoredPosition = _targetPosition.Value;
          return;
        }
        else
        {
          // Для RectTransform используем TransformPoint для конвертации в мировой, потом в Canvas
          RectTransform targetRect = _target as RectTransform;
          if (targetRect != null)
          {
            // Берём позицию центра target RectTransform в мировых координатах
            targetPosition = targetRect.TransformPoint(Vector3.zero);
          }
          else
          {
            targetPosition = _target.position;
          }
        }
      }
      else
      {
        targetPosition = _targetPosition ?? _target.position + _target.rotation * _worldLocalOffset;
      }

      targetPosition = _canvas.WorldToCanvasPosition(targetPosition + _worldOffset, main) + _anchorOffset;

      IsOnScreen = ScreenRect.Contains(targetPosition);

      if (!IsOnScreen)
      {
        if (_clampInsideScreen)
          targetPosition = ClampOnScreen(targetPosition, _rectTransform.rect.size, ScreenRect);

        if (_avoidance)
          targetPosition = AvoidOtherObjects(targetPosition, _rectTransform.rect.size);
      }

      TargetCanvasPosition = targetPosition;

      if (ignoreLerp)
      {
        _rectTransform.anchoredPosition = targetPosition;
      }
      else
      {
        switch (_updatePosType)
        {
          case EnumUpdatePosType.HARD:
            _rectTransform.anchoredPosition = targetPosition;
            break;

          case EnumUpdatePosType.LERP:
            _rectTransform.anchoredPosition = Vector2.Lerp(_rectTransform.anchoredPosition, targetPosition,
              Time.deltaTime * _lerpSpeed);
            break;

          case EnumUpdatePosType.MOVE_TOWARDS:
            _rectTransform.anchoredPosition = Vector2.MoveTowards(_rectTransform.anchoredPosition, targetPosition,
              Time.deltaTime * _lerpSpeed);
            break;
        }
      }

      CameraDistance = main != null ? (main.transform.position - targetPosition).magnitude : 0;
    }

    private Vector2 ClampOnScreen(Vector2 position, Vector2 size, Rect screen)
    {
      if (_clampByDirection)
      {
        float t = 1f;
        float xMinOverScreen = position.x < screen.xMin ? Mathf.Abs(position.x - screen.xMin) : 0;
        float xMaxOverScreen = position.x > screen.xMax ? Mathf.Abs(position.x - screen.xMax) : 0;
        float xOverScreen = Mathf.Max(xMinOverScreen, xMaxOverScreen);

        float yMinOverScreen = position.y < screen.yMin ? Mathf.Abs(position.y - screen.yMin) : 0;
        float yMaxOverScreen = position.y > screen.yMax ? Mathf.Abs(position.y - screen.yMax) : 0;
        float yOverScreen = Mathf.Max(yMinOverScreen, yMaxOverScreen) * (screen.width / screen.height);

        bool clampByX = xOverScreen > yOverScreen;

        if (clampByX && position.x < screen.xMin)
          t = Mathf.InverseLerp(screen.center.x, position.x, screen.xMin);
        else if (clampByX && position.x > screen.xMax)
          t = Mathf.InverseLerp(screen.center.x, position.x, screen.xMax);
        else if (!clampByX && position.y < screen.yMin)
          t = Mathf.InverseLerp(screen.center.y, position.y, screen.yMin);
        else if (!clampByX && position.y > screen.yMax)
          t = Mathf.InverseLerp(screen.center.y, position.y, screen.yMax);

        Vector3 newAnchor = Vector3.Lerp(screen.center, position, t);
        return screen.Clamp(newAnchor, _useSize ? size : default);
      }

      return ScreenRect.Clamp(position);
    }

    private Vector2 AvoidOtherObjects(Vector2 position, Vector3 size)
    {
      _cachedCollidedAnchors.Clear();
      SyncCollidedAnchors(position, size, ref _cachedCollidedAnchors);
      //ко-ко-костыль. Вроде проблема решена, но тут может появится другая :(
      float sign = 0;
      foreach (AnchorToTarget anchor in _cachedCollidedAnchors)
      {
        Vector2 dir = (position - anchor.TargetCanvasPosition);
        float widthSum = (size.x + anchor.RectTransform.rect.width);
        float heightSum = (size.y + anchor.RectTransform.rect.height);
        float offsetX = widthSum * 0.5f - Mathf.Abs(dir.x);
        float offsetY = heightSum * 0.5f - Mathf.Abs(dir.y);
        {
          bool offsetByX = true;

          if (CanAvoidByX && CanAvoidByY)
            offsetByX = offsetX < offsetY;
          else if (CanAvoidByX)
            offsetByX = true;
          else if (CanAvoidByY)
            offsetByX = false;

          if (sign == 0)
            sign = offsetByX ? Mathf.Sign(dir.x) : Mathf.Sign(dir.y);

          Vector2 direction = offsetByX
            ? Vector2.right * offsetX * sign
            : Vector2.up * offsetY * sign;

          position += direction;
        }
      }

      return position;
    }

    private void SyncCollidedAnchors(Vector2 position, Vector2 size, ref List<AnchorToTarget> result)
    {
      result.Clear();

      if (CanvasGroup.alpha == 0)
        return;

      foreach (AnchorToTarget target in _avoidanceTargets)
      {
        if (!target.gameObject.activeSelf)
          continue;

        if (target.CanvasGroup != null && target.CanvasGroup.alpha == 0)
          continue;

        Vector2 dir = (position - target.TargetCanvasPosition);
        float offsetX = (size.x + target.RectTransform.rect.width) * 0.5f - Mathf.Abs(dir.x);
        float offsetY = (size.y + target.RectTransform.rect.height) * 0.5f - Mathf.Abs(dir.y);

        if (offsetX > 0 && offsetY > 0)
          result.Add(target);
      }
    }

    public void SetClampOnScreen(bool clamp) => _clampInsideScreen = clamp;

    public void SetViewportPosition(Vector3 position)
    {
      _nonTargetViewportPosition = position;
    }

    public void AddAvoidanceTarget(AnchorToTarget target)
    {
      if (target == this)
        return;

      if (_avoidanceTargets.Contains(target))
        return;

      _avoidanceTargets.Add(target);
    }

    public void RemoveAvoidanceTarget(AnchorToTarget target) => _avoidanceTargets.Remove(target);

    public void SetClampByDirection(bool clamp) => _clampByDirection = clamp;

    private void OnEnable()
    {
      // Не сбрасываем _target — он должен сохраняться при включении/выключении
      _targetPosition = null;

      // Регистрируем если тикер уже доступен (после Construct)
      if (_ticker != null)
      {
        _ticker.Register(this);
      }
    }

    private void OnDisable()
    {
      if (_ticker != null)
      {
        _ticker.Unregister(this);
      }
    }
  }
}
