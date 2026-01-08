using System;
using System.Collections.Generic;
using LiteUI.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace LiteUI.Element.Progress
{
    [ExecuteInEditMode]
    public class ColorizedStatusProgress : MonoBehaviour
    {
        private const float PROGRESS_UPDATE_STEP = 0.01f;
        
        [SerializeField]
        private Slider _slider = null!;
        [SerializeField]
        private Image _colorizedImage = null!;
        [SerializeField]
        private Image _statusImage = null!;
        [SerializeField]
        private ColorizeDictionary _colorizeDictionary = new();
        [SerializeField]
        private StatusDictionary _statusDictionary = new();
        [SerializeField]
        private Color _frozenColor = Color.cyan;

        private float _prevUpdatedProgress = -1;
        private bool _frozen;

#if UNITY_EDITOR
        private void Update()
        {
            SetProgress(_slider.value);
        }
#endif

        public void SetProgress(float progress)
        {
            if (Mathf.Abs(progress - _prevUpdatedProgress) < PROGRESS_UPDATE_STEP) {
                return;
            }
            _prevUpdatedProgress = progress;
            _slider.value = progress;
            SetStatusImage(progress);
            SetColor(progress);
        }

        public void SetFrozenState(bool frozen)
        {
            _frozen = frozen;
            SetColor(_prevUpdatedProgress);
        }

        private void SetColor(float progress)
        {
            if (_frozen) {
                _colorizedImage.color = _frozenColor;
                return;
            }
            float leftValue = float.MinValue;
            Color leftColor = Color.black;
            float rightValue = float.MaxValue;
            Color rightColor = Color.white;

            foreach (KeyValuePair<float, Color> pair in _colorizeDictionary) {
                if (leftValue < pair.Key && progress >= pair.Key) {
                    leftValue = pair.Key;
                    leftColor = pair.Value;
                }
                if (pair.Key < rightValue && progress <= pair.Key) {
                    rightValue = pair.Key;
                    rightColor = pair.Value;
                }
            }

            float part;
            if (progress >= rightValue) {
                part = 1;
            } else {
                part = (progress - leftValue) / (rightValue - leftValue);
            }
            _colorizedImage.color = Color.Lerp(leftColor, rightColor, part);
        }

        private void SetStatusImage(float progress)
        {
            float curValue = float.MinValue;
            Sprite? curSprite = null;
            foreach (KeyValuePair<float, Sprite> pair in _statusDictionary) {
                if (pair.Key < curValue || progress < pair.Key) {
                    continue;
                }
                curSprite = pair.Value;
                curValue = pair.Key;
            }
            if (curSprite == null) {
                return;
            }
            _statusImage.sprite = curSprite;
        }
    }

    [Serializable]
    public class ColorizeDictionary : SerializableDictionary<float, Color>
    {
    }

    [Serializable]
    public class StatusDictionary : SerializableDictionary<float, Sprite>
    {
    }
}
