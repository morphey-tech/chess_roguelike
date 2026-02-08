namespace Project.Gameplay.Gameplay.Combat.Damage
{
    public sealed class DamageResult
    {
        public int Raw { get; }
        public int Final { get; }
        public bool Blocked { get; }
        public bool Dodged { get; }

        public DamageResult(int raw, int final, bool blocked, bool dodged)
        {
            Raw = raw;
            Final = final;
            Blocked = blocked;
            Dodged = dodged;
        }
    }
}
