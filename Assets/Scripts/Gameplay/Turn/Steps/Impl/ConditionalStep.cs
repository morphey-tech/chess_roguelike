using System;
using Cysharp.Threading.Tasks;

namespace Project.Gameplay.Gameplay.Turn.Steps.Impl
{
    public sealed class ConditionalStep : ITurnStep
    {
        public string Id { get; }

        private readonly Func<ActionContext, bool> _condition;
        private readonly ITurnStep _innerStep;

        public ConditionalStep(string id, Func<ActionContext, bool> condition, ITurnStep innerStep)
        {
            Id = id;
            _condition = condition;
            _innerStep = innerStep;
        }

        public UniTask ExecuteAsync(ActionContext context)
        {
            if (_condition(context))
                return _innerStep.ExecuteAsync(context);

            return UniTask.CompletedTask;
        }
    }
}
