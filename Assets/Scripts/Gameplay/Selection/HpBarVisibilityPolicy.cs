using Project.Core.Core.Configs.Gameplay;
using Project.Gameplay.Gameplay.Figures;

namespace Project.Gameplay.Gameplay.Selection
{
    public static class HpBarVisibilityPolicy
    {
        public static bool ShouldShow(
            HpBarVisibilityMode mode,
            HpBarTeamScope scope,
            Team team,
            bool isHovered,
            bool hasFriendlySelection)
        {
            if (!IsInScope(scope, team))
                return false;

            return mode switch
            {
                HpBarVisibilityMode.Always => true,
                HpBarVisibilityMode.OnHover => isHovered,
                HpBarVisibilityMode.OnHoverOrSelection => isHovered || hasFriendlySelection,
                _ => true
            };
        }

        public static bool IsInScope(HpBarTeamScope scope, Team team)
        {
            return scope switch
            {
                HpBarTeamScope.EnemiesOnly => team == Team.Enemy,
                HpBarTeamScope.AlliesOnly => team == Team.Player,
                HpBarTeamScope.All => true,
                _ => true
            };
        }
    }
}
