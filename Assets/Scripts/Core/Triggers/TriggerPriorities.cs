namespace Project.Core.Core.Triggers
{
    public static class TriggerPriorities
    {
        public const int Critical = -100;  // Cancel death, revive
        public const int High = -50;       // Damage modification, dodge
        public const int Normal = 0;       // Most effects
        public const int Low = 50;         // Buffs, shields
        public const int Cleanup = 100;    // Rewards, cleanup
    }
}