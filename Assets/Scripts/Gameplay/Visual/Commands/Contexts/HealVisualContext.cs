namespace Project.Gameplay.Gameplay.Visual.Commands.Contexts
{
    public readonly struct HealVisualContext
    {
        public int TargetId { get; }
        public int Amount { get; }

        public HealVisualContext(int targetId, int amount = 0)
        {
            TargetId = targetId;
            Amount = amount;
        }
    }
}
