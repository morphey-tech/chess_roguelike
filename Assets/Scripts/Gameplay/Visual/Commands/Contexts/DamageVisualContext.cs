namespace Project.Gameplay.Gameplay.Visual.Commands.Contexts
{
    public readonly struct DamageVisualContext
    {
        public int TargetId { get; }
        public float Amount { get; }
        public bool IsCritical { get; }
        public string DamageType { get; }

        public DamageVisualContext(int targetId, float amount = 0f, bool isCritical = false, string damageType = null)
        {
            TargetId = targetId;
            Amount = amount;
            IsCritical = isCritical;
            DamageType = damageType;
        }
    }
}
