namespace Project.Gameplay.Gameplay.Save
{
    public interface ISaveDataProvider
    {
        void Populate(SaveSnapshot snapshot);
    }
}