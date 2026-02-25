namespace Project.Gameplay.Gameplay.Attack.Rules
{
    /// <summary>
    /// Interface for attack validation rules.
    /// Rules determine whether a target can be attacked based on various conditions.
    /// </summary>
    public interface IAttackRule
    {
        int Priority { get; }
        bool Validate(AttackRuleContext context);
    }
}
