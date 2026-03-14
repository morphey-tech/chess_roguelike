using System;

namespace Project.Gameplay.Gameplay.Save.Models
{
    /// <summary>
    /// Сериализуемое представление статус-эффекта.
    /// </summary>
    [Serializable]
    public sealed class EffectState
    {
        public string EffectId { get; set; }
        public int RemainingTurns { get; set; }
        public int RemainingUses { get; set; }
        public int StackCount { get; set; }

        public EffectState() { }

        public EffectState(string effectId, int turns = -1, int uses = -1, int stacks = 1)
        {
            EffectId = effectId;
            RemainingTurns = turns;
            RemainingUses = uses;
            StackCount = stacks;
        }
    }
}
