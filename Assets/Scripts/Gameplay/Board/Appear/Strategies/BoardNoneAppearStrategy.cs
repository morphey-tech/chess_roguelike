using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Project.Gameplay.Presentations;
using UnityEngine;

namespace Project.Gameplay.Gameplay.Board.Appear.Strategies
{
    public class BoardNoneAppearStrategy : IBoardAppearAnimationStrategy
    {
        public string Id => "none";

        public UniTask Appear(IReadOnlyList<EntityLink> cells)
        {
            foreach (EntityLink cell in cells)
            {
                if (cell != null)
                    cell.transform.localScale = Vector3.zero;
            }
            foreach (EntityLink cell in cells)
            {
                if (cell != null)
                    cell.transform.localScale = Vector3.one;
            }
            return UniTask.CompletedTask;
        }
    }
}