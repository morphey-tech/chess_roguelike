namespace Project.Gameplay.Gameplay.Combat.Damage
{
    public interface IDamagePipeline
    {
        DamageResult Calculate(DamageContext context);
    }
}
