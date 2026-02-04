using Project.Gameplay.Gameplay.Figures;

namespace Project.Gameplay.Gameplay.Combat.Effects.Impl
{
    /// <summary>
    /// REQUESTS a bonus move for the attacker (e.g., slippery passive).
    /// 
    /// IMPORTANT: This is a DECLARATION, not EXECUTION.
    /// - This effect only sets ActionContext.BonusMoveDistance
    /// - TurnSystem decides whether to grant the bonus move
    /// - DO NOT put actual movement logic here
    /// 
    /// The actual move will be handled by subsequent turn steps.
    /// </summary>
    public sealed class BonusMoveRequestEffect : ICombatEffect
    {
        public CombatEffectPhase Phase => CombatEffectPhase.BonusActions;
        public int OrderInPhase => 0;

        private readonly Figure _figure;
        private readonly int _distance;

        public BonusMoveRequestEffect(Figure figure, int distance)
        {
            _figure = figure;
            _distance = distance;
        }

        public void Apply(CombatEffectContext context)
        {
            context.Logger.Info($"[DEBUG] BonusMoveRequestEffect.Apply: figure={_figure.Id}, distance={_distance}, ActionContext.Actor={context.ActionContext.Actor.Id}");
            context.ActionContext.BonusMoveDistance = _distance;
        }
    }
}
