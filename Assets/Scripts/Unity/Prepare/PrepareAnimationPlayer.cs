using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using VContainer;

namespace Project.Unity.Unity.Prepare
{
    /// <summary>
    /// Только анимации. Всё что связано с DOTween — здесь.
    /// </summary>
    public sealed class PrepareAnimationPlayer
    {
        private const float SpawnDuration = 0.35f;
        private static readonly Ease SpawnEase = Ease.OutBack;

        public async UniTask PlaySpawnAsync(GameObject obj)
        {
            if (obj == null) return;
            obj.transform.localScale = Vector3.zero;
            await UniTask.Yield();
            await obj.transform
                .DOScale(Vector3.one, SpawnDuration)
                .SetEase(SpawnEase)
                .AsyncWaitForCompletion()
                .AsUniTask();
        }
    }
}
