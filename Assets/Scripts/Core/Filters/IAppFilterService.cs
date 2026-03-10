using Cysharp.Threading.Tasks;

namespace Project.Core.Core.Filters
{
    public interface IAppFilterService
    {
        void AddFilter<T>() where T : IApplicationFilter;
        UniTask RunAsync();
    }
}