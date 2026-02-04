using Cysharp.Threading.Tasks;
using Project.Gameplay.Gameplay.Visual.Commands;
using VContainer;

namespace Project.Gameplay.Gameplay.Visual
{
    /// <summary>
    /// High-level visual playback policy.
    /// 
    /// TurnStep knows WHAT to play, Pipeline decides HOW.
    /// Executor is implementation detail, Pipeline is policy.
    /// 
    /// HIERARCHY:
    /// TurnStep → VisualScope (IVisualCommandSink) → VisualPipeline (policy) → Executor (mechanics)
    /// 
    /// USAGE (recommended):
    /// using var scope = _visualPipeline.BeginScope();
    /// effectContext = new CombatEffectContext(..., scope, ...);
    /// ApplyEffects(...);
    /// await scope.PlayAsync();
    /// 
    /// Effects only see IVisualCommandSink — can't Clear(), can't read Commands.
    /// </summary>
    public sealed class VisualPipeline
    {
        private readonly VisualCommandExecutor _executor;

        /// <summary>
        /// If true, animations are skipped (instant execution).
        /// </summary>
        public bool SkipAnimations { get; set; }

        /// <summary>
        /// Animation speed multiplier (1.0 = normal, 2.0 = 2x speed).
        /// TODO: Apply when animation system supports it.
        /// </summary>
        public float SpeedMultiplier { get; set; } = 1f;

        [Inject]
        private VisualPipeline(VisualCommandExecutor executor)
        {
            _executor = executor;
        }

        /// <summary>
        /// Begin a scoped visual command collection.
        /// Returns IVisualCommandSink for effects to use.
        /// Call scope.PlayAsync() when done.
        /// </summary>
        public VisualScope BeginScope()
        {
            return new VisualScope(this);
        }

        /// <summary>
        /// Play all visual commands from the queue.
        /// Internal — use BeginScope() instead.
        /// </summary>
        internal async UniTask PlayAsync(VisualCommandQueue? queue)
        {
            if (queue == null || queue.Commands.Count == 0)
            {
                return;
            }
            if (SkipAnimations)
            {
                queue.Clear();
                return;
            }

            await _executor.ExecuteAsync(queue);
        }

        /// <summary>
        /// Play a single visual command.
        /// </summary>
        public async UniTask PlayAsync(IVisualCommand? command)
        {
            if (command == null)
            {
                return;
            }
            if (SkipAnimations)
            {
                return;
            }

            VisualCommandQueue queue = new();
            queue.Enqueue(command);
            await _executor.ExecuteAsync(queue);
        }
    }
}
