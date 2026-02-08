using Cysharp.Threading.Tasks;
using DG.Tweening;
using LiteUI.Common.Extensions;
using UnityEngine;
using VContainer;

namespace Project.Unity.Unity.Prepare
{
    /// <summary>
    /// Только анимации. Всё что связано с DOTween — здесь.
    /// </summary>
    public sealed class PrepareAnimationPlayer
    {
        private const float SPAWN_DURATION = 0.2f;
        
        private static readonly Ease _spawnEase = Ease.OutBack;

        public static async UniTask PlaySpawnAsync(GameObject obj)
        {
            if (obj.IsNullOrDestroyed())
            {
                return;
            }
            obj.transform.localScale = Vector3.zero;
            await obj.transform
                .DOScale(Vector3.one, SPAWN_DURATION)
                .SetEase(_spawnEase)
                .AsyncWaitForCompletion()
                .AsUniTask();
        }
    }
}
