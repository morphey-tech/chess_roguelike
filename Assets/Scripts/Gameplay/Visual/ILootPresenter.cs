using Cysharp.Threading.Tasks;
using Project.Gameplay.Gameplay.Visual.Commands.Contexts;

namespace Project.Gameplay.Gameplay.Visual
{
    /// <summary>
    /// Presents loot (apply to economy only, or full scatter → magnet → absorb).
    /// </summary>
    public interface ILootPresenter
    {
        UniTask PresentAsync(LootVisualContext context);
    }
}
