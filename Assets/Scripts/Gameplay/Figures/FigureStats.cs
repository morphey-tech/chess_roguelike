using System;
using System.Collections.Generic;
using System.Linq;
using Project.Gameplay.Gameplay.Attack;

namespace Project.Gameplay.Gameplay.Figures
{
    public class FigureStats
    {
        public int MaxHp { get; }
        public int CurrentHp { get; private set; }
        public IReadOnlyList<AttackProfile> Attacks { get; }

        /// <summary>Compatibility: max damage across all attacks.</summary>
        public int Attack => Attacks.Count == 0 ? 0 : Attacks.Max(a => a.Damage);

        /// <summary>Compatibility: max range across all attacks.</summary>
        public int AttackRange => MaxAttackRange;

        public int MaxAttackRange => Attacks.Count == 0 ? 0 : Attacks.Max(a => a.Range);

        public bool IsDead => CurrentHp <= 0;

        public FigureStats(int maxHp, IReadOnlyList<AttackProfile>? attacks)
        {
            MaxHp = maxHp;
            CurrentHp = maxHp;
            Attacks = attacks ?? Array.Empty<AttackProfile>();
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