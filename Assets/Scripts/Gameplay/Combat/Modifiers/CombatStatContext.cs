using Project.Gameplay.Gameplay.Attack;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Combat
{
    public class CombatStatContext
    {
        public Figure Attacker;
        public Figure Target;
        public AttackProfile Profile;
        public BoardGrid Grid;

        public float Damage;
        public int Range;
    }
}