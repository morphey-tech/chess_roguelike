using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Project.Gameplay.Gameplay.Stage;

namespace Project.Gameplay.Gameplay.Figures
{
    public interface IFiguresSpawnProvider
    {
        UniTask<IReadOnlyList<FigureSpawnEntry>> BuildAsync(StageContext context);
    }
}