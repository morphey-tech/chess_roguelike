using Cysharp.Threading.Tasks;

namespace Project.Gameplay.Presentations
{
    public interface IPresenter 
    {
        UniTask Init(EntityLink link);
    }
}