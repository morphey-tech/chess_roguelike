using System;
using System.Collections.Generic;
using Project.Core.Window;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Gameplay.UI
{
    public class TooltipWindow : ParameterlessWindow
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI _contentText;
        [SerializeField] private RectTransform _rootRect;

        [Header("Settings")]
        [SerializeField] private float _paddingX = 10f;
        [SerializeField] private float _paddingY = 8f;
        [SerializeField] private float _maxWidth = 300f;
        [SerializeField] private float _minWidth = 150f;
        [SerializeField] private float _cursorOffset = 10f;
        [SerializeField] private TMP_FontAsset[] _fallbackFontAssets = Array.Empty<TMP_FontAsset>();

        private RectTransform _rectTransform;

        protected override void OnInit()
        {
            base.OnInit();
            _rectTransform = GetComponent<RectTransform>();

            if (_fallbackFontAssets is { Length: > 0 })
            {
                _contentText.font.fallbackFontAssetTable = new List<TMP_FontAsset>(_fallbackFontAssets);
            }
        }

        public void Show(string content, Vector2 position)
        {
            if (_contentText == null || _rootRect == null)
            {
                return;
            }

            _contentText.text = content;
            _contentText.gameObject.SetActive(true);
            LayoutRebuilder.ForceRebuildLayoutImmediate(_rootRect);

            Vector2 preferredSize = _contentText.GetPreferredValues();

            float targetWidth = Mathf.Max(preferredSize.x + (_paddingX * 2), _minWidth);
            targetWidth = Mathf.Min(targetWidth, _maxWidth);
            float targetHeight = preferredSize.y + (_paddingY * 2);

            if (preferredSize.x + (_paddingX * 2) > _maxWidth)
            {
                _contentText.rectTransform.sizeDelta = new Vector2(_maxWidth - (_paddingX * 2), 0);
                LayoutRebuilder.ForceRebuildLayoutImmediate(_rootRect);
                preferredSize = _contentText.GetPreferredValues();
                targetWidth = _maxWidth;
                targetHeight = preferredSize.y + (_paddingY * 2);
            }

            targetWidth = Mathf.Max(targetWidth, _minWidth);
            targetHeight = Mathf.Max(targetHeight, 30f);

            _rootRect.sizeDelta = new Vector2(targetWidth, targetHeight);

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _rectTransform,
                    position,
                    null,
                    out Vector2 localPosition))
            {
                float halfWidth = targetWidth * 0.5f;
                float halfHeight = targetHeight * 0.5f;

                bool isLeftSide = position.x < Screen.width * 0.5f;

                float offsetX = isLeftSide ? _cursorOffset + halfWidth : -(_cursorOffset + halfWidth);
                float offsetY = -halfHeight;

                Vector2 targetPosition = localPosition + new Vector2(offsetX, offsetY);

                float canvasHalfWidth = _rectTransform.rect.width * 0.5f;
                float canvasHalfHeight = _rectTransform.rect.height * 0.5f;

                if (targetPosition.x - halfWidth < -canvasHalfWidth)
                {
                    targetPosition.x = -canvasHalfWidth + halfWidth + 5;
                }

                if (targetPosition.x + halfWidth > canvasHalfWidth)
                {
                    targetPosition.x = canvasHalfWidth - halfWidth - 5;
                }

                if (targetPosition.y - halfHeight < -canvasHalfHeight)
                {
                    targetPosition.y = -canvasHalfHeight + halfHeight + 5;
                }

                if (targetPosition.y + halfHeight > canvasHalfHeight)
                {
                    targetPosition.y = canvasHalfHeight - halfHeight - 5;
                }

                _rootRect.anchoredPosition = targetPosition;
            }

            Show();
        }
    }
}
