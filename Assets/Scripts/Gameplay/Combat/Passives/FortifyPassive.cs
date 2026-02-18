using Project.Gameplay.Gameplay.Combat;
using Project.Gameplay.Gameplay.Combat.Contexts;
using Project.Gameplay.Gameplay.Combat.Triggers;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Modifier;

namespace Project.Gameplay.Gameplay.Combat.Passives
{
    /// <summary>
    /// Tank: +Defence each turn only if this figure did NOT move this turn (timed 1 turn).
    /// </summary>
    public sealed class FortifyPassive : IPassive, IOnTurnStart
    {
        public string Id { get; }
        public int Priority => 100;

        private readonly int _defenceBonus;

        public FortifyPassive(string id, int damageReduction)
        {
            Id = id;
            _defenceBonus = damageReduction;
        }

        public void OnTurnStart(Figure figure, TurnContext context)
        {
            var fortifyMod = new FortifyDefenceModifier(figure, _defenceBonus);
            var mod = new TimedStatModifier(fortifyMod, 1);
            figure.Stats.Defence.Add(mod);
        }
    }
}
