namespace Project.Gameplay.Gameplay.Figures
{
    public class FigureStats
    {
        public int MaxHp { get; }
        public int CurrentHp { get; private set; }
        public int Attack { get; }
        
        public bool IsDead => CurrentHp <= 0;

        public FigureStats(int maxHp, int attack)
        {
            MaxHp = maxHp;
            CurrentHp = maxHp;
            Attack = attack;
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
            CurrentHp = System.Math.Min(CurrentHp + amount, MaxHp);
        }
    }
}