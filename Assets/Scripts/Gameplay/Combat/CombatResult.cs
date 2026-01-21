namespace Project.Gameplay.Gameplay.Combat
{
    public struct CombatResult
    {
        public int DamageDealt { get; set; }
        public bool TargetDied { get; set; }
        public int HealedAmount { get; set; }
        public bool AttackerMoves { get; set; }
        public bool WasCritical { get; set; }
    }
}
