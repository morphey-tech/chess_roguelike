using LiteUI.Common.Logger;
using LiteUI.Element.Widgets;
using UnityEngine;

namespace LiteUI.Dialog.Controllers
{
    public class DialogInputLock : MonoBehaviour
    {
        private static readonly IUILogger _logger = LoggerFactory.GetLogger<DialogInputLock>();
        
        private const string INPUT_LOCK_OBJECT_NAME = "DialogInputLockFog";

        private GameObject _inputLockFog = null!;
        private int _lockCount;

        public static DialogInputLock Create(GameObject container)
        {
            GameObject lockObject = new(INPUT_LOCK_OBJECT_NAME);
            lockObject.transform.SetParent(container.transform, false);
            
            RectTransform rectTransform = lockObject.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            RaycastCatcher raycastCatcher = lockObject.AddComponent<RaycastCatcher>();
            raycastCatcher.raycastTarget = true;

            lockObject.SetActive(false);
            DialogInputLock dialogInputLock = lockObject.AddComponent<DialogInputLock>();
            dialogInputLock._inputLockFog = lockObject;
            return dialogInputLock;
        }
        
        public void LockInput()
        {
            _lockCount++;
            if (_lockCount > 1) {
                return;
            }
            _inputLockFog.SetActive(true);
        }

        public void UnlockInput()
        {
            _lockCount--;
            if (_lockCount < 0) {
                _lockCount = 0;
                _logger.Warn("Unlocked dialog input while it is not locked");
            }
            if (_lockCount == 0) {
                _inputLockFog.SetActive(false);
            }
        }
    }
}
