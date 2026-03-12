namespace Project.Gameplay.Gameplay.Stage.Messages
{
    /// <summary>
    /// Сообщение от консольных команд для принудительного завершения стадии.
    /// </summary>
    public readonly struct ForceStageEndMessage
    {
        public readonly StageOutcome Outcome;

        public ForceStageEndMessage(StageOutcome outcome)
        {
            Outcome = outcome;
        }
    }
}
