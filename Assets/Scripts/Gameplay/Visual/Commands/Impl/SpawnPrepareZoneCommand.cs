using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Project.Gameplay.Gameplay.Prepare;
using Project.Gameplay.Gameplay.Visual;

namespace Project.Gameplay.Gameplay.Visual.Commands.Impl
{
    /// <summary>
    /// Команда: заспавнить зону подготовки (слоты по одному, затем фигуры по одной).
    /// </summary>
    public sealed class SpawnPrepareZoneCommand : IVisualCommand
    {
        private readonly IReadOnlyList<PrepareZoneFigureData> _figures;

        public string DebugName => $"SpawnPrepareZone(count={_figures.Count})";

        public SpawnPrepareZoneCommand(IReadOnlyList<PrepareZoneFigureData> figures)
        {
            _figures = figures;
        }

        public UniTask ExecuteAsync(IPresenterProvider presenters)
        {
            return presenters.Prepare.SpawnPrepareZoneAsync(_figures);
        }
    }
}
