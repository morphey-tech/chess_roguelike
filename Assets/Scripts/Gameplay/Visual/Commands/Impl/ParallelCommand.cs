using System.Linq;
using Cysharp.Threading.Tasks;
using Project.Gameplay.Gameplay.Visual;

namespace Project.Gameplay.Gameplay.Visual.Commands.Impl
{
    /// <summary>
    /// Executes multiple visual commands in parallel.
    /// Use when animations can run simultaneously (e.g., damage + screen shake).
    /// </summary>
    public sealed class ParallelCommand : IVisualCommand
    {
        private readonly IVisualCommand[] _commands;

        public string DebugName => $"Parallel([{string.Join(", ", _commands.Select(c => c.DebugName))}])";

        public ParallelCommand(params IVisualCommand[] commands)
        {
            _commands = commands;
        }

        public async UniTask ExecuteAsync(IPresenterProvider presenters)
        {
            await UniTask.WhenAll(_commands.Select(c => c.ExecuteAsync(presenters)));
        }
    }
}
