using System.Collections.Generic;
using System.Linq;
using Project.Core.Core.World;
using UnityEngine;
using static LiteUI.Common.Preconditions;

namespace Project.Unity.Unity.World
{
    /// <summary>
    /// Коллектор объектов мира + реализация IWorldRoot для Gameplay слоя.
    /// </summary>
    public class WorldObjectCollector : MonoBehaviour, IWorldRoot
    {
        [Header("Корневые контейнеры для спавна")]
        [SerializeField] private Transform _boardRoot;
        [SerializeField] private Transform _figureRoot;
        [SerializeField] private Transform _prepareRoot;
        [SerializeField] private Transform _effectsRoot;

        Transform IWorldRoot.BoardRoot => _boardRoot != null ? _boardRoot : transform;
        Transform IWorldRoot.FigureRoot => _figureRoot != null ? _figureRoot : transform;
        Transform IWorldRoot.PrepareRoot => _prepareRoot != null ? _prepareRoot : transform;
        Transform IWorldRoot.EffectsRoot => _effectsRoot != null ? _effectsRoot : transform;
        
        public T? GetObjectByType<T>(bool includeInactive = false)
            where T : class
        {
            return gameObject.GetComponentInChildren<T>(includeInactive);
        }

        public T RequireObjectByType<T>(bool includeInactive = false)
            where T : class
        {
            return CheckNotNull(gameObject.GetComponentInChildren<T>(includeInactive));
        }

        public List<T> GetObjectsByType<T>(bool includeInactive = false)
        {
            return gameObject.GetComponentsInChildren<T>(includeInactive).ToList();
        }

        public GameObject? GetObjectByName(string objectName, bool includeInactive = false)
        {
            Transform[] transforms = WorldContainer.transform.GetComponentsInChildren<Transform>(includeInactive);
            return transforms.FirstOrDefault(t => t.name == objectName)?.gameObject;
        }

        public GameObject RequireObjectByName(string objectName, bool includeInactive = false)
        {
            return CheckNotNull(GetObjectByName(objectName, includeInactive))!;
        }

        public GameObject WorldContainer => gameObject;
    }
}
