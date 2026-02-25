namespace Project.Gameplay.Gameplay.Combat.Visual
{
    /// <summary>
    /// Domain-side visual event. Planner сортирует по Stage, затем OrderInStage.
    /// Melee: PreHit(Attack) → Hit(Damage) → [Death, Loot при убийстве].
    /// Ranged: PreHit(Fly) → Hit(Impact=0, Cleanup=1, ProjectileHitApply=2) → при убийстве дописываются Death, Loot.
    /// </summary>
    public interface ICombatVisualEvent
    {
        CombatVisualStage Stage { get; }
        int OrderInStage { get; }
    }
}