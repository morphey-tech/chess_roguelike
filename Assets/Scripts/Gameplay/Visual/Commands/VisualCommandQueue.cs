using System.Collections.Generic;

namespace Project.Gameplay.Gameplay.Visual.Commands
{
    /// <summary>
    /// Collects visual commands during domain logic execution.
    /// After logic completes, commands are passed to VisualCommandExecutor.
    /// Implements IVisualCommandSink to enforce layer boundary.
    /// </summary>
    public sealed class VisualCommandQueue : IVisualCommandSink
    {
        private readonly List<IVisualCommand> _commands = new();

        public IReadOnlyList<IVisualCommand> Commands => _commands;

        public void Enqueue(IVisualCommand command)
        {
            _commands.Add(command);
        }

        public void Clear()
        {
            _commands.Clear();
        }
    }
}
