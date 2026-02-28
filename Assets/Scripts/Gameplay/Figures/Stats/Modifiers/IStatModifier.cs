namespace Project.Gameplay.Gameplay.Figures
{
    public interface IStatModifier<T>
    {
        string Id { get; }
        int Priority { get; }
        int Duration { get; }
        bool Stackable { get; }
        bool IsExpired { get; }
        ModifierSourceContext SourceContext { get; }

        T Apply(T value);
        void Tick();
    }
}