namespace Project.Gameplay.Gameplay.Combat.Damage
{
    public interface IDamageModifier
    {
        int Order { get; }
        int Modify(DamageContext context, int value);
    }
}
