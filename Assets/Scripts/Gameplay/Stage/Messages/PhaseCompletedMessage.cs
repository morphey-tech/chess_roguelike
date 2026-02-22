namespace Project.Gameplay.Gameplay.Stage.Messages
{
    /// <summary>
    /// Сообщение о завершении фазы стейджа
    /// </summary>
    public readonly struct PhaseCompletedMessage
    {
        public readonly string PhaseId;

        public PhaseCompletedMessage(string phaseId)
        {
            PhaseId = phaseId;
        }
    }
}
