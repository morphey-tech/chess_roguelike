using Cysharp.Threading.Tasks;

namespace Project.Gameplay.UI
{
    public interface IGameUiService
    {
        UniTask ShowWorldUiAsync();
        UniTask ShowPreparePhaseAsync();
        void SetGamePhase();
    }
}
