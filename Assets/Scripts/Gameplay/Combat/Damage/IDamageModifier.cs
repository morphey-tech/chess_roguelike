namespace Project.Gameplay.Gameplay.Combat.Damage
{
    public interface IDamageModifier
    {
        int Order { get; }
        float Modify(DamageContext context, float value);
    }
}
