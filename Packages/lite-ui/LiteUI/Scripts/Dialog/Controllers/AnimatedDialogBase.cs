using Cysharp.Threading.Tasks;
using LiteUI.Tweening;
using LiteUI.Tweening.Config;
using UnityEngine;

namespace LiteUI.Dialog.Controllers
{
    /// <summary>
    /// Базовый класс для диалогов с анимациями на твинах.
    /// Наследуйся от этого класса вместо MonoBehaviour для диалогов с анимациями.
    /// </summary>
    public abstract class AnimatedDialogBase : MonoBehaviour, IUIAnimatable
    {
        [Header("Анимации диалога")]
        [SerializeField] protected UITweenConfig _showAnimation;
        [SerializeField] protected UITweenConfig _hideAnimation;

        private UITweenPlayer _tweenPlayer;

        protected UITweenPlayer TweenPlayer => _tweenPlayer ??= new UITweenPlayer(gameObject);

        public virtual async UniTask AnimateShow()
        {
            if (_showAnimation != null)
            {
                await TweenPlayer.Play(_showAnimation);
            }
            else
            {
                // Дефолтная анимация если конфиг не задан
                await TweenPlayer.Play(UITweenPresets.PopIn());
            }
        }

        public virtual async UniTask AnimateHide()
        {
            if (_hideAnimation != null)
            {
                await TweenPlayer.Play(_hideAnimation);
            }
            else
            {
                // Дефолтная анимация если конфиг не задан
                await TweenPlayer.Play(UITweenPresets.PopOut());
            }
        }

        /// <summary>
        /// Воспроизвести кастомную анимацию
        /// </summary>
        protected async UniTask PlayAnimation(UITweenConfig config)
        {
            if (config != null)
            {
                await TweenPlayer.Play(config);
            }
        }

        /// <summary>
        /// Воспроизвести эффект внимания (pulse)
        /// </summary>
        protected async UniTask PlayAttention()
        {
            await TweenPlayer.Play(UITweenPresets.Pulse());
        }

        /// <summary>
        /// Воспроизвести эффект тряски (shake)
        /// </summary>
        protected async UniTask PlayShake()
        {
            await TweenPlayer.Play(UITweenPresets.Shake());
        }

        protected virtual void OnDestroy()
        {
            TweenPlayer?.Stop();
        }
    }
}

