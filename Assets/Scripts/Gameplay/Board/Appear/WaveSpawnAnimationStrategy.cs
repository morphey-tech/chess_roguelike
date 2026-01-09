using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Project.Gameplay.Gameplay.Board.Appear
{
    public class WaveSpawnAnimationStrategy: IBoardAppearAnimationStrategy
    {
        public string Id => "wave";

        public async UniTask Appear(IReadOnlyList<GameObject> cells)
        {
            IReadOnlyList<GameObject> ordered =
                cells.OrderBy(c => 
                        c.transform.position.x + c.transform.position.z)
                    .ToList();

            foreach (GameObject cell in ordered)
            {
                Animate(cell);
                await UniTask.Delay(25);
            }
        }

        private static void Animate(GameObject cell)
        {
            Transform t = cell.transform;
            t.localScale = Vector3.zero;
            t.DOScale(Vector3.one, 0.25f)
                .SetEase(Ease.OutBack);
        }
    }
}