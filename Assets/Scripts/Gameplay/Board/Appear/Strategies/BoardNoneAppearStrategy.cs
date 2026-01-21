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
            return UniTask.CompletedTask;
        }
    }
}