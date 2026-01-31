using System;
using System.Linq;
using MessagePipe;
using Project.Core.Core.Configs.Turn;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Attack;
using Project.Gameplay.Gameplay.Combat;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Turn.Steps;
using Project.Gameplay.Gameplay.Turn.Steps.Impl;

namespace Project.Gameplay.Gameplay.Turn
{
    public sealed class TurnStepFactory
    {
        private readonly MovementService _movementService;
        private readonly AttackStrategyFactory _attackFactory;
        private readonly CombatResolver _combatResolver;
        private readonly IFigurePresenter _figurePresenter;
        private readonly IPublisher<FigureDeathMessage> _deathPublisher;
        private readonly ILogService _logService;

        public TurnStepFactory(
            MovementService movementService,
            AttackStrategyFactory attackFactory,
            CombatResolver combatResolver,
            IFigurePresenter figurePresenter,
            IPublisher<FigureDeathMessage> deathPublisher,
            ILogService logService)
        {
            _movementService = movementService;
            _attackFactory = attackFactory;
            _combatResolver = combatResolver;
            _figurePresenter = figurePresenter;
            _deathPublisher = deathPublisher;
            _logService = logService;
        }

        public ITurnStep CreateStep(StepConfig config, string parentId = "")
        {
            string stepId = string.IsNullOrEmpty(parentId) 
                ? config.Type 
                : $"{parentId}.{config.Type}";

            return config.Type switch
            {
                "move" => new MoveStep(stepId, _movementService, _figurePresenter),
                "attack" => new AttackStep(stepId, _attackFactory, _combatResolver, _figurePresenter, _deathPublisher, _logService.CreateLogger<AttackStep>()),
                "move_to_killed" => new MoveToKilledTargetStep(_movementService, _figurePresenter, _logService),
                "composite" => CreateComposite(stepId, config.Steps),
                _ => throw new Exception($"Unknown step type: {config.Type}")
            };
        }

        public ITurnStep CreateComposite(string id, StepConfig[] steps)
        {
            var stepList = steps.Select(s => CreateStep(s, id)).ToList();
            return new CompositeTurnStep(id, stepList);
        }
    }
}
