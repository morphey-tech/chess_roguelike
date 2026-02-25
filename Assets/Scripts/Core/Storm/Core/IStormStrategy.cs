using System.Collections.Generic;
using Project.Core.Core.Grid;

namespace Project.Core.Core.Storm.Core
{
    public interface IStormStrategy
    {
        IEnumerable<GridPosition> GetWarningCells(StormContext context);

        IEnumerable<GridPosition> GetDangerCells(StormContext context);

        bool HasNextStep(StormContext context);

        bool AdvanceStep(ref StormContext context);

        int GetMaxStepsInLayer(StormContext context);
    }
}
