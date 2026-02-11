namespace Project.Gameplay.Gameplay.Stage
{
    public sealed class StageResult
    {
        public StageOutcome Outcome { get; }
        public int TurnCount { get; }
        public int EnemiesKilled { get; }

        public StageResult(StageOutcome outcome, int turnCount, int enemiesKilled)
        {
            Outcome = outcome;
            TurnCount = turnCount;
            EnemiesKilled = enemiesKilled;
        }
    }
}
