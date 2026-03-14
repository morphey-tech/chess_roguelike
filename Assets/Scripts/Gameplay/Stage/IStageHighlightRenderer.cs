using System.Collections.Generic;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Stage.Analysis;

namespace Project.Gameplay.Gameplay.Stage
{
    public interface IStageHighlightRenderer
    {
        void Show(StageActorAnalysis analysis);
        void ShowMovesOnly(IReadOnlyCollection<GridPosition> moveTargets);
        void Clear();
    }
}
