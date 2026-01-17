using System;
using Cysharp.Threading.Tasks;
using Project.Core.Core.Save;
using UnityEngine;
using VContainer;

namespace Project.Gameplay.Gameplay.Save
{
    public sealed class SaveService : ISaveService
    {
        private readonly FileSaveStorage _storage;
        private readonly ISaveEnvironment _environment;
        private readonly ISaveDataProvider[] _providers;
        private readonly ISaveDataApplier[] _appliers;

        [Inject]
        private SaveService(
            FileSaveStorage storage,
            ISaveEnvironment environment,
            ISaveDataProvider[] providers,
            ISaveDataApplier[] appliers)
        {
            _storage = storage;
            _environment = environment;
            _providers = providers;
            _appliers = appliers;
        }

        public async UniTask SaveAsync(string slotId)
        {
            SaveSnapshot snapshot = new()
            {
                SlotId = slotId,
                SaveTime = DateTime.Now,
                SceneId = _environment.CurrentScene,
                Version = 1
            };

            foreach (ISaveDataProvider provider in _providers)
            {
                provider.Populate(snapshot);
            }

            string json = JsonUtility.ToJson(snapshot, true);
            await _storage.WriteAsync(slotId, json);
        }

        public async UniTask<bool> LoadAsync(string slotId)
        {
            if (!_storage.Exists(slotId))
            {
                return false;
            }

            string json = await _storage.ReadAsync(slotId);
            SaveSnapshot snapshot = JsonUtility.FromJson<SaveSnapshot>(json);

            foreach (ISaveDataApplier applier in _appliers)
            {
                applier.Apply(snapshot);
            }

            return true;
        }

        public UniTask<bool> HasSaveAsync(string slotId)
        {
            return UniTask.FromResult(_storage.Exists(slotId));
        }

        public UniTask DeleteAsync(string slotId)
        {
            _storage.Delete(slotId);
            return UniTask.CompletedTask;
        }

        public UniTask<string[]> GetSlotsAsync()
        {
            return UniTask.FromResult(_storage.GetAllSlots());
        }
    }
}


