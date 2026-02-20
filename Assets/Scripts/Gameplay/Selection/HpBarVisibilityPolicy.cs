using Project.Core.Core.Configs.Gameplay;
using Project.Gameplay.Gameplay.Figures;

namespace Project.Gameplay.Gameplay.Selection
{
    public static class HpBarVisibilityPolicy
    {
        public static bool ShouldShow(
            HpBarVisibilityMode modeAllies,
            HpBarVisibilityMode modeEnemies,
            Team team,
            bool isHovered,
            bool hasFriendlySelection)
        {
            var mode = team == Team.Player ? modeAllies : modeEnemies;

            return mode switch
            {
                HpBarVisibilityMode.Always => true,
                HpBarVisibilityMode.OnHover => isHovered,
                HpBarVisibilityMode.OnHoverOrSelection => isHovered || hasFriendlySelection,
                _ => true
            };
        }
    }
}
