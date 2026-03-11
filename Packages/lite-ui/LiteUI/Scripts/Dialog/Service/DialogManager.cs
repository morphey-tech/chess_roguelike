using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LiteUI.Common.Logger;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using JetBrains.Annotations;
using LiteUI.Common.Extensions;
using LiteUI.Dialog.Attributes;
using LiteUI.Dialog.Controllers;
using LiteUI.Dialog.Model;
using LiteUI.Element.Images;
using LiteUI.Dialog.Event;
using LiteUI.UI.Exceptions;
using MessagePipe;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using static LiteUI.Common.Preconditions;

namespace LiteUI.Dialog.Service
{
  [PublicAPI]
  public class DialogManager
  {
    private const string DIALOG_SHADE_GO_NAME = "DialogBackgoundShade";
    private const float DIALOG_SHADE_ALPHA = 0.85f;
    
    private static readonly IUILogger _logger = LoggerFactory.GetLogger<DialogManager>();

    private readonly IDialogLoader _dialogLoader;
    private readonly IPublisher<string, DialogMessage> _dialogPublisher;

    private GameObject _dialogContainer = null!;

    private DialogInputLock _dialogInputLock = null!;
    private Image _dialogBackgroundShade = null!;
    private readonly List<DialogData> _deferredDialogs = new();
    private readonly DialogStack _dialogStack = new();

    private readonly List<DialogData> _mutedDialogs = new();
    private List<Type>? _muteExcludedTypes;


    [Inject]
    private DialogManager(IDialogLoader dialogLoader,
                          IPublisher<string, DialogMessage> dialogPublisher)
    {
      _dialogLoader = dialogLoader;
      _dialogPublisher = dialogPublisher;
    }

    public void AttachRootContainer(GameObject root)
    {
      _dialogContainer = root;
      Transform dialogBackgroundShadeTfm = root.transform.Find(DIALOG_SHADE_GO_NAME);
      _dialogBackgroundShade = dialogBackgroundShadeTfm.RequireComponent<Image>();
      _dialogInputLock = DialogInputLock.Create(root);
    }

    public void Show<T>(params object?[]? initParams)
        where T : MonoBehaviour
    {
      Show(typeof(T), initParams);
    }

    public void Show(Type controllerType, params object?[]? initParams)
    {
      if (_dialogStack.IsEmpty) {
        ShowModal(controllerType, initParams);
        return;
      }
      _deferredDialogs.Add(new DialogData(controllerType, initParams, null));
    }

    public void ShowModal<T>(params object?[]? initParams)
    {
      ShowModal(typeof(T), initParams);
    }

    public void ShowModal(Type controllerType, params object?[]? initParams)
    {
      ShowModalAsync(controllerType, initParams).Forget(e => {
        if (e is OperationCanceledException) {
          return;
        }
        if (e is UICreateCanceledException) {
          _logger.Debug("Dialog UI create canceled", e);
          return;
        }
        _logger.Error("Dialog show modal error", e);
      });
    }

    public async UniTask<T> ShowModalAsync<T>(params object?[]? initParams)
        where T : MonoBehaviour
    {
      return (T)await ShowModalAsync(typeof(T), initParams);
    }

    public async UniTask<MonoBehaviour> ShowModalAsync(Type controllerType,
                                                       params object?[]? initParams)
    {
      initParams ??= Array.Empty<object>();
      if (_muteExcludedTypes != null && !_muteExcludedTypes.Contains(controllerType)) {
        UniTaskCompletionSource<MonoBehaviour> showCompletionSource = new();
        _mutedDialogs.Add(new DialogData(controllerType, initParams, showCompletionSource));
        return await showCompletionSource.Task;
      }

      _logger.Debug(
        $"Will show dialog. Type={controllerType.Name}, params='{string.Join(",", initParams.Select(p => $"{p}"))}'");

      UIDialogAttribute uiDialogAttribute = controllerType.GetCustomAttribute<UIDialogAttribute>();
      CheckNotNull(uiDialogAttribute, $"No UIDialogAttribute on class {controllerType.Name}");

      _dialogInputLock.LockInput();

      UIDialog uiDialog = new(controllerType);
      _dialogStack.Add(uiDialog);
      try {
        MonoBehaviour loadedDialog =
            await _dialogLoader.LoadDialogAsync(controllerType, initParams, _dialogContainer);
        if (!_dialogStack.Contains(uiDialog)) {
          _dialogLoader.Unload(loadedDialog);
          throw new OperationCanceledException("Dialog already hide");
        }
        if (uiDialog.Muted) {
          UniTaskCompletionSource<MonoBehaviour> showCompletionSource = new();
          _mutedDialogs.Add(new DialogData(controllerType, initParams, showCompletionSource));
          _dialogLoader.Unload(loadedDialog);
          return await showCompletionSource.Task;
        }
        uiDialog.SetDialog(loadedDialog, uiDialogAttribute);
        _dialogStack.SortDialogs(_dialogInputLock, _dialogBackgroundShade);
        _dialogBackgroundShade.DOFade(DIALOG_SHADE_ALPHA, 0.3f);

        await uiDialog.ShowAsync();
        _dialogPublisher.Publish(DialogMessage.OPENED, new DialogMessage(controllerType));
        return loadedDialog;
      }
      catch
      {
        _dialogBackgroundShade.DOKill();
        _dialogStack.Remove(uiDialog);
        _dialogInputLock.UnlockInput();
        throw;
      }
    }

    public void Hide(MonoBehaviour dialog)
    {
      Hide(dialog.gameObject).Forget();
    }

    public async UniTask Hide(GameObject dialog)
    {
      if (ReferenceEquals(dialog, null)) {
        throw new ArgumentNullException(nameof(dialog), "Try to hide null dialog");
      }
      UIDialog? uiDialog = _dialogStack.FindDialogForObject(dialog);
      if (uiDialog == null) {
        _logger.Warn($"Trying hide dialog but UIDialog not found. Object name={dialog.name}");
        return;
      }
      _logger.Debug($"Will hide dialog of type={uiDialog.DialogType?.Name}");
      _dialogBackgroundShade.DOFade(0f, 0.3f);
      await HideAsync(uiDialog);
      _dialogPublisher.Publish(DialogMessage.CLOSED, new DialogMessage(uiDialog.DialogType));
    }

    public void HideAll()
    {
      _logger.Debug("Hide all dialogs");
      _deferredDialogs.Clear();
      foreach (UIDialog uiDialog in _dialogStack.Items) {
        HideAsync(uiDialog).Forget();
      }
    }

    private async UniTask HideAsync(UIDialog uiDialog)
    {
      if (uiDialog.Hiding) {
        return;
      }

      _dialogStack.Remove(uiDialog);

      if (uiDialog.IsInited) {
        _dialogInputLock.UnlockInput();
      }

      _dialogStack.SortDialogs(_dialogInputLock, _dialogBackgroundShade);

      if (_dialogStack.IsEmpty && _deferredDialogs.Count > 0) {
        DialogData deferredDialog = _deferredDialogs[0];
        _deferredDialogs.RemoveAt(0);
        // ReSharper disable once MethodHasAsyncOverload
        ShowModal(deferredDialog.DialogType, deferredDialog.Params);
      }

      if (uiDialog.IsInited) {
        await uiDialog.HideAsync();

        _dialogLoader.Unload(uiDialog.DialogController!);
      }
    }

    public void MuteDialogs(List<Type>? excludedDialogs = null)
    {
      _muteExcludedTypes = excludedDialogs ?? new List<Type>();

      foreach (DialogData dialog in _mutedDialogs) {
        if (!_muteExcludedTypes.Contains(dialog.DialogType)) {
          continue;
        }
        ShowModal(dialog.DialogType, dialog.Params);
        _mutedDialogs.Remove(dialog);
      }

      _dialogStack.Mute(_muteExcludedTypes);
    }

    public void UnmuteDialogs()
    {
      _muteExcludedTypes = null;
      _dialogStack.Unmute();

      foreach (DialogData dialog in _mutedDialogs.ToList()) {
        DoUnmute(dialog).Forget();
        _mutedDialogs.Remove(dialog);
      }

      if (_dialogStack.IsEmpty) {
        TryShowDeferred();
      }

      async UniTaskVoid DoUnmute(DialogData dialogData)
      {
        try {
          MonoBehaviour dialog = await ShowModalAsync(dialogData.DialogType, dialogData.Params);
          if (dialogData.ShowCompletionSource == null) {
            _logger.Warn("Mute dialog doesn't have completion source");
            return;
          }
          dialogData.ShowCompletionSource.TrySetResult(dialog);
        }
        catch (Exception e) {
          if (dialogData.ShowCompletionSource == null) {
            _logger.Warn("Mute dialog doesn't have completion source");
            return;
          }
          dialogData.ShowCompletionSource.TrySetException(e);
        }
      }
    }

    public bool HasDialog<T>()
        where T : MonoBehaviour
    {
      return HasDialog(typeof(T));
    }

    public bool HasDialog(Type controllerType)
    {
      return _dialogStack.FindDialogByType(controllerType) != null;
    }

    public bool HasAnyDialog()
    {
      return !_dialogStack.IsEmpty;
    }

    public bool IsDialogTop(Type dialogType)
    {
      return _dialogStack.IsDialogTop(dialogType);
    }

    public bool IsDialogOpened(Type dialogType)
    {
      UIDialog? uiDialog = _dialogStack.FindDialogByType(dialogType);
      return uiDialog != null && uiDialog.Opened && !uiDialog.Hiding;
    }

    private void TryShowDeferred()
    {
      if (_deferredDialogs.Count <= 0) {
        return;
      }
      foreach (DialogData dialog in _deferredDialogs) {
        if (_muteExcludedTypes != null && !_muteExcludedTypes.Contains(dialog.DialogType)) {
          continue;
        }

        _deferredDialogs.Remove(dialog);
        ShowModal(dialog.DialogType, dialog.Params);
        return;
      }
    }

    public GameObject DialogContainer => _dialogContainer;
  }
}
