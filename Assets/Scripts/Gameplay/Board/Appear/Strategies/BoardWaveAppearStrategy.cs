using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Project.Gameplay.Presentations;
using UnityEngine;

namespace Project.Gameplay.Gameplay.Board.Appear.Strategies
{
    public class BoardWaveAppearStrategy: IBoardAppearAnimationStrategy
    {
        public string Id => "wave";

        public async UniTask Appear(IReadOnlyList<EntityLink> cells)
        {
            IReadOnlyList<EntityLink> ordered =
                cells.OrderBy(c => 
                        c.transform.position.x + c.transform.position.z)
                    .ToList();

            foreach (EntityLink cell in ordered)
            {
                Animate(cell);
                await UniTask.Delay(25);
            }
        }

        private static void Animate(EntityLink cell)
        {
            Transform t = cell.transform;
            t.localScale = Vector3.zero;
            t.DOScale(Vector3.one, 0.25f)
                .SetEase(Ease.OutBack);
        }
    }
}