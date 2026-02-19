namespace Project.Gameplay.Gameplay.Combat.Damage
{
    public sealed class DamageResult
    {
        public float Raw { get; }
        public float Final { get; }
        public bool Blocked { get; }
        public bool Dodged { get; }
        public bool Cancelled { get; }

        public DamageResult(float raw, float final, bool blocked, bool dodged, bool cancelled)
        {
            Raw = raw;
            Final = final;
            Blocked = blocked;
            Dodged = dodged;
            Cancelled = cancelled;
        }

        public static DamageResult MakeCancelled(bool dodged = false)
        {
            return new DamageResult(0, 0, false, dodged, true);
        }
    }
}
