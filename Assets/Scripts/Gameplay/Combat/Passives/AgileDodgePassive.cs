using Project.Core.Core.Random;
using Project.Gameplay.Gameplay.Combat.Contexts;
using Project.Gameplay.Gameplay.Combat.Triggers;
using Project.Gameplay.Gameplay.Figures.StatusEffects;

namespace Project.Gameplay.Gameplay.Combat.Passives
{
    public sealed class AgileDodgePassive : IPassive, IOnMove
    {
        public string Id { get; }
        public int Priority => 200;

        private readonly float _chance;
        private readonly IRandomService _random;

        public AgileDodgePassive(string id, float chance, IRandomService random)
        {
            Id = id;
            _chance = chance;
            _random = random;
        }

        void IOnMove.OnMove(MoveContext context)
        {
            context.Actor.Effects.AddOrStack(new DodgeEffect(_chance, random: _random, turns: 1, uses: 1));
        }
    }
}