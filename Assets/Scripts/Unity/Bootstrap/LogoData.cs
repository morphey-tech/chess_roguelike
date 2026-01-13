using System;
using UnityEngine;

namespace Project.Unity.Unity.Bootstrap
{
    /// <summary>
    /// Данные одного логотипа
    /// </summary>
    [Serializable]
    public class LogoData
    {
        [Tooltip("Название (для отладки)")]
        public string Name = "Logo";

        [Tooltip("Спрайт логотипа")]
        public Sprite Sprite;

        [Tooltip("Звук при показе")]
        public AudioClip Sound;

        [Header("Переопределение тайминга")]
        [Tooltip("Использовать свои тайминги вместо дефолтных")]
        public bool OverrideTiming;
        
        [Tooltip("Длительность появления")]
        public float FadeInDuration = 0.5f;
        
        [Tooltip("Длительность показа")]
        public float DisplayDuration = 2f;
        
        [Tooltip("Длительность исчезновения")]
        public float FadeOutDuration = 0.5f;

        [Header("Переопределение цвета")]
        [Tooltip("Использовать свой цвет")]
        public bool OverrideColor;
        
        [Tooltip("Цвет изображения")]
        public Color Color = Color.white;
    }
}