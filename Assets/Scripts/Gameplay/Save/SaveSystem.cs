using System;
using System.IO;
using Cysharp.Threading.Tasks;
using Project.Core.Logging;
using Project.Core.Player;
using Project.Core.Save;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using ILogger = Project.Core.Logging.ILogger;

namespace Project.Gameplay.Save
{
    public class SaveSystem : ISaveSystem, IDisposable
    {
        private const string SaveExtension = ".sav";
        
        private readonly PlayerModel _playerModel;
        private readonly string _savePath;
        private readonly ILogger _logger;
        private bool _disposed;
        
        [Inject]
        public SaveSystem(PlayerModel playerModel, ILogService logService)
        {
            _playerModel = playerModel;
            _logger = logService.CreateLogger<SaveSystem>();
            _savePath = Path.Combine(Application.persistentDataPath, "Saves");
            
            if (!Directory.Exists(_savePath))
            {
                Directory.CreateDirectory(_savePath);
            }
            
            _logger.Info($"Save path: {_savePath}");
        }
        
        public async UniTask SaveAsync(string slotId)
        {
            ThrowIfDisposed();
            
            SaveData saveData = new SaveData
            {
                SlotId = slotId,
                SaveTime = DateTime.Now,
                Player = _playerModel.ToSaveData(),
                SceneName = SceneManager.GetActiveScene().name
            };
            
            string json = JsonUtility.ToJson(saveData, true);
            string filePath = GetSaveFilePath(slotId);
            
            await File.WriteAllTextAsync(filePath, json);
            
            _logger.Info($"Saved to: {filePath}");
        }
        
        public async UniTask<bool> LoadAsync(string slotId)
        {
            ThrowIfDisposed();
            
            string filePath = GetSaveFilePath(slotId);
            
            if (!File.Exists(filePath))
            {
                _logger.Warning($"Save not found: {filePath}");
                return false;
            }
            
            string json = await File.ReadAllTextAsync(filePath);
            SaveData saveData = JsonUtility.FromJson<SaveData>(json);
            
            if (saveData.Player != null)
            {
                _playerModel.FromSaveData(saveData.Player);
            }
            
            _logger.Info($"Loaded from: {filePath}");
            return true;
        }
        
        public UniTask<bool> HasSaveAsync(string slotId)
        {
            string filePath = GetSaveFilePath(slotId);
            return UniTask.FromResult(File.Exists(filePath));
        }
        
        public UniTask DeleteSaveAsync(string slotId)
        {
            string filePath = GetSaveFilePath(slotId);
            
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.Info($"Deleted: {filePath}");
            }
            
            return UniTask.CompletedTask;
        }
        
        public UniTask<string[]> GetAllSaveSlotsAsync()
        {
            if (!Directory.Exists(_savePath))
            {
                return UniTask.FromResult(Array.Empty<string>());
            }
            
            string[] files = Directory.GetFiles(_savePath, $"*{SaveExtension}");
            string[] slots = new string[files.Length];
            
            for (int i = 0; i < files.Length; i++)
            {
                slots[i] = Path.GetFileNameWithoutExtension(files[i]);
            }
            
            return UniTask.FromResult(slots);
        }
        
        private string GetSaveFilePath(string slotId)
        {
            return Path.Combine(_savePath, slotId + SaveExtension);
        }
        
        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(SaveSystem));
            }
        }
        
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _logger.Info("Disposed");
        }
    }
}


