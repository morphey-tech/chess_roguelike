using System.Collections.Generic;
using Project.Gameplay.Gameplay.Visual.Commands;

namespace Project.Gameplay.Gameplay.Visual
{
    /// <summary>
    /// Позволяет командам добавлять команды в текущую очередь во время исполнения (например Death+Loot после ApplyDamage).
    /// Устанавливается executor'ом перед циклом, сбрасывается после.
    /// </summary>
    public interface IVisualQueueAppender
    {
        void SetCurrentQueue(VisualCommandQueue queue);
        void EnqueueCommands(IReadOnlyList<IVisualCommand> commands);
    }
}
