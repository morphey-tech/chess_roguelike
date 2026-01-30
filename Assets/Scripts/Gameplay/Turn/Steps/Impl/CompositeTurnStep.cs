using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;

namespace Project.Gameplay.Gameplay.Turn.Steps.Impl
{
    public sealed class CompositeTurnStep : ITurnStep
    {
        public string Id { get; }

        private readonly List<ITurnStep> _steps;

        public CompositeTurnStep(string id, IEnumerable<ITurnStep> steps)
        {
            Id = id;
            _steps = steps.ToList();
        }

        public async UniTask ExecuteAsync(ActionContext context)
        {
            foreach (ITurnStep step in _steps)
            {
                await step.ExecuteAsync(context);
            }
        }
    }
}
