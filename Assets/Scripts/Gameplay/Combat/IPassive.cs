namespace Project.Gameplay.Gameplay.Combat
{
    public interface IPassive
    {
        string Id { get; }
        int Priority { get; }
        void OnPreDamage(HitContext context);
        void OnPostDamage(HitContext context);
    }
}
