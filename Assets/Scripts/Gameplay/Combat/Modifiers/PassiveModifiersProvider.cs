using System.Collections.Generic;
using Project.Gameplay.Gameplay.Figures;

namespace Project.Gameplay.Gameplay.Combat
{
    public sealed class PassiveModifiersProvider : ICombatModifierProvider
    {
        private readonly Dictionary<int, List<CombatModifierInstance>> _modifiers = new();

        public void Add(Figure actor, CombatModifierInstance instance)
        {
            if (!_modifiers.TryGetValue(actor.Id, out var list))
            {
                list = new List<CombatModifierInstance>();
                _modifiers[actor.Id] = list;
            }

            list.Add(instance);
        }

        public void TickTurn(Team ownerTeam, int currentRound)
        {
            foreach (var list in _modifiers.Values)
            {
                list.RemoveAll(b => b.IsExpired(ownerTeam, currentRound));
            }
        }
        
        public IEnumerable<ICombatStatModifier> GetModifiers(HitContext ctx)
        {
            if (!_modifiers.TryGetValue(ctx.Attacker.Id, out var buffs))
            {
                yield break;
            }
            for (int index = 0; index < buffs.Count; index++)
            {
                CombatModifierInstance? buff = buffs[index];
                yield return buff.Modifier;
            }
        }
    }
}