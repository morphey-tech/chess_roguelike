using System;

namespace Project.Gameplay.Gameplay.Figures
{
    public class FigureStats
    {
        public int MaxHp { get; }
        public int CurrentHp { get; private set; }
        public int Attack { get; }
        
        /// <summary>
        /// Attack range in cells. 1 = melee (adjacent cells only).
        /// </summary>
        public int AttackRange { get; }
        
        public bool IsDead => CurrentHp <= 0;

        public FigureStats(int maxHp, int attack, int attackRange = 1)
        {
            MaxHp = maxHp;
            CurrentHp = maxHp;
            Attack = attack;
            AttackRange = Math.Max(1, attackRange);
        }

        /// <summary>
        /// Apply damage and return true if figure died.
        /// </summary>
        public bool TakeDamage(int damage)
        {
            CurrentHp -= damage;
            return IsDead;
        }

        public void Heal(int amount)
        {
            CurrentHp = Math.Min(CurrentHp + amount, MaxHp);
        }
    }
}