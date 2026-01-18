using System;
using Project.Core.Core.Configs.Stage;
using Project.Gameplay.Gameplay.Grid;
using Project.Gameplay.Gameplay.Save.Models;

namespace Project.Gameplay.Gameplay.Stage
{
    /// <summary>
    /// Shared context passed between stage phases.
    /// Created once per stage, allows phases to share data.
    /// </summary>
    public sealed class StageContext
    {
        public Stage Stage { get; }
        public StageConfig StageConfig { get; }
        public PlayerRunStateModel RunState { get; }
        public BoardGrid Grid => Stage.Grid;
        
        /// <summary>
        /// Called by phases that returned WaitForCompletion when they're ready to proceed.
        /// </summary>
        public Action<PhaseResult> CompletePhase { get; internal set; }

        public StageContext(Stage stage, StageConfig config, PlayerRunStateModel runState)
        {
            Stage = stage ?? throw new ArgumentNullException(nameof(stage));
            StageConfig = config ?? throw new ArgumentNullException(nameof(config));
            RunState = runState ?? throw new ArgumentNullException(nameof(runState));
        }
    }
}