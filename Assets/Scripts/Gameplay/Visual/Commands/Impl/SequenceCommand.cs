using System.Linq;
using Cysharp.Threading.Tasks;
using Project.Gameplay.Gameplay.Visual;

namespace Project.Gameplay.Gameplay.Visual.Commands.Impl
{
    /// <summary>
    /// Executes multiple visual commands in sequence.
    /// Use when animations must run one after another as a logical group.
    /// </summary>
    public sealed class SequenceCommand : IVisualCommand
    {
        private readonly IVisualCommand[] _commands;

        public string DebugName => $"Sequence([{string.Join(", ", _commands.Select(c => c.DebugName))}])";
        public VisualCommandMode Mode => VisualCommandMode.Blocking;

        public SequenceCommand(params IVisualCommand[] commands)
        {
            _commands = commands;
        }

        public async UniTask ExecuteAsync(IPresenterProvider presenters)
        {
            foreach (var command in _commands)
            {
                await command.ExecuteAsync(presenters);
            }
        }
    }
}
