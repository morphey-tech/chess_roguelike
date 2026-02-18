namespace Project.Gameplay.Gameplay.Modifier
{
    public interface IStatModifier<T>
    {
        int Priority { get; }
        T Apply(T value);
    }
}