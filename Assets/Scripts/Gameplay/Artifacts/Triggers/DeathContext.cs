namespace Project.Gameplay.Gameplay.Artifacts.Triggers
{
    public readonly struct DeathContext
    {
        public readonly int VictimId;
        public readonly int? KillerId;

        public DeathContext(int victimId, int? killerId = null)
        {
            VictimId = victimId;
            KillerId = killerId;
        }
    }
}