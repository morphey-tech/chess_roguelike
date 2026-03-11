using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Project.Gameplay.Gameplay.UI
{
    /// <summary>
    /// Сервис для создания и управления UI-элементами с автоматическим инжектом зависимостей.
    /// Вдохновлён LiteUI.UIService.
    /// </summary>
    public interface IUIAssetService
    {
        /// <summary>
        /// Загружает префаб из Addressables (не инстанцирует).
        /// </summary>
        UniTask<GameObject> LoadPrefabAsync(string address, CancellationToken cancellationToken = default);

        /// <summary>
        /// Синхронное создание UI-элемента из префаба с инжектом зависимостей.
        /// </summary>
        T Instantiate<T>(T prefab, Transform parent) where T : Component;

        /// <summary>
        /// Синхронное создание UI-элемента из префаба с позицией и ротацией.
        /// </summary>
        T Instantiate<T>(T prefab, Vector3 position, Quaternion rotation, Transform parent) where T : Component;

        /// <summary>
        /// Инстанцирует префаб напрямую (для префабов из Inspector/Resources).
        /// </summary>
        GameObject? InstantiatePrefabDirectly(GameObject prefab, Vector3 position, Quaternion rotation, Transform? parent = null);

        /// <summary>
        /// Синхронное создание GameObject из префаба с инжектом зависимостей.
        /// </summary>
        GameObject Instantiate(GameObject prefab, Transform parent);

        /// <summary>
        /// Асинхронное создание UI-элемента по адресу Addressables с инжектом зависимостей.
        /// </summary>
        UniTask<T> CreateAsync<T>(string address, Transform? parent = null, CancellationToken cancellationToken = default) where T : MonoBehaviour;

        /// <summary>
        /// Асинхронное создание UI-элемента по адресу с параметрами инициализации.
        /// </summary>
        UniTask<T> CreateAsync<T>(string address, object[]? initParams, Transform? parent = null, CancellationToken cancellationToken = default) where T : MonoBehaviour;

        /// <summary>
        /// Асинхронное создание коллекции UI-элементов.
        /// </summary>
        UniTask<List<T>> CreateCollectionAsync<T>(string address, List<object[]>? itemsParams, Transform? parent = null, CancellationToken cancellationToken = default) where T : MonoBehaviour;

        /// <summary>
        /// Освобождает экземпляр UI-элемента.
        /// </summary>
        void Release(GameObject instance);

        /// <summary>
        /// Очищает кеш префабов.
        /// </summary>
        void FlushCache();

        /// <summary>
        /// Прикрепляет контроллер к существующему GameObject.
        /// </summary>
        T AttachController<T>(GameObject uiObject, params object?[]? initParams) where T : MonoBehaviour;
    }
}