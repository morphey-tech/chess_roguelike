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

        public AgileDodgePassive(string id, float chance)
        {
            Id = id;
            _chance = chance;
        }

        void IOnMove.OnMove(MoveContext context)
        {
            context.Actor.Effects.Add(new DodgeEffect(_chance, turns: 1, uses: 1));
        }
    }
}