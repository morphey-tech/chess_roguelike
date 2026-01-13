using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer.Unity;

namespace Project.Gameplay.Gameplay.Memory
{
    public sealed class MemoryCleanService : IInitializable, IDisposable
    {
        private bool _isCleaning;
        
        void IInitializable.Initialize()
        {
            Application.lowMemory += OnMemoryIsLow;
        }

        void IDisposable.Dispose()
        {
            Application.lowMemory -= OnMemoryIsLow;
        }

        private void OnMemoryIsLow()
        {
            CleanMemory().Forget();
        }

        public async UniTask CleanMemory()
        {
            if (_isCleaning)
            {
                return;
            }

            _isCleaning = true;

            try
            {
                await UniTask.NextFrame();
                await Resources.UnloadUnusedAssets();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            finally
            {
                _isCleaning = false;
            }
        }
    }
}
