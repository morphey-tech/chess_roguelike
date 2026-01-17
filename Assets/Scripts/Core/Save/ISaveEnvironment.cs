namespace Project.Core.Core.Save
{
    public interface ISaveEnvironment
    {
        string SavePath { get; }
        string CurrentScene { get; }
    }
}