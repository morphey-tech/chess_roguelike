namespace Project.Gameplay.Gameplay.Combat.Passives
{
    public sealed class LifestealPassive : IPassive
    {
        public string Id { get; }
        public int Priority => 100;
        
        private readonly float _percent;

        public LifestealPassive(string id, float percent)
        {
            Id = id;
            _percent = percent;
        }

        public void OnPreDamage(HitContext context) { }

        public void OnPostDamage(HitContext context)
        {
            int healAmount = (int)(context.FinalDamage * _percent);
            if (healAmount > 0)
            {
                context.Attacker.Stats.Heal(healAmount);
                context.HealedAmount += healAmount;
            }
        }
    }
}
