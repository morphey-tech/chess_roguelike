namespace Project.Gameplay.Gameplay.Combat
{
    public interface ICombatStatSystem
    {
        CalculatedHitStats Calculate(HitContext context);
    }
}