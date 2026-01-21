using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Project.Gameplay.Presentations;
using UnityEngine;

namespace Project.Gameplay.Gameplay.Board.Appear.Strategies
{
    public interface IBoardAppearAnimationStrategy
    {
        string Id { get; }
        UniTask Appear(IReadOnlyList<EntityLink> cells);
    }
}