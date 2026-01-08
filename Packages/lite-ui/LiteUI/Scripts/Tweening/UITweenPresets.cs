using System.Collections.Generic;
using DG.Tweening;
using LiteUI.Tweening.Config;
using UnityEngine;
using PlayMode = LiteUI.Tweening.Config.PlayMode;
using TweenType = LiteUI.Tweening.Config.TweenType;

namespace LiteUI.Tweening
{
    /// <summary>
    /// Фабрика для создания анимаций из кода без ScriptableObject.
    /// Используй для быстрого прототипирования или динамических анимаций.
    /// </summary>
    public static class UITweenPresets
    {
        #region Fade Presets
        
        /// <summary>Fade In: alpha 0 → 1</summary>
        public static UITweenConfig FadeIn(float duration = 0.3f, Ease ease = Ease.OutQuad)
        {
            return CreateConfig(new TweenAnimationData
            {
                Type = TweenType.Fade,
                Duration = duration,
                Ease = ease,
                UseFrom = true,
                Value = TweenValue.FromFloat(0f)
            });
        }

        /// <summary>Fade Out: alpha → 0</summary>
        public static UITweenConfig FadeOut(float duration = 0.3f, Ease ease = Ease.InQuad)
        {
            return CreateConfig(new TweenAnimationData
            {
                Type = TweenType.Fade,
                Duration = duration,
                Ease = ease,
                Value = TweenValue.FromFloat(0f)
            });
        }

        #endregion

        #region Scale Presets

        /// <summary>Scale In: scale 0 → 1</summary>
        public static UITweenConfig ScaleIn(float duration = 0.3f, Ease ease = Ease.OutBack)
        {
            return CreateConfig(new TweenAnimationData
            {
                Type = TweenType.ScaleUniform,
                Duration = duration,
                Ease = ease,
                UseFrom = true,
                Value = TweenValue.FromFloat(0f)
            });
        }

        /// <summary>Scale Out: scale → 0</summary>
        public static UITweenConfig ScaleOut(float duration = 0.2f, Ease ease = Ease.InBack)
        {
            return CreateConfig(new TweenAnimationData
            {
                Type = TweenType.ScaleUniform,
                Duration = duration,
                Ease = ease,
                Value = TweenValue.FromFloat(0f)
            });
        }

        /// <summary>Scale Bounce: punch scale effect</summary>
        public static UITweenConfig ScaleBounce(float strength = 0.2f, float duration = 0.3f)
        {
            return CreateConfig(new TweenAnimationData
            {
                Type = TweenType.PunchScale,
                Duration = duration,
                Value = TweenValue.FromVector3(Vector3.one * strength)
            });
        }

        #endregion

        #region Slide Presets

        /// <summary>Slide In from Left</summary>
        public static UITweenConfig SlideInLeft(float distance = 300f, float duration = 0.3f, Ease ease = Ease.OutQuad)
        {
            return CreateConfig(new TweenAnimationData
            {
                Type = TweenType.Move,
                Duration = duration,
                Ease = ease,
                UseFrom = true,
                Value = TweenValue.FromVector2(new Vector2(-distance, 0))
            });
        }

        /// <summary>Slide In from Right</summary>
        public static UITweenConfig SlideInRight(float distance = 300f, float duration = 0.3f, Ease ease = Ease.OutQuad)
        {
            return CreateConfig(new TweenAnimationData
            {
                Type = TweenType.Move,
                Duration = duration,
                Ease = ease,
                UseFrom = true,
                Value = TweenValue.FromVector2(new Vector2(distance, 0))
            });
        }

        /// <summary>Slide In from Top</summary>
        public static UITweenConfig SlideInTop(float distance = 300f, float duration = 0.3f, Ease ease = Ease.OutQuad)
        {
            return CreateConfig(new TweenAnimationData
            {
                Type = TweenType.Move,
                Duration = duration,
                Ease = ease,
                UseFrom = true,
                Value = TweenValue.FromVector2(new Vector2(0, distance))
            });
        }

        /// <summary>Slide In from Bottom</summary>
        public static UITweenConfig SlideInBottom(float distance = 300f, float duration = 0.3f, Ease ease = Ease.OutQuad)
        {
            return CreateConfig(new TweenAnimationData
            {
                Type = TweenType.Move,
                Duration = duration,
                Ease = ease,
                UseFrom = true,
                Value = TweenValue.FromVector2(new Vector2(0, -distance))
            });
        }

        /// <summary>Slide Out to direction</summary>
        public static UITweenConfig SlideOut(Vector2 direction, float distance = 300f, float duration = 0.2f, Ease ease = Ease.InQuad)
        {
            return CreateConfig(new TweenAnimationData
            {
                Type = TweenType.Move,
                Duration = duration,
                Ease = ease,
                IsRelative = true,
                Value = TweenValue.FromVector2(direction.normalized * distance)
            });
        }

        #endregion

        #region Combined Presets

        /// <summary>Pop In: fade + scale from center</summary>
        public static UITweenConfig PopIn(float duration = 0.3f)
        {
            return CreateConfig(PlayMode.Parallel,
                new TweenAnimationData
                {
                    Type = TweenType.Fade,
                    Duration = duration,
                    Ease = Ease.OutQuad,
                    UseFrom = true,
                    Value = TweenValue.FromFloat(0f)
                },
                new TweenAnimationData
                {
                    Type = TweenType.ScaleUniform,
                    Duration = duration,
                    Ease = Ease.OutBack,
                    UseFrom = true,
                    Value = TweenValue.FromFloat(0.5f)
                }
            );
        }

        /// <summary>Pop Out: fade + scale to center</summary>
        public static UITweenConfig PopOut(float duration = 0.2f)
        {
            return CreateConfig(PlayMode.Parallel,
                new TweenAnimationData
                {
                    Type = TweenType.Fade,
                    Duration = duration,
                    Ease = Ease.InQuad,
                    Value = TweenValue.FromFloat(0f)
                },
                new TweenAnimationData
                {
                    Type = TweenType.ScaleUniform,
                    Duration = duration,
                    Ease = Ease.InBack,
                    Value = TweenValue.FromFloat(0.5f)
                }
            );
        }

        /// <summary>Slide + Fade In from direction</summary>
        public static UITweenConfig SlideAndFadeIn(Vector2 fromOffset, float duration = 0.3f)
        {
            return CreateConfig(PlayMode.Parallel,
                new TweenAnimationData
                {
                    Type = TweenType.Fade,
                    Duration = duration,
                    Ease = Ease.OutQuad,
                    UseFrom = true,
                    Value = TweenValue.FromFloat(0f)
                },
                new TweenAnimationData
                {
                    Type = TweenType.Move,
                    Duration = duration,
                    Ease = Ease.OutQuad,
                    UseFrom = true,
                    Value = TweenValue.FromVector2(fromOffset)
                }
            );
        }

        #endregion

        #region Effect Presets

        /// <summary>Shake: position shake effect</summary>
        public static UITweenConfig Shake(float strength = 10f, float duration = 0.5f)
        {
            return CreateConfig(new TweenAnimationData
            {
                Type = TweenType.ShakePosition,
                Duration = duration,
                Value = TweenValue.FromVector2(Vector2.one * strength)
            });
        }

        /// <summary>Pulse: scale punch for attention</summary>
        public static UITweenConfig Pulse(float strength = 0.1f, float duration = 0.3f)
        {
            return CreateConfig(new TweenAnimationData
            {
                Type = TweenType.PunchScale,
                Duration = duration,
                Value = TweenValue.FromVector3(Vector3.one * strength)
            });
        }

        #endregion

        #region Factory Methods

        /// <summary>
        /// Создать конфиг с одной анимацией
        /// </summary>
        public static UITweenConfig CreateConfig(TweenAnimationData animation)
        {
            return CreateConfig(PlayMode.Parallel, animation);
        }

        /// <summary>
        /// Создать конфиг с несколькими анимациями
        /// </summary>
        public static UITweenConfig CreateConfig(PlayMode playMode, params TweenAnimationData[] animations)
        {
            var config = ScriptableObject.CreateInstance<UITweenConfig>();
            
            // Используем рефлексию чтобы установить приватные поля
            var animationsField = typeof(UITweenConfig).GetField("_animations", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var playModeField = typeof(UITweenConfig).GetField("_playMode", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            animationsField?.SetValue(config, new List<TweenAnimationData>(animations));
            playModeField?.SetValue(config, playMode);
            
            return config;
        }

        #endregion
    }
}

