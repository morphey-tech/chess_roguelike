using Project.Core.Core.Random;
using Project.Core.Core.Triggers;
using Project.Gameplay.Gameplay.Combat.Contexts;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Figures.StatusEffects;

namespace Project.Gameplay.Gameplay.Combat.Passives
{
    public sealed class AgileDodgePassive : IPassive, IOnMove
    {
        public string Id { get; }
        public int Priority => TriggerPriorities.Normal;
        public TriggerGroup Group => TriggerGroup.Default;
        public TriggerPhase Phase => TriggerPhase.AfterMove;

        private readonly float _chance;
        private readonly IRandomService _random;

        public AgileDodgePassive(string id, float chance, IRandomService random)
        {
            Id = id;
            _chance = chance;
            _random = random;
        }

        public bool Matches(TriggerContext context)
        {
            if (context.Type != TriggerType.OnMove)
            {
                return false;
            }
            return context.Actor != null;
        }

        public TriggerResult Execute(TriggerContext context)
        {
            if (context is not IMoveContext moveContext)
            {
                return TriggerResult.Continue;
            }
            return HandleMove(moveContext);
        }

        public TriggerResult HandleMove(IMoveContext context)
        {
            if (context.Actor is not Figure actor)
            {
                return TriggerResult.Continue;
            }
            actor.Effects.AddOrStack(new DodgeEffect(_chance, random: _random, turns: 1, uses: 1));
            return TriggerResult.Continue;
        }
    }
}