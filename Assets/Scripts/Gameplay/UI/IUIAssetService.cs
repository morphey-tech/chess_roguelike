using UnityEngine;

namespace Project.Gameplay.Gameplay.UI
{
    /// <summary>
    /// Фабрика для создания UI префабов с автоматическим инжектом зависимостей.
    /// </summary>
    public interface IUIAssetService
    {
        T Instantiate<T>(T prefab, Transform parent) where T : Component;
        T Instantiate<T>(T prefab, Vector3 position, Quaternion rotation, Transform parent) where T : Component;
        GameObject Instantiate(GameObject prefab, Transform parent);
    }
}