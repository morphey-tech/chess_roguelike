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

            // Максимальный слой: (boardSize - minSize) / 2
            int maxLayer = (boardSize - minSize) / 2;
            if (layer > maxLayer)
                yield break;

            // Warning клетки — это следующий слой после текущего (граница)
            // Если зона ещё не сжалась (step 0), warning на текущем слое
            // Если зона сжалась (step 1+), warning на следующем слое
            int warningLayer = step >= 3 ? layer + 1 : layer;
            
            if (warningLayer > maxLayer)
                yield break;

            int topRow = warningLayer;
            int bottomRow = boardSize - 1 - warningLayer;
            int leftCol = warningLayer;
            int rightCol = boardSize - 1 - warningLayer;

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

            // Максимальный слой: (boardSize - minSize) / 2
            int maxLayer = (boardSize - minSize) / 2;
            if (layer > maxLayer)
                yield break;

            // Danger клетки включают ВСЕ слои от 0 до текущего
            // Это означает, что внешние кольца остаются опасными при сужении
            for (int l = 0; l <= layer; l++)
            {
                int topRow = l;
                int bottomRow = boardSize - 1 - l;
                int leftCol = l;
                int rightCol = boardSize - 1 - l;

                // Danger rows на шагах 1+
                if (step >= 1)
                {
                    // Верхний ряд danger
                    for (int col = leftCol; col <= rightCol; col++)
                        positions.Add(new GridPosition(topRow, col));

                    // Нижний ряд danger
                    for (int col = leftCol; col <= rightCol; col++)
                        positions.Add(new GridPosition(bottomRow, col));
                }

                // Danger cols на шагах 2+
                if (step >= 2)
                {
                    // Левая колонка danger (кроме углов, которые уже в danger rows)
                    for (int row = topRow + 1; row < bottomRow; row++)
                        positions.Add(new GridPosition(row, leftCol));

                    // Правая колонка danger (кроме углов)
                    for (int row = topRow + 1; row < bottomRow; row++)
                        positions.Add(new GridPosition(row, rightCol));
                }
            }

            foreach (var pos in positions)
                yield return pos;
        }

        public bool HasNextStep(ZoneContext context)
        {
            // Максимальный слой: (boardSize - minSize) / 2
            int maxLayer = (context.BoardSize - context.SafeZoneMinSize) / 2;

            // Если есть следующий шаг в текущем слое (даже если это финальный слой)
            if (context.StepInLayer < StepsPerLayer - 1)
                return true;

            // Если достигнут минимальный размер и последний шаг в слое, дальше сужать нельзя
            if (context.CurrentLayer >= maxLayer)
                return false;

            // Если есть следующий слой
            return context.CurrentLayer < maxLayer;
        }

        public bool AdvanceStep(ref ZoneContext context)
        {
            // Максимальный слой: (boardSize - minSize) / 2
            int maxLayer = (context.BoardSize - context.SafeZoneMinSize) / 2;

            // Если есть следующий шаг в текущем слое (даже если это финальный слой)
            if (context.StepInLayer < StepsPerLayer - 1)
            {
                context.StepInLayer++;
                return true;
            }

            // Если достигнут минимальный размер и последний шаг в слое, дальше сужать нельзя
            if (context.CurrentLayer >= maxLayer)
                return false;

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
