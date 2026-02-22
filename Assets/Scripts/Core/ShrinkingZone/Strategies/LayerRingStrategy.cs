using System.Collections.Generic;
using Project.Core.Core.Grid;
using Project.Core.Core.ShrinkingZone.Core;

namespace Project.Core.Core.ShrinkingZone.Strategies
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
    public class LayerRingStrategy : IZoneShrinkStrategy
    {
        // Количество шагов в слое перед переходом к следующему
        private const int StepsPerLayer = 4;

        public IEnumerable<GridPosition> GetWarningCells(ZoneContext context)
        {
            var positions = new List<GridPosition>();
            int layer = context.CurrentLayer;
            int step = context.StepInLayer;
            int boardSize = context.BoardSize;
            int minSize = context.SafeZoneMinSize;

            // Максимальный слой, до которого можно сужаться
            int maxLayer = (boardSize / 2) - minSize;
            if (layer > maxLayer)
                yield break;

            int topRow = layer;
            int bottomRow = boardSize - 1 - layer;
            int leftCol = layer;
            int rightCol = boardSize - 1 - layer;

            // Warning rows на шаге 0
            if (step == 0)
            {
                // Верхний ряд warning
                for (int col = leftCol; col <= rightCol; col++)
                    positions.Add(new GridPosition(topRow, col));

                // Нижний ряд warning
                for (int col = leftCol; col <= rightCol; col++)
                    positions.Add(new GridPosition(bottomRow, col));
            }
            // Warning cols на шаге 1
            else if (step == 1)
            {
                // Левая колонка warning (кроме углов, которые уже в danger rows)
                for (int row = topRow + 1; row < bottomRow; row++)
                    positions.Add(new GridPosition(row, leftCol));

                // Правая колонка warning (кроме углов)
                for (int row = topRow + 1; row < bottomRow; row++)
                    positions.Add(new GridPosition(row, rightCol));
            }

            foreach (var pos in positions)
                yield return pos;
        }

        public IEnumerable<GridPosition> GetDangerCells(ZoneContext context)
        {
            var positions = new List<GridPosition>();
            int layer = context.CurrentLayer;
            int step = context.StepInLayer;
            int boardSize = context.BoardSize;
            int minSize = context.SafeZoneMinSize;

            // Максимальный слой
            int maxLayer = (boardSize / 2) - minSize;
            if (layer > maxLayer)
                yield break;

            int topRow = layer;
            int bottomRow = boardSize - 1 - layer;
            int leftCol = layer;
            int rightCol = boardSize - 1 - layer;

            // Danger rows на шагах 1 и 2
            if (step >= 1)
            {
                // Верхний ряд danger
                for (int col = leftCol; col <= rightCol; col++)
                    positions.Add(new GridPosition(topRow, col));

                // Нижний ряд danger
                for (int col = leftCol; col <= rightCol; col++)
                    positions.Add(new GridPosition(bottomRow, col));
            }

            // Danger cols на шаге 2
            if (step >= 2)
            {
                // Левая колонка danger (кроме углов, которые уже в danger rows)
                for (int row = topRow + 1; row < bottomRow; row++)
                    positions.Add(new GridPosition(row, leftCol));

                // Правая колонка danger (кроме углов)
                for (int row = topRow + 1; row < bottomRow; row++)
                    positions.Add(new GridPosition(row, rightCol));
            }

            foreach (var pos in positions)
                yield return pos;
        }

        public bool HasNextStep(ZoneContext context)
        {
            int maxLayer = (context.BoardSize / 2) - context.SafeZoneMinSize;
            
            // Если достигнут минимальный размер, дальше сужать нельзя
            if (context.CurrentLayer >= maxLayer)
                return false;

            // Если есть следующий шаг в текущем слое
            if (context.StepInLayer < StepsPerLayer - 1)
                return true;

            // Если есть следующий слой
            return context.CurrentLayer < maxLayer;
        }

        public bool AdvanceStep(ref ZoneContext context)
        {
            int maxLayer = (context.BoardSize / 2) - context.SafeZoneMinSize;

            // Если достигнут минимальный размер
            if (context.CurrentLayer >= maxLayer)
                return false;

            // Если есть следующий шаг в текущем слое
            if (context.StepInLayer < StepsPerLayer - 1)
            {
                context.StepInLayer++;
                return true;
            }

            // Переход к следующему слою
            if (context.CurrentLayer < maxLayer)
            {
                context.CurrentLayer++;
                context.StepInLayer = 0;
                return true;
            }

            return false;
        }

        public int GetMaxStepsInLayer(ZoneContext context)
        {
            return StepsPerLayer;
        }
    }
}
