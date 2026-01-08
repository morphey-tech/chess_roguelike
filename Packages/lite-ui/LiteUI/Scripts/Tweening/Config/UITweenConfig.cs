using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace LiteUI.Tweening.Config
{
    /// <summary>
    /// Конфигурация анимации UI элемента.
    /// Создаётся как ScriptableObject в редакторе: Create → LiteUI → Tween Config
    /// </summary>
    [CreateAssetMenu(fileName = "UITweenConfig", menuName = "LiteUI/Tween Config")]
    public class UITweenConfig : ScriptableObject
    {
        [Tooltip("Список анимаций для воспроизведения")]
        [SerializeField] private List<TweenAnimationData> _animations = new();
        
        [Tooltip("Режим воспроизведения: параллельно или последовательно")]
        [SerializeField] private PlayMode _playMode = PlayMode.Parallel;
        
        [Tooltip("Общая задержка перед началом")]
        [SerializeField] private float _startDelay;

        public IReadOnlyList<TweenAnimationData> Animations => _animations;
        public PlayMode PlayMode => _playMode;
        public float StartDelay => _startDelay;
    }

    public enum PlayMode
    {
        /// <summary>Все анимации запускаются одновременно</summary>
        Parallel,
        /// <summary>Анимации запускаются друг за другом</summary>
        Sequence
    }

    /// <summary>
    /// Данные одной анимации
    /// </summary>
    [Serializable]
    public class TweenAnimationData
    {
        [Tooltip("Тип анимации")]
        public TweenType Type = TweenType.Fade;
        
        [Tooltip("Длительность анимации")]
        public float Duration = 0.3f;
        
        [Tooltip("Задержка перед этой анимацией")]
        public float Delay;
        
        [Tooltip("Easing функция")]
        public Ease Ease = Ease.OutQuad;
        
        [Tooltip("Использовать From (анимация ОТ значения К текущему)")]
        public bool UseFrom;
        
        [Header("Значения")]
        [Tooltip("Целевое значение (или From значение если UseFrom=true)")]
        public TweenValue Value;
        
        [Tooltip("Относительная анимация (прибавляется к текущему)")]
        public bool IsRelative;
    }

    public enum TweenType
    {
        /// <summary>Прозрачность (CanvasGroup.alpha)</summary>
        Fade,
        /// <summary>Масштаб (transform.localScale)</summary>
        Scale,
        /// <summary>Равномерный масштаб (одно значение для X, Y, Z)</summary>
        ScaleUniform,
        /// <summary>Позиция (RectTransform.anchoredPosition)</summary>
        Move,
        /// <summary>Поворот (transform.localEulerAngles)</summary>
        Rotate,
        /// <summary>Размер (RectTransform.sizeDelta)</summary>
        Size,
        /// <summary>Цвет (Graphic.color)</summary>
        Color,
        /// <summary>Punch Scale (пружинящий эффект)</summary>
        PunchScale,
        /// <summary>Punch Position</summary>
        PunchPosition,
        /// <summary>Shake Position</summary>
        ShakePosition,
        /// <summary>Shake Rotation</summary>
        ShakeRotation
    }

    /// <summary>
    /// Универсальное значение для разных типов анимаций
    /// </summary>
    [Serializable]
    public struct TweenValue
    {
        [Tooltip("Для Fade, ScaleUniform")]
        public float FloatValue;
        
        [Tooltip("Для Move, Size")]
        public Vector2 Vector2Value;
        
        [Tooltip("Для Scale, Rotate, PunchScale, ShakePosition")]
        public Vector3 Vector3Value;
        
        [Tooltip("Для Color")]
        public Color ColorValue;

        public static TweenValue FromFloat(float value) => new() { FloatValue = value };
        public static TweenValue FromVector2(Vector2 value) => new() { Vector2Value = value };
        public static TweenValue FromVector3(Vector3 value) => new() { Vector3Value = value };
        public static TweenValue FromColor(Color value) => new() { ColorValue = value };
    }
}

