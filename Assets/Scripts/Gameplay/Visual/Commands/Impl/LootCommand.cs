using Cysharp.Threading.Tasks;
using Project.Gameplay.Gameplay.Visual;
using Project.Gameplay.Gameplay.Visual.Commands.Contexts;

namespace Project.Gameplay.Gameplay.Visual.Commands.Impl
{
    /// <summary>
    /// Visual command for loot drop. Executed by ILootPresenter (apply-only or full visual).
    /// </summary>
    public sealed class LootCommand : IVisualCommand
    {
        private readonly LootVisualContext _ctx;

        public string DebugName => $"Loot(pos=({_ctx.DropPosition.Row},{_ctx.DropPosition.Column}), r={_ctx.Loot.Resources.Count}, i={_ctx.Loot.Items.Count})";
        public VisualCommandMode Mode => VisualCommandMode.Background;

        public LootCommand(LootVisualContext ctx)
        {
            _ctx = ctx;
        }

        public async UniTask ExecuteAsync(IPresenterProvider presenters)
        {
            if (presenters.Loot == null)
                return;
            await presenters.Loot.PresentAsync(_ctx);
        }
    }
}
