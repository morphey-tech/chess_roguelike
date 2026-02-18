namespace Project.Gameplay.Gameplay.Visual.Commands.Contexts
{
    public readonly struct HealVisualContext
    {
        public int TargetId { get; }
        public float Amount { get; }

        public HealVisualContext(int targetId, float amount = 0)
        {
            TargetId = targetId;
            Amount = amount;
        }
    }
}
