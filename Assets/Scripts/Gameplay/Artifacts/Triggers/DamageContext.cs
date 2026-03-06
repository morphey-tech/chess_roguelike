namespace Project.Gameplay.Gameplay.Artifacts.Triggers
{
    public readonly struct DamageContext
    {
        public readonly int TargetId;
        public readonly int Amount;

        public DamageContext(int targetId, int amount)
        {
            TargetId = targetId;
            Amount = amount;
        }
    }
}