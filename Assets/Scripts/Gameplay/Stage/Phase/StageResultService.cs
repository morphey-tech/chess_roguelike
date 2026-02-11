namespace Project.Gameplay.Gameplay.Stage.Phase
{
    public sealed class StageResultService
    {
        private readonly StageEndDetector _detector;
        private readonly CombatStats _stats;

        public StageResultService(StageEndDetector detector, CombatStats stats)
        {
            _detector = detector;
            _stats = stats;
        }

        public StageResult? TryBuild()
        {
            if (_detector.IsVictory())
                return new StageResult(StageOutcome.Victory, _stats.Turns, _stats.Kills);

            if (_detector.IsDefeat())
                return new StageResult(StageOutcome.Defeat, _stats.Turns, _stats.Kills);

            return null;
        }
    }
}
