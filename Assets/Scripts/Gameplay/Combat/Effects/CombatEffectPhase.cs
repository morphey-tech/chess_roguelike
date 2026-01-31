namespace Project.Gameplay.Gameplay.Combat.Effects
{
    /// <summary>
    /// Combat phases determine the order of effect execution.
    /// Effects within the same phase are sorted by OrderInPhase.
    /// 
    /// DESIGN NOTES:
    /// - Phases are fixed execution order, OrderInPhase is flexible within phase
    /// - New effects should fit into existing phases, not create new ones
    /// - If order matters between two effects, use OrderInPhase (0, 10, 20...)
    /// </summary>
    public enum CombatEffectPhase
    {
        /// <summary>Before any visuals (setup, validation)</summary>
        PreAnimation = 0,
        
        /// <summary>Attack animations</summary>
        Animation = 10,
        
        /// <summary>Primary damage application (main target)</summary>
        Damage = 20,
        
        /// <summary>
        /// Secondary damage and reactions (splash, pierce, thorns, bonus damage).
        /// NOTE: Lifesteal can live here with OrderInPhase if it needs to happen before thorns.
        /// Use OrderInPhase to control: lifesteal (0) -> splash (10) -> thorns (30)
        /// </summary>
        SecondaryDamage = 30,
        
        /// <summary>
        /// Healing effects (lifesteal, regen).
        /// NOTE: If heal needs to happen earlier (e.g., before thorns),
        /// place HealEffect in SecondaryDamage with appropriate OrderInPhase.
        /// </summary>
        Healing = 40,
        
        /// <summary>Movement effects (push, retreat)</summary>
        Movement = 50,
        
        /// <summary>Death handling (remove figures, publish events)</summary>
        Death = 60,
        
        /// <summary>Post-death effects (on-kill triggers, soul harvest)</summary>
        PostDeath = 70,
        
        /// <summary>
        /// Bonus action REQUESTS only.
        /// IMPORTANT: This phase is for DECLARATIONS, not EXECUTION.
        /// - BonusMoveRequestEffect: requests bonus move, TurnSystem decides
        /// - ExtraAttackRequestEffect: requests extra attack, TurnSystem decides
        /// DO NOT put actual moves, attacks, or initiative changes here.
        /// TurnSystem is the authority on whether bonus actions happen.
        /// </summary>
        BonusActions = 80,
        
        /// <summary>Cleanup and logging</summary>
        Cleanup = 90
    }
}
