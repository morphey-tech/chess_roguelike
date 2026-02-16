using System.Collections.Generic;
using Project.Gameplay.Gameplay.Figures;

namespace Project.Gameplay.Gameplay.Combat
{
    public interface ICombatModifierProvider
    {
        void Add(Figure actor, CombatModifierInstance instance);
        void TickTurn(Team ownerTeam, int currentRound);
        IEnumerable<ICombatStatModifier> GetModifiers(HitContext ctx);
    }
}