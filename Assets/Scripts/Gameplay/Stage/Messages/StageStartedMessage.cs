namespace Project.Gameplay.Gameplay.Stage.Messages
{
    /// <summary>
    /// Сообщение о начале стадии
    /// </summary>
    public readonly struct StageStartedMessage
    {
        public readonly string StageId;
        public readonly int StageIndex;

        public StageStartedMessage(string stageId, int stageIndex)
        {
            StageId = stageId;
            StageIndex = stageIndex;
        }
    }
}
