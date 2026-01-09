namespace Project.Gameplay.Gameplay.Stage
{
    public readonly struct StageStartedMessage
    {
        public readonly string StageId;
        public readonly string BoardId;

        public StageStartedMessage(string stageId, string boardId)
        {
            StageId = stageId;
            BoardId = boardId;
        }
    }
}
