namespace Project.Core.Config
{
    public interface IConfigService
    {
        T Get<T>() where T : class;
        bool TryGet<T>(out T config) where T : class;
        void Register<T>(T config) where T : class;
    }
}


