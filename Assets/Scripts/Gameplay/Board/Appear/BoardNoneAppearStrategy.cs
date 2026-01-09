using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Project.Gameplay.Gameplay.Board.Appear
{
    public class BoardNoneAppearStrategy : IBoardAppearAnimationStrategy
    {
        public string Id => "none";

        public UniTask Appear(IReadOnlyList<GameObject> cells)
        {
            return UniTask.CompletedTask;
        }
    }
}