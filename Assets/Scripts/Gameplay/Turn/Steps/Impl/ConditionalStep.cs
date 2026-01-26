using System;
using Cysharp.Threading.Tasks;

namespace Project.Gameplay.Gameplay.Turn.Steps.Impl
{
    public sealed class ConditionalStep : ITurnStep
    {
        public string Id { get; }

        private readonly Func<TurnStepContext, bool> _condition;
        private readonly ITurnStep _innerStep;

        public ConditionalStep(string id, Func<TurnStepContext, bool> condition, ITurnStep innerStep)
        {
            Id = id;
            _condition = condition;
            _innerStep = innerStep;
        }

        public UniTask ExecuteAsync(TurnStepContext context)
        {
            if (_condition(context))
                return _innerStep.ExecuteAsync(context);

            return UniTask.CompletedTask;
        }
    }
}
