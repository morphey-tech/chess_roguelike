namespace Project.Gameplay.Gameplay.Combat
{
    public interface ICombatStatModifier
    {
        int Priority { get; }
        void Modify(CombatStatContext ctx);
    }
}