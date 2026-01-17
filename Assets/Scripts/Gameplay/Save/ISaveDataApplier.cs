namespace Project.Gameplay.Gameplay.Save
{
    public interface ISaveDataApplier
    {
        void Apply(SaveSnapshot snapshot);
    }
}