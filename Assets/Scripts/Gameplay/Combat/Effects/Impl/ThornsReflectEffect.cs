using Cysharp.Threading.Tasks;
using Project.Gameplay.Gameplay.Figures;

namespace Project.Gameplay.Gameplay.Combat.Effects.Impl
{
    /// <summary>
    /// Visual effect for thorns damage reflection.
    /// Note: actual damage is already applied during passive execution.
    /// </summary>
    public sealed class ThornsReflectEffect : ICombatEffect
    {
        public CombatEffectPhase Phase => CombatEffectPhase.SecondaryDamage;
        public int OrderInPhase => 30; // After bonus damage

        private readonly Figure _attacker;
        private readonly int _reflectedDamage;

        public ThornsReflectEffect(Figure attacker, int reflectedDamage)
        {
            _attacker = attacker;
            _reflectedDamage = reflectedDamage;
        }

        public UniTask ApplyAsync(CombatEffectContext context)
        {
            context.FigurePresenter.PlayDamageEffect(_attacker.Id);
            context.Logger.Info($"{_attacker} takes {_reflectedDamage} thorns damage. HP: {_attacker.Stats.CurrentHp}/{_attacker.Stats.MaxHp}");
            return UniTask.CompletedTask;
        }
    }
}
