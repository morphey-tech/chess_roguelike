using Project.Gameplay.Gameplay.Stage;

namespace Project.Gameplay.Gameplay.Stage.Messages
{
    /// <summary>
    /// Сообщения о событиях стадий и фаз (старт/завершение стадии/фазы).
    /// </summary>
    public readonly struct StagePhaseMessage
    {
        public const string STAGE_STARTED = "stageStarted";
        public const string STAGE_COMPLETED = "stageCompleted";
        public const string PHASE_STARTED = "phaseStarted";
        public const string PHASE_COMPLETED = "phaseCompleted";

        public readonly string Type;
        public readonly string StageId;
        public readonly string PhaseId;
        public readonly int StageIndex;
        public readonly int PhaseIndex;
        public readonly StageResult Result;

        private StagePhaseMessage(
            string type,
            string stageId,
            string phaseId,
            int stageIndex,
            int phaseIndex,
            StageResult result)
        {
            Type = type;
            StageId = stageId;
            PhaseId = phaseId;
            StageIndex = stageIndex;
            PhaseIndex = phaseIndex;
            Result = result;
        }

        public static StagePhaseMessage StageStarted(string stageId, int stageIndex)
        {
            return new StagePhaseMessage(STAGE_STARTED, stageId, string.Empty, stageIndex, 0, null);
        }

        public static StagePhaseMessage StageCompleted(string stageId, StageResult result)
        {
            return new StagePhaseMessage(STAGE_COMPLETED, stageId, string.Empty, 0, 0, result);
        }

        public static StagePhaseMessage PhaseStarted(string phaseId, int phaseIndex, string stageId)
        {
            return new StagePhaseMessage(PHASE_STARTED, stageId, phaseId, 0, phaseIndex, null);
        }

        public static StagePhaseMessage PhaseCompleted(string phaseId)
        {
            return new StagePhaseMessage(PHASE_COMPLETED, string.Empty, phaseId, 0, 0, null);
        }
    }
}
