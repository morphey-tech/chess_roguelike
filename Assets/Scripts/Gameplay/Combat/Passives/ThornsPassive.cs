namespace Project.Gameplay.Gameplay.Combat.Passives
{
    public sealed class ThornsPassive : IPassive
    {
        public string Id { get; }
        public int Priority => 200;
        
        private readonly float _reflectPercent;

        public ThornsPassive(string id, float reflectPercent)
        {
            Id = id;
            _reflectPercent = reflectPercent;
        }

        public void OnPreDamage(HitContext context) { }

        public void OnPostDamage(HitContext context)
        {
            int reflectDamage = (int)(context.FinalDamage * _reflectPercent);
            if (reflectDamage > 0)
            {
                context.Attacker.Stats.TakeDamage(reflectDamage);
            }
        }
    }
}
