using System;
using Cysharp.Threading.Tasks;
using Project.Gameplay.Gameplay.Visual.Commands;

namespace Project.Gameplay.Gameplay.Visual
{
    /// <summary>
    /// Scoped visual command collector.
    /// 
    /// Effects see only IVisualCommandSink — they can Enqueue, nothing else.
    /// PlayAsync is called by TurnStep when domain logic is complete.
    /// 
    /// USAGE:
    /// using var scope = _visualPipeline.BeginScope();
    /// effectContext = new CombatEffectContext(..., scope, ...);
    /// ApplyEffects(...);
    /// await scope.PlayAsync();
    /// </summary>
    public sealed class VisualScope : IVisualCommandSink, IDisposable
    {
        private readonly VisualCommandQueue _queue = new();
        private readonly VisualPipeline _pipeline;
        
        private bool _played;
        private bool _disposed;

        public VisualScope(VisualPipeline pipeline)
        {
            _pipeline = pipeline;
        }

        /// <summary>
        /// Add a visual command to the scope.
        /// This is the ONLY thing effects can do.
        /// </summary>
        public void Enqueue(IVisualCommand command)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(VisualScope));
            }
            if (_played)
            {
                throw new InvalidOperationException("Cannot enqueue after PlayAsync was called");
            }
            _queue.Enqueue(command);
        }

        /// <summary>
        /// Play all collected commands.
        /// Can only be called once.
        /// </summary>
        public async UniTask PlayAsync()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(VisualScope));
            }
            if (_played)
            {
                throw new InvalidOperationException("PlayAsync can only be called once");
            }
            _played = true;
            await _pipeline.PlayAsync(_queue);
        }

        void IDisposable.Dispose()
        {
            _disposed = true;
        }
    }
}
