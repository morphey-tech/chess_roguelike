using System;
using System.Collections.Generic;
using System.Linq;
using Project.Gameplay.Gameplay.Attack;

namespace Project.Gameplay.Gameplay.Figures
{
    /// <summary>
    /// Stats block: Attack/Defence are Base + Mods (math only here).
    /// CombatResolver reads .Attack.Value and .Defence.Value — no formulas elsewhere.
    /// </summary>
    public class FigureStats
    {
        public int MaxHp { get; }
        public float CurrentHp { get; private set; }
        public IReadOnlyList<AttackProfile> Attacks { get; }

        /// <summary>Attack stat: base + modifiers. Use .Value for combat formula.</summary>
        public FigureStat<float> Attack { get; }
        /// <summary>Defence stat: base + modifiers. Use .Value for combat formula.</summary>
        public FigureStat<float> Defence { get; }

        public int AttackRange => MaxAttackRange;
        public int MaxAttackRange => Attacks.Count == 0 ? 0 : Attacks.Max(a => a.Range);
        public bool IsDead => CurrentHp <= 0f;

        public FigureStats(int maxHp, IReadOnlyList<AttackProfile>? attacks, float baseAttack, float baseDefence)
        {
            MaxHp = maxHp;
            CurrentHp = maxHp;
            Attacks = attacks ?? Array.Empty<AttackProfile>();
            Attack = new FigureStat<float>(baseAttack);
            Defence = new FigureStat<float>(baseDefence);
        }

        /// <summary>
        /// Apply damage and return true if figure died.
        /// </summary>
        public bool TakeDamage(float damage)
        {
            CurrentHp -= damage;
            return IsDead;
        }

        public void Heal(float amount)
        {
            CurrentHp = Math.Min(CurrentHp + amount, MaxHp);
        }

        /// <summary>
        /// Called once per turn to update all stat modifiers.
        /// </summary>
        public void Tick()
        {
            Attack.Tick();
            Defence.Tick();
        }
    }
}