namespace Project.Gameplay.Gameplay.Figures.StatusEffects
{
    /// <summary>
    /// Buff effect that modifies a stat (Attack, Defence, or MaxHp).
    /// </summary>
    public sealed class InspirationBuffEffect : StatusEffectBase
    {
        public override string Id => "inspiration_buff";
        public override EffectCategory Category => EffectCategory.Buff;

        private readonly BuffType _buffType;
        private readonly float _value;
        private readonly string _modifierId;

        public InspirationBuffEffect(BuffType buffType, float value, int turns)
            : base(turns: turns, uses: -1)
        {
            _buffType = buffType;
            _value = value;
            _modifierId = $"inspiration_{buffType}";
        }

        public override void OnApply(Figure owner)
        {
            switch (_buffType)
            {
                case BuffType.Attack:
                    owner.Stats.Attack.AddModifier(new FlatModifier<float>(_modifierId, _value, 0, 1, true, ModifierSourceContext.CombatEffect));
                    break;
                case BuffType.Defence:
                    owner.Stats.Defence.AddModifier(new FlatModifier<float>(_modifierId, _value, 0, 1, true, ModifierSourceContext.CombatEffect));
                    break;
                case BuffType.Evasion:
                    owner.Stats.Evasion.AddModifier(new FlatModifier<float>(_modifierId, _value, 0, 1, true, ModifierSourceContext.CombatEffect));
                    break;
            }
        }

        public override void OnRemove(Figure owner)
        {
            switch (_buffType)
            {
                case BuffType.Attack:
                    owner.Stats.Attack.RemoveModifiersById(_modifierId);
                    break;
                case BuffType.Defence:
                    owner.Stats.Defence.RemoveModifiersById(_modifierId);
                    break;
                case BuffType.Evasion:
                    owner.Stats.Evasion.RemoveModifiersById(_modifierId);
                    break;
            }
        }
    }
}
