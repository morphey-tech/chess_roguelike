using System;
using Cysharp.Threading.Tasks;

namespace Project.Core.Config
{
    /// <summary>
    /// Провайдер конфигов - загружает конфиги из Addressables по типу/лейблу.
    /// В отличие от IConfigService (который хранит уже загруженные), 
    /// этот интерфейс отвечает за загрузку.
    /// </summary>
    public interface IConfigProvider
    {
        /// <summary>
        /// Загрузить конфиг по адресу в Addressables
        /// </summary>
        UniTask<T> LoadAsync<T>(string address) where T : UnityEngine.Object;
        
        /// <summary>
        /// Загрузить конфиг по типу (адрес = имя типа)
        /// </summary>
        UniTask<T> LoadAsync<T>() where T : UnityEngine.Object;
        
        /// <summary>
        /// Загрузить все конфиги с указанным лейблом
        /// </summary>
        UniTask LoadAllByLabelAsync(string label);
        
        /// <summary>
        /// Загрузить и зарегистрировать конфиг
        /// </summary>
        UniTask<T> LoadAndRegisterAsync<T>(string address = null) where T : UnityEngine.Object;
        
        /// <summary>
        /// Проверить загружен ли конфиг
        /// </summary>
        bool IsLoaded<T>() where T : class;
    }
}

