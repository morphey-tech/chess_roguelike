using System.Collections;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Project.Core.Core.Assets;
using Project.Core.Core.Logging;
using Sirenix.Utilities;
using UnityEngine.UI;

namespace Project.Core.Window
{

  [DisallowMultipleComponent]
  public class Window : MonoBehaviour
  {
    public event Action<Window> OnClose;
    public event Action<Window> OnHide;
    public event Action<Window> OnBeforeHide;
    public event Action<Window> OnAfterClose;
    public event Action<Window> OnShow;

    private bool _isClosed = false;
    private bool _forceInvisible = false;
    protected Animator Animator => _wndAnimator;
    private Animator _wndAnimator;
    public bool AnimationIsPlaying { get; private set; }
    private WindowAnimationSettings _wndAnimSettings;
    private WindowAnimatorOpenController _windowAnimatorOpenController;
    private static readonly int OpenTrigger = Animator.StringToHash("OpenTrigger");
    private static readonly int CloseTrigger = Animator.StringToHash("CloseTrigger");
    private static readonly int AnimationInteger = Animator.StringToHash("Animation");
    private Action _animationCompleteCallback;
    private CanvasGroup _canvasGroup;
    private AnimationMode _animationMode;
    private IEnumerable<Window> _windowsBeforeShow;
    private IAssetService _assetService;
    private ILogger<Window> _log;

    protected Canvas Canvas { get; private set; }

    public bool CanvasInvisible { get; private set; }

    private enum AnimationMode
    {
      NONE,
      SETTINGS,
      CONTROLLER
    }

    public void Init(IAssetService assetService, ILogService logService)
    {
      _assetService = assetService;
      _log = logService.CreateLogger<Window>();
      if (TryGetComponent(out _wndAnimator))
      {
        if (TryGetComponent(out _wndAnimSettings))
        {
          _animationMode = AnimationMode.SETTINGS;
        }
        else
        {
          _animationMode = AnimationMode.NONE;
        }
      }

      if (!gameObject.TryGetComponent<Canvas>(out var canvas))
        canvas = gameObject.AddComponent<Canvas>();

      Canvas = canvas;

      if (!gameObject.TryGetComponent(out _canvasGroup))
        _canvasGroup = gameObject.AddComponent<CanvasGroup>();

      if (!gameObject.TryGetComponent<GraphicRaycaster>(out _))
        gameObject.AddComponent<GraphicRaycaster>();

      canvas.overrideSorting = true;
      canvas.sortingOrder = ZOrder * -10;

      InitClickablesFeedbackHandlers();
      OnInit();
    }

    private void InitClickablesFeedbackHandlers()
    {
      var buttons = gameObject.GetComponentsInChildren<Button>(true);
      var toggles = gameObject.GetComponentsInChildren<Toggle>(true);

      foreach (var button in buttons)
        InitClickableFeedbackHandlers(button.gameObject);

      foreach (var toggle in toggles)
        InitClickableFeedbackHandlers(toggle.gameObject);
    }

    private void InitClickableFeedbackHandlers(GameObject go)
    {
      /*if (!go.GetComponent<ClickSound>())
      {
        go.AddComponent<ClickSound>().down = SoundSettings.Instance.ClickSound;
      }*/
    }

    protected virtual void OnInit()
    {
    }

    private IEnumerator CloseRoutine()
    {
      _isClosed = true;

      yield return new WaitUntil(() => !gameObject.activeSelf);

      OnClose?.Invoke(this);

      if (_isClosed)
      {
        OnAfterClosed();
        OnAfterClose?.Invoke(this);
        Destroy(gameObject);
      }

      yield return null;
    }

    public void Close()
    {
      if (_isClosed)
      {
        _log.Warning("Window is already closed");
        return;
      } //FIX for NullReferenceException: Object reference not set to an instance of an object. Window+<ClearRoutine>d__19.MoveNext ()

      OnBeforeClosed();
      Hide();
      UI.StartCoroutine(CloseRoutine());
    }

    public void CloseImmediate()
    {
      if (_isClosed)
        return;

      if (HideOtherWindows)
        ShowOtherWindows();

      OnBeforeClosed();
      OnHidden();
      _isClosed = true;
      OnClose?.Invoke(this);
      OnAfterClosed();
      OnAfterClose?.Invoke(this);
      Destroy(gameObject);
    }

    public virtual void ShowWithDeps(object[] deps)
    {
      Show();
    }

    public void Show(bool immediate = false)
    {
      if (!IsValid())
        return;

      if (IsVisible())
        return;

      SetVisible(true);
      _isClosed = false;

      if (!immediate && _wndAnimator != null)
      {
        switch (_animationMode)
        {
          case AnimationMode.SETTINGS:
            _wndAnimator.SetInteger(AnimationInteger, _wndAnimSettings.OpenAnimation);
            _wndAnimator.SetTrigger(OpenTrigger);
            break;
          case AnimationMode.CONTROLLER:
            _windowAnimatorOpenController.Show();
            break;
        }
      }

      if (HideOtherWindows)
      {
        _windowsBeforeShow ??= UI.GetVisibleStack().Where(w => w != this).ToList();
        _windowsBeforeShow?.ForEach(w =>
        {
          if (w != null && !w.IgnoreHideOthersWindows)
            w.SetInvisible(true);
        });
      }

      OnShowed();
      OnShow?.Invoke(this);
    }

    private void SetVisible(bool value = true)
    {
      gameObject.SetActive(value); //debug

      SetMouseInteraction(value);
    }

    public void SetMouseInteraction(bool state)
    {
      _canvasGroup.blocksRaycasts = state;
    }

    public void SetInvisible(bool state, bool force = false)
    {
      if (_canvasGroup == null)
        return;

      if (_forceInvisible && !force)
        return;

      if (force)
        _forceInvisible = state;

      _canvasGroup.alpha = state ? 0 : 1;
      _canvasGroup.blocksRaycasts = !state;
      CanvasInvisible = state;
    }

    protected virtual void OnShowed()
    {
    }

    protected virtual void OnBeforeClosed()
    {
    }

    protected virtual void OnAfterClosed()
    {
    }

    public void Hide() => Hide(false);

    public void HideImmediate() => Hide(true);


    public void Hide(bool immediate)
    {
      if (!IsValid())
        return;

      if (IsVisible() == false)
        return;

      if (AnimationIsPlaying)
      {
        if (!immediate)
          return;

        AnimationIsPlaying = false;
        _wndAnimator.StopPlayback();
      }

      OnBeforeHidden();
      OnBeforeHide?.Invoke(this);

      if (!immediate && _wndAnimator != null)
      {
        switch (_animationMode)
        {
          case AnimationMode.SETTINGS:
            _wndAnimator.SetInteger(AnimationInteger, _wndAnimSettings.CloseAnimation);
            _wndAnimator.SetTrigger(CloseTrigger);
            SetMouseInteraction(false);
            AnimationIsPlaying = true;
            _animationCompleteCallback = () =>
            {
              DefaultHide();
              AnimationIsPlaying = false;
            };
            break;
          case AnimationMode.CONTROLLER:
            AnimationIsPlaying = true;
            _windowAnimatorOpenController.HideAsync().ContinueWith(() =>
            {
              // Window may not be even available
              if (this == null)
                return;

              //animation was stopped by other process
              if (!AnimationIsPlaying)
                return;

              DefaultHide();
              AnimationIsPlaying = false;
            });
            break;
          case AnimationMode.NONE:
            DefaultHide();
            break;
        }
      }
      else
        DefaultHide();

      void DefaultHide()
      {
        if (HideOtherWindows)
        {
          ShowOtherWindows();
        }

        AnimationIsPlaying = false;

        OnHidden();
        SetVisible(false);
        OnHide?.Invoke(this);
      }
    }

    private void ShowOtherWindows()
    {
      _windowsBeforeShow?.ForEach(w =>
      {
        if (w != null && !w.IgnoreHideOthersWindows)
          w.SetInvisible(false);
      });

      _windowsBeforeShow = null;
    }

    protected virtual void OnHidden()
    {
    }

    protected virtual void OnBeforeHidden()
    {
    }

    public bool IsVisible()
    {
      try
      {
        if (gameObject == null)
          return false;
      }
      catch (Exception)
      {
        return false;
      }

      return gameObject.activeInHierarchy;
    }

    public bool IsValid()
    {
      try
      {
        if (gameObject == null)
          return false;
      }
      catch (Exception)
      {
        return false;
      }

      return true;
    }

    public virtual bool NeedShowBackground => false;

    public virtual int ZOrder => 0;

    public virtual bool CanBeClosedByBackground => false;

    protected virtual string WindowOpenSfx => "window_open";

    protected virtual string WindowCloseSfx => "window_close";

    protected virtual bool HideOtherWindows => false;
    protected virtual bool IgnoreHideOthersWindows => false;

    public IEnumerator CloseWithDelay(float delay)
    {
      yield return new WaitForSeconds(delay);
      Close();
    }

    public void CloseWithDelay2(float delay)
    {
      StartCoroutine(CloseWithDelay(delay));
    }

    public void AnimationComplete()
    {
      _animationCompleteCallback?.Invoke();
      _animationCompleteCallback = null;
    }

    public void PlaySound(string path)
    {
      /*if (path.IsNullOrEmpty())
      {
        GameEnv.Logger.Error("sound path is empty");
        return;
      }

      AudioSystem.PlayForget(path, new AudioPlayParams().OnUI());*/
    }
  }
}
