using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Project.Gameplay.Gameplay.Board.Appear.Strategies
{
    public interface IBoardAppearAnimationStrategy
    {
        string Id { get; }
        UniTask Appear(IReadOnlyList<GameObject> cells);
    }
}