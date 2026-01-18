using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Project.Gameplay.Gameplay.Stage;

namespace Project.Gameplay.Gameplay.Figures
{
    public sealed class EmptyFiguresSpawnProvider : IFiguresSpawnProvider
    {
        public UniTask<IReadOnlyList<FigureSpawnEntry>> BuildAsync(StageContext context)
        {
            return UniTask.FromResult<IReadOnlyList<FigureSpawnEntry>>(
                Array.Empty<FigureSpawnEntry>());
        }
    }
}
