namespace Project.Gameplay.Gameplay.Attack.Rules
{
    /// <summary>
    /// Interface for attack validation rules.
    /// Rules determine whether a target can be attacked based on various conditions.
    /// </summary>
    public interface IAttackRule
    {
        /// <summary>
        /// Priority of rule execution (higher = later in pipeline).
        /// </summary>
        int Priority { get; }
        
        /// <summary>
        /// Validates if the target can be attacked based on this rule.
        /// Returns true if attack is allowed, false if blocked.
        /// </summary>
        bool Validate(AttackRuleContext context);
    }
}
