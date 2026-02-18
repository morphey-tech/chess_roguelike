namespace Project.Gameplay.Gameplay.Combat.Damage
{
    public sealed class DamageResult
    {
        public float Raw { get; }
        public float Final { get; }
        public bool Blocked { get; }
        public bool Dodged { get; }

        public DamageResult(float raw, float final, bool blocked, bool dodged)
        {
            Raw = raw;
            Final = final;
            Blocked = blocked;
            Dodged = dodged;
        }
    }
}
