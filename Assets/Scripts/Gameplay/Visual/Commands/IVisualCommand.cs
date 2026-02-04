using Cysharp.Threading.Tasks;
using Project.Gameplay.Gameplay.Visual;

namespace Project.Gameplay.Gameplay.Visual.Commands
{
    /// <summary>
    /// Represents a visual command that can be executed asynchronously.
    /// Commands are created by domain logic and executed by VisualCommandExecutor.
    /// 
    /// DESIGN RULES:
    /// - Domain logic creates commands, never calls presenters directly
    /// - Commands receive IPresenterProvider at execution time, not construction
    /// - Commands are executed sequentially by VisualCommandExecutor
    /// - Each command awaits its animation/visual completion
    /// - Presenters are dumb executors with no logic
    /// </summary>
    public interface IVisualCommand
    {
        /// <summary>
        /// Human-readable description for debug logs.
        /// Example: "DamageCommand(target=5, dmg=12)"
        /// </summary>
        string DebugName { get; }
        
        /// <summary>
        /// Execute the visual command (play animation, update UI, etc.)
        /// Should await until the visual effect is complete.
        /// </summary>
        /// <param name="presenters">Provider for accessing presenters</param>
        UniTask ExecuteAsync(IPresenterProvider presenters);
    }
}
