using System;
using LiteUI.Common.Logger;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using LiteUI.Dialog.Attributes;
using LiteUI.Tweening;
using UnityEngine;

namespace LiteUI.Dialog.Controllers
{
  [PublicAPI]
  public class UIDialog
  {
    private static readonly IUILogger _logger = LoggerFactory.GetLogger<UIDialog>();

    public Type DialogType { get; private set; }
    public MonoBehaviour? DialogController { get; private set; }
    
    private IUIAnimatable? _animatable;
    private UITweenAnimator? _tweenAnimator;

    public bool Opened { get; set; }
    public bool Hiding { get; set; }

    private bool _muted;

    public UIDialog(Type controllerType)
    {
      DialogType = controllerType;
    }

    public void SetDialog(MonoBehaviour dialogController, UIDialogAttribute uiDialogAttribute)
    {
      DialogController = dialogController;
      
      // Проверяем поддержку анимаций: IUIAnimatable или UITweenAnimator
      _animatable = dialogController as IUIAnimatable;
      _tweenAnimator = dialogController.GetComponent<UITweenAnimator>();
    }

    public async UniTask ShowAsync()
    {
      if (!IsInited) {
        _logger.Error("UIDialog not initialized yet");
        return;
      }
      if (Muted) {
        return;
      }
      
      DialogController!.gameObject.SetActive(true);
      
      // Воспроизводим анимацию показа
      if (_animatable != null) {
        await _animatable.AnimateShow();
      } else if (_tweenAnimator != null) {
        await _tweenAnimator.Show();
      }
      
      Opened = true;
    }

    public async UniTask HideAsync()
    {
      if (!IsInited) {
        _logger.Error("UIDialog not initialized yet");
        return;
      }
      if (Hiding) {
        _logger.Warn($"Dialog already hiding {DialogType.Name}");
        return;
      }

      Hiding = true;

      // Воспроизводим анимацию скрытия
      if (_animatable != null) {
        await _animatable.AnimateHide();
      } else if (_tweenAnimator != null) {
        await _tweenAnimator.Hide();
      }
      
      DialogController!.gameObject.SetActive(false);
      Opened = false;
    }

    public bool IsInited => DialogController != null;

    public bool Muted
    {
      set
      {
        _muted = value;
        if (DialogController == null) {
          return;
        }
        DialogController.gameObject.SetActive(!value);
      }
      get => _muted;
    }
  }
}
