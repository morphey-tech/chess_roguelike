using Cysharp.Threading.Tasks;
using DG.Tweening;
using Project.Gameplay.Presentations;
using UnityEngine;

namespace Project.Unity.Unity.Views.Presentations
{
    public class FigureSpawnPresenter : MonoBehaviour, IPresenter
    {
        [Header("Spawn")]
        [SerializeField] private float _spawnDuration = 0.3f;
        [SerializeField] private Ease _spawnEase = Ease.OutBack;

        public UniTask Init(EntityLink link)
        {
            return UniTask.CompletedTask;
        }

        public async UniTaskVoid PlaySpawnAsync()
        {
            await transform
                .DOScale(Vector3.one, _spawnDuration)
                .SetEase(_spawnEase)
                .AsyncWaitForCompletion();
        }
        
    }
}
