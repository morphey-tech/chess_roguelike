namespace Project.Gameplay.Gameplay.Artifacts.Triggers
{
    public readonly struct KillContext
    {
        public readonly int KillerId;
        public readonly int VictimId;

        public KillContext(int killerId, int victimId)
        {
            KillerId = killerId;
            VictimId = victimId;
        }
    }
}