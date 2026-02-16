using Project.Gameplay.Gameplay.Figures;

namespace Project.Gameplay.Gameplay.Combat
{
    public sealed class CombatModifierInstance
    {
        public readonly ICombatStatModifier Modifier;
        public readonly Team OwnerTeam;
        public readonly int ExpiredTurn;

        public CombatModifierInstance(ICombatStatModifier modifier, Team ownerTeam, int expiredTurn)
        {
            Modifier = modifier;
            OwnerTeam = ownerTeam;
            ExpiredTurn = expiredTurn;
        }

        public bool IsExpired(Team team, int currentTurn)
        {
            return team == OwnerTeam && currentTurn >= ExpiredTurn;
        }
    }
}