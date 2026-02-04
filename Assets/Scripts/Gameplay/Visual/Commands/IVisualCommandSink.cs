namespace Project.Gameplay.Gameplay.Visual.Commands
{
    /// <summary>
    /// Interface for adding visual commands.
    /// Effects receive only this - no access to queue internals.
    /// Enforces layer boundary between domain and visual.
    /// </summary>
    public interface IVisualCommandSink
    {
        void Enqueue(IVisualCommand command);
    }
}
