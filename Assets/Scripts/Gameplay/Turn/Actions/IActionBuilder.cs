namespace Project.Gameplay.Gameplay.Turn.Actions
{
    public interface IActionBuilder
    {
        string ActionType { get; }
        ICombatAction Build(ActionConfig config, IActionBuilderContext builderContext);
    }
}
