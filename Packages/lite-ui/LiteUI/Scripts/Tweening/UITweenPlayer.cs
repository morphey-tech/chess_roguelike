using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using LiteUI.Common.Extensions;
using LiteUI.Tweening.Config;
using UnityEngine;
using UnityEngine.UI;
using PlayMode = LiteUI.Tweening.Config.PlayMode;
using TweenType = LiteUI.Tweening.Config.TweenType;

namespace LiteUI.Tweening
{
    /// <summary>
    /// Проигрыватель UI анимаций на основе конфигов.
    /// Не требует MonoBehaviour — можно создать из любого места.
    /// </summary>
    public class UITweenPlayer
    {
        private readonly GameObject _target;
        private readonly RectTransform _rectTransform;
        private readonly CanvasGroup _canvasGroup;
        private readonly Graphic _graphic;
        
        private Sequence? _currentSequence;
        private bool _isPlaying;

        /// <summary>
        /// Создать плеер для указанного GameObject
        /// </summary>
        /// <param name="target">Целевой UI объект</param>
        /// <param name="autoAddCanvasGroup">Автоматически добавить CanvasGroup если нет</param>
        public UITweenPlayer(GameObject target, bool autoAddCanvasGroup = true)
        {
            _target = target;
            _rectTransform = target.GetComponent<RectTransform>();
            _graphic = target.GetComponent<Graphic>();
            
            _canvasGroup = target.GetComponent<CanvasGroup>();
            if (_canvasGroup == null && autoAddCanvasGroup)
            {
                _canvasGroup = target.AddComponent<CanvasGroup>();
            }
        }

        /// <summary>
        /// Воспроизвести анимацию из конфига
        /// </summary>
        public async UniTask Play(UITweenConfig config)
        {
            if (config == null || config.Animations.Count == 0)
            {
                return;
            }

            Stop();
            
            _isPlaying = true;
            _currentSequence = DOTween.Sequence();
            
            if (config.StartDelay > 0)
            {
                _currentSequence.AppendInterval(config.StartDelay);
            }

            foreach (var animData in config.Animations)
            {
                Tween? tween = CreateTween(animData);
                if (tween.IsNullOrDestroyed())
                {
                    continue;
                }
                if (config.PlayMode == PlayMode.Parallel)
                {
                    _currentSequence.Join(tween);
                }
                else
                {
                    _currentSequence.Append(tween);
                }
            }

            try
            {
                await _currentSequence.AsyncWaitForCompletion();
            }
            catch (Exception)
            {
                // Sequence was killed
            }
            finally
            {
                _isPlaying = false;
            }
        }

        /// <summary>
        /// Воспроизвести несколько конфигов последовательно
        /// </summary>
        public async UniTask PlaySequence(params UITweenConfig[] configs)
        {
            foreach (var config in configs)
            {
                await Play(config);
            }
        }

        /// <summary>
        /// Остановить текущую анимацию
        /// </summary>
        /// <param name="complete">Завершить до конца или просто остановить</param>
        public void Stop(bool complete = false)
        {
            if (_currentSequence != null && _currentSequence.IsActive())
            {
                _currentSequence.Kill(complete);
            }
            _currentSequence = null;
            _isPlaying = false;
        }

        /// <summary>
        /// Анимация сейчас проигрывается
        /// </summary>
        public bool IsPlaying => _isPlaying;

        private Tween? CreateTween(TweenAnimationData data)
        {
            Tween? tween = data.Type switch
            {
                TweenType.Fade => CreateFadeTween(data),
                TweenType.Scale => CreateScaleTween(data),
                TweenType.ScaleUniform => CreateScaleUniformTween(data),
                TweenType.Move => CreateMoveTween(data),
                TweenType.Rotate => CreateRotateTween(data),
                TweenType.Size => CreateSizeTween(data),
                TweenType.Color => CreateColorTween(data),
                TweenType.PunchScale => CreatePunchScaleTween(data),
                TweenType.PunchPosition => CreatePunchPositionTween(data),
                TweenType.ShakePosition => CreateShakePositionTween(data),
                TweenType.ShakeRotation => CreateShakeRotationTween(data),
                _ => null
            };

            if (tween != null)
            {
                tween.SetDelay(data.Delay);
                tween.SetEase(data.Ease);
                
                if (data.IsRelative && !IsPunchOrShake(data.Type))
                {
                    tween.SetRelative();
                }
            }

            return tween;
        }

        private Tween? CreateFadeTween(TweenAnimationData data)
        {
            if (_canvasGroup == null)
            {
                return null;
            }

            float targetValue = data.Value.FloatValue;
            
            if (data.UseFrom)
            {
                return _canvasGroup.DOFade(targetValue, data.Duration).From(targetValue).SetTarget(_target);
            }
            return _canvasGroup.DOFade(targetValue, data.Duration).SetTarget(_target);
        }

        private Tween CreateScaleTween(TweenAnimationData data)
        {
            Vector3 targetValue = data.Value.Vector3Value;
            
            if (data.UseFrom)
            {
                return _target.transform.DOScale(targetValue, data.Duration).From(targetValue);
            }
            return _target.transform.DOScale(targetValue, data.Duration);
        }

        private Tween CreateScaleUniformTween(TweenAnimationData data)
        {
            float targetValue = data.Value.FloatValue;
            Vector3 scaleVector = Vector3.one * targetValue;
            
            if (data.UseFrom)
            {
                return _target.transform.DOScale(scaleVector, data.Duration).From(scaleVector);
            }
            return _target.transform.DOScale(scaleVector, data.Duration);
        }

        private Tween? CreateMoveTween(TweenAnimationData data)
        {
            if (_rectTransform == null)
            {
                return null;
            }

            Vector2 targetValue = data.Value.Vector2Value;
            if (data.UseFrom)
            {
                return _rectTransform.DOAnchorPos(targetValue, data.Duration).From(targetValue);
            }
            return _rectTransform.DOAnchorPos(targetValue, data.Duration);
        }

        private Tween CreateRotateTween(TweenAnimationData data)
        {
            Vector3 targetValue = data.Value.Vector3Value;
            
            if (data.UseFrom)
            {
                return _target.transform.DOLocalRotate(targetValue, data.Duration).From(targetValue);
            }
            return _target.transform.DOLocalRotate(targetValue, data.Duration);
        }

        private Tween? CreateSizeTween(TweenAnimationData data)
        {
            if (_rectTransform == null)
            {
                return null;
            }

            Vector2 targetValue = data.Value.Vector2Value;
            return data.UseFrom 
                ? _rectTransform.DOSizeDelta(targetValue, data.Duration).From(targetValue) 
                : _rectTransform.DOSizeDelta(targetValue, data.Duration);
        }

        private Tween? CreateColorTween(TweenAnimationData data)
        {
            if (_graphic == null)
            {
                return null;
            }

            Color targetValue = data.Value.ColorValue;
            return data.UseFrom 
                ? _graphic.DOColor(targetValue, data.Duration).From(targetValue) 
                : _graphic.DOColor(targetValue, data.Duration);
        }

        private Tween CreatePunchScaleTween(TweenAnimationData data)
        {
            return _target.transform.DOPunchScale(data.Value.Vector3Value, data.Duration);
        }

        private Tween? CreatePunchPositionTween(TweenAnimationData data)
        {
            return _rectTransform == null 
                ? null : _rectTransform.DOPunchAnchorPos(data.Value.Vector2Value, data.Duration);
        }

        private Tween? CreateShakePositionTween(TweenAnimationData data)
        {
            return _rectTransform == null 
                ? null : _rectTransform.DOShakeAnchorPos(data.Duration, data.Value.Vector2Value);
        }

        private Tween CreateShakeRotationTween(TweenAnimationData data)
        {
            return _target.transform.DOShakeRotation(data.Duration, data.Value.Vector3Value);
        }

        private static bool IsPunchOrShake(TweenType type)
        {
            return type is TweenType.PunchScale or TweenType.PunchPosition 
                       or TweenType.ShakePosition or TweenType.ShakeRotation;
        }
    }
}

