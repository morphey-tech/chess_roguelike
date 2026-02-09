using Cysharp.Threading.Tasks;
using Project.Gameplay.UI;

namespace Project.Unity.UI
{
    public sealed class GameUiService : IGameUiService
    {
        public async UniTask ShowWorldUiAsync()
        {
            await Gameplay.Gameplay.UI.UI.ShowAsync<WorldUIWindow>();
        }

        public async UniTask ShowPreparePhaseAsync()
        {
            TurnWindow? wnd = await Gameplay.Gameplay.UI.UI.ShowAsync<TurnWindow>();
            wnd?.SetPreparePhase();
        }

        public void SetGamePhase()
        {
            if (!Gameplay.Gameplay.UI.UI.IsValid)
            {
                return;
            }

            Gameplay.Gameplay.UI.UI.GetOrCreate<TurnWindow>().SetGamePhase();
        }
    }
}
