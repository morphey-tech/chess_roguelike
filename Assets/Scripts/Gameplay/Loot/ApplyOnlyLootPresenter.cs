using Cysharp.Threading.Tasks;
using Project.Gameplay.Gameplay.Economy;
using Project.Gameplay.Gameplay.Visual;
using Project.Gameplay.Gameplay.Visual.Commands.Contexts;

namespace Project.Gameplay.Gameplay.Loot
{
    /// <summary>
    /// Applies loot to economy immediately with no visual. Use for tests or when full LootPresenter is not available.
    /// </summary>
    public sealed class ApplyOnlyLootPresenter : ILootPresenter
    {
        private readonly EconomyService _economy;

        public ApplyOnlyLootPresenter(EconomyService economy)
        {
            _economy = economy;
        }

        public UniTask PresentAsync(LootVisualContext context)
        {
            if (context?.Loot is { IsEmpty: false })
            {
                _economy.ApplyLootResult(context.Loot);
            }
            return UniTask.CompletedTask;
        }
    }
}
