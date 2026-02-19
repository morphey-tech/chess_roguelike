namespace Project.Gameplay.Gameplay.Visual.Commands.Contexts
{
    public readonly struct DamageVisualContext
    {
        public int TargetId { get; }
        public float Amount { get; }
        public bool IsCritical { get; }
        public bool IsDodged { get; }
        public string DamageType { get; }

        public DamageVisualContext(int targetId, float amount = 0f, bool isCritical = false, 
            bool isDodged = false, string damageType = null)
        {
            TargetId = targetId;
            Amount = amount;
            IsCritical = isCritical;
            IsDodged = isDodged;
            DamageType = damageType;
        }
    }
}
