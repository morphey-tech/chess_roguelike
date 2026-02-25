namespace Project.Gameplay.Gameplay.Combat.Visual
{
    /// <summary>
    /// Порядок визуала: PreHit → Hit → Death → Loot → Cleanup.
    /// Внутри Stage команды сортируются по OrderInStage.
    /// </summary>
    public enum CombatVisualStage
    {
        PreHit = 0,  // melee: Attack; ranged: FlyProjectile
        Hit = 1,     // melee: Damage; ranged: Impact(0), Cleanup(1), ProjectileHitApply(2) [+ при смерти Death/Loot из очереди]
        Death = 2,   // DeathCommand (HideHpBar → PlayDeath → RemoveFigure)
        Loot = 3,    // LootCommand
        Cleanup = 4  // не используется для projectile (Cleanup в Hit)
    }
}