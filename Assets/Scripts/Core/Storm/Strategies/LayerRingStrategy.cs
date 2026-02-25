using System.Collections.Generic;
using Project.Core.Core.Grid;
using Project.Core.Core.Storm.Core;

namespace Project.Core.Core.Storm.Strategies
{
    /// <summary>
    /// Стратегия послойного сужения зоны
    /// 
    /// Алгоритм:
    /// 1. Сужение происходит слоями от краёв к центру
    /// 2. Каждый слой имеет 4 шага:
    ///    - Step 0: Warning rows (верхний и нижний ряды предупреждения)
    ///    - Step 1: Danger rows + Warning cols (ряды опасные, колонки предупреждения)
    ///    - Step 2: Danger rows + Danger cols (все клетки опасные)
    ///    - Step 3: Переход к следующему слою
    /// </summary>
    public sealed class LayerRingStrategy : IStormStrategy
    {
        private const int StepsPerLayer = 4;

        public IEnumerable<GridPosition> GetWarningCells(StormContext context)
        {
            List<GridPosition> positions = new();
            int layer = context.CurrentLayer;
            int step = context.StepInLayer;
            int boardSize = context.BoardSize;
            int minSize = context.SafeZoneMinSize;

            int maxLayer = (boardSize - minSize) / 2;
            if (layer > maxLayer)
            {
                yield break;
            }

            int warningLayer = step >= 3 ? layer + 1 : layer;
            if (warningLayer > maxLayer)
            {
                yield break;
            }

            int bottomRow = boardSize - 1 - warningLayer;
            int rightCol = boardSize - 1 - warningLayer;

            if (step == 0)
            {
                for (int col = warningLayer; col <= rightCol; col++)
                {
                    positions.Add(new GridPosition(warningLayer, col));
                }
                for (int col = warningLayer; col <= rightCol; col++)
                {
                    positions.Add(new GridPosition(bottomRow, col));
                }
            }
            else if (step == 1)
            {
                for (int row = warningLayer + 1; row < bottomRow; row++)
                {
                    positions.Add(new GridPosition(row, warningLayer));
                }
                for (int row = warningLayer + 1; row < bottomRow; row++)
                {
                    positions.Add(new GridPosition(row, rightCol));
                }
            }

            foreach (GridPosition pos in positions)
            {
                yield return pos;
            }
        }

        public IEnumerable<GridPosition> GetDangerCells(StormContext context)
        {
            List<GridPosition> positions = new();
            int layer = context.CurrentLayer;
            int step = context.StepInLayer;
            int boardSize = context.BoardSize;
            int minSize = context.SafeZoneMinSize;

            int maxLayer = (boardSize - minSize) / 2;
            if (layer > maxLayer)
            {
                yield break;
            }

            for (int l = 0; l <= layer; l++)
            {
                int bottomRow = boardSize - 1 - l;
                int rightCol = boardSize - 1 - l;

                if (step >= 1)
                {
                    for (int col = l; col <= rightCol; col++)
                    {
                        positions.Add(new GridPosition(l, col));
                    }
                    for (int col = l; col <= rightCol; col++)
                    {
                        positions.Add(new GridPosition(bottomRow, col));
                    }
                }

                if (step >= 2)
                {
                    for (int row = l + 1; row < bottomRow; row++)
                    {
                        positions.Add(new GridPosition(row, l));
                    }
                    for (int row = l + 1; row < bottomRow; row++)
                    {
                        positions.Add(new GridPosition(row, rightCol));
                    }
                }
            }

            foreach (GridPosition pos in positions)
            {
                yield return pos;
            }
        }

        public bool HasNextStep(StormContext context)
        {
            int maxLayer = (context.BoardSize - context.SafeZoneMinSize) / 2;
            if (context.StepInLayer < StepsPerLayer - 1)
            {
                return true;
            }
            if (context.CurrentLayer >= maxLayer)
            {
                return false;
            }
            return context.CurrentLayer < maxLayer;
        }

        public bool AdvanceStep(ref StormContext context)
        {
            int maxLayer = (context.BoardSize - context.SafeZoneMinSize) / 2;
            if (context.StepInLayer < StepsPerLayer - 1)
            {
                context.StepInLayer++;
                return true;
            }
            if (context.CurrentLayer >= maxLayer)
            {
                return false;
            }
            if (context.CurrentLayer < maxLayer)
            {
                context.CurrentLayer++;
                context.StepInLayer = 0;
                return true;
            }
            return false;
        }

        public int GetMaxStepsInLayer(StormContext context)
        {
            return StepsPerLayer;
        }
    }
}
