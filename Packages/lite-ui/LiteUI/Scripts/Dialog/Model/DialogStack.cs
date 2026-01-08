using System;
using System.Collections.Generic;
using System.Linq;
using LiteUI.Dialog.Controllers;
using UnityEngine;
using UnityEngine.UI;

namespace LiteUI.Dialog.Model
{
    internal class DialogStack
    {
        private readonly List<UIDialog> _dialogStack = new();

        public void Add(UIDialog uiDialog)
        {
            _dialogStack.Add(uiDialog);
        }

        public void Remove(UIDialog uiDialog)
        {
            _dialogStack.Remove(uiDialog);
        }
        
        public void Mute(List<Type> excludedDialogs)
        {
            foreach (UIDialog uiDialog in _dialogStack) {
                if (uiDialog.DialogType != null && !excludedDialogs.Contains(uiDialog.DialogType)) {
                    uiDialog.Muted = true;
                } else {
                    uiDialog.Muted = false;
                }
            }
        }

        public void Unmute()
        {
            _dialogStack.ForEach(d => d.Muted = false);
        }

        public bool Contains(UIDialog uiDialog)
        {
            return _dialogStack.Contains(uiDialog);
        }

        public void SortDialogs(DialogInputLock dialogInputLock, Image dialogBackgroundShade)
        {
            if (_dialogStack.Count == 0) {
                return;
            }
            for (int i = 0; i < _dialogStack.Count - 1; i++) {
                UIDialog uiDialog = _dialogStack[i];
                if (uiDialog.DialogController != null) {
                    uiDialog.DialogController.transform.SetSiblingIndex(i);
                }
            }
            dialogBackgroundShade.transform.SetSiblingIndex(0);
            dialogInputLock.transform.SetSiblingIndex(1);
            UIDialog lastDialog = _dialogStack.Last();
            if (lastDialog.DialogController != null) {
                lastDialog.DialogController.transform.SetSiblingIndex(_dialogStack.Count + 1);
            }
        }

        public UIDialog? FindDialogByType(Type dialogType)
        {
            return _dialogStack.FirstOrDefault(d => d.DialogType == dialogType);
        }

        public UIDialog? FindDialogForObject(GameObject dialog)
        {
            return _dialogStack.FirstOrDefault(d => d.DialogController != null && ReferenceEquals(d.DialogController.gameObject, dialog));
        }

        public bool IsDialogTop(Type dialogType)
        {
            return _dialogStack.Count > 0 && _dialogStack.Last().DialogType == dialogType;
        }

        public bool IsEmpty => _dialogStack.Count == 0;

        public List<UIDialog> Items => _dialogStack.ToList();
    }
}
