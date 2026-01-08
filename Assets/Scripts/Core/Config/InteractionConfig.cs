using System;
using UnityEngine;

namespace Project.Core.Config
{
    [Serializable]
    public class InteractionConfig
    {
        [Header("Дистанция")]
        public float MaxDistance = 3f;
        public float MinDistance = 0.5f;

        [Header("Угол обзора")]
        [Range(0f, 180f)]
        public float MaxAngle = 45f;

        [Header("Приоритет")]
        public bool PreferCloser = true;
        public bool PreferCentered = true;
        public float DistanceWeight = 0.5f;
        public float AngleWeight = 0.5f;

        [Header("Raycast")]
        public LayerMask InteractableLayers = ~0;
        public LayerMask ObstacleLayers = ~0;
        public bool CheckLineOfSight = true;

        [Header("Обновление")]
        public float UpdateInterval = 0.1f;
    }
}

