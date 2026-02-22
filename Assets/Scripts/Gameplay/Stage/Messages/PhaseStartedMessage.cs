namespace Project.Gameplay.Gameplay.Stage.Messages
{
    /// <summary>
    /// Сообщение о начале фазы
    /// </summary>
    public readonly struct PhaseStartedMessage
    {
        public readonly string PhaseId;
        public readonly int PhaseIndex;
        public readonly string StageId;

        public PhaseStartedMessage(string phaseId, int phaseIndex, string stageId)
        {
            PhaseId = phaseId;
            PhaseIndex = phaseIndex;
            StageId = stageId;
        }
    }
}
