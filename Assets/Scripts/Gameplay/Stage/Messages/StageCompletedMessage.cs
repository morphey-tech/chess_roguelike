namespace Project.Gameplay.Gameplay.Stage.Messages
{
    /// <summary>
    /// Published when a stage is completed (all phases finished).
    /// </summary>
    public readonly struct StageCompletedMessage
    {
        public string StageId { get; }
        public StageResult Result { get; }

        public StageCompletedMessage(string stageId, StageResult result)
        {
            StageId = stageId;
            Result = result;
        }
    }

    public enum StageResult
    {
        Victory,
        Defeat,
        Skipped
    }
}
