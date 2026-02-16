namespace Project.Gameplay.Gameplay.Modifier
{
    public interface ITimedModifier
    {
        void Tick();
        bool IsExpired { get; }
    }
}
