using Cysharp.Threading.Tasks;

namespace Project.Core.Core.Filters
{
    public interface IApplicationFilter
    {
        UniTask RunAsync();
    }
}