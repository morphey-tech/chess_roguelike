using System.Collections.Generic;

namespace Project.Gameplay.Gameplay.Visual.Commands
{
    /// <summary>
    /// Collects visual commands. Поддерживает добавление команд во время исполнения (для projectile death+loot).
    /// </summary>
    public sealed class VisualCommandQueue : IVisualCommandSink
    {
        private readonly List<IVisualCommand> _commands = new();

        public IReadOnlyList<IVisualCommand> Commands => _commands;

        public void Enqueue(IVisualCommand command)
        {
            _commands.Add(command);
        }

        public void EnqueueRange(IEnumerable<IVisualCommand> commands)
        {
            if (commands != null)
            {
                foreach (IVisualCommand cmd in commands)
                    _commands.Add(cmd);
            }
        }

        public void Clear()
        {
            _commands.Clear();
        }
    }
}
