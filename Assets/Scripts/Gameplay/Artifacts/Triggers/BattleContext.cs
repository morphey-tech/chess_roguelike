namespace Project.Gameplay.Gameplay.Artifacts.Triggers
{
    public readonly struct BattleContext
    {
        public readonly int TeamId;

        public BattleContext(int teamId)
        {
            TeamId = teamId;
        }
    }
}