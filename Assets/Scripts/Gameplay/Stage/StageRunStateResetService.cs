using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Save.Models;

namespace Project.Gameplay.Gameplay.Stage
{
    /// <summary>
    /// Encapsulates run-state mutations required before stage restart/switch.
    /// </summary>
    public sealed class StageRunStateResetService
    {
        public void ResetForStage(PlayerRunStateModel runState, string stageId)
        {
            runState.StageId = stageId;
            ResetFiguresToHand(runState);
        }

        public void ResetFiguresToHand(PlayerRunStateModel runState)
        {
            foreach (FigureState figure in runState.Figures)
                figure.Location = FigureLocation.InHand();
        }
    }
}
