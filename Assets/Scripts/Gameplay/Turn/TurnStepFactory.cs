using System;
using System.Linq;
using MessagePipe;
using Project.Core.Core.Configs.Turn;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Attack;
using Project.Gameplay.Gameplay.Combat;
using Project.Gameplay.Gameplay.Combat.Visual;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Turn.Steps;
using Project.Gameplay.Gameplay.Turn.Steps.Impl;
using Project.Gameplay.Gameplay.Visual;

namespace Project.Gameplay.Gameplay.Turn
{
    public sealed class TurnStepFactory
    {
        private readonly MovementService _movementService;
        private readonly AttackStrategyFactory _attackFactory;
        private readonly CombatResolver _combatResolver;
        private readonly IAttackResolver _attackResolver;
        private readonly ICombatVisualPlanner _visualPlanner;
        private readonly PassiveTriggerService _passives;
        private readonly VisualPipeline _visualPipeline;
        private readonly IPublisher<FigureDeathMessage> _deathPublisher;
        private readonly ActionContextAccessor _contextAccessor;
        private readonly ILogService _logService;

        public TurnStepFactory(
            MovementService movementService,
            AttackStrategyFactory attackFactory,
            CombatResolver combatResolver,
            IAttackResolver attackResolver,
            ICombatVisualPlanner visualPlanner,
            PassiveTriggerService passives,
            VisualPipeline visualPipeline,
            IPublisher<FigureDeathMessage> deathPublisher,
            ActionContextAccessor contextAccessor,
            ILogService logService)
        {
            _movementService = movementService;
            _attackFactory = attackFactory;
            _combatResolver = combatResolver;
            _attackResolver = attackResolver;
            _visualPlanner = visualPlanner;
            _passives = passives;
            _visualPipeline = visualPipeline;
            _deathPublisher = deathPublisher;
            _contextAccessor = contextAccessor;
            _logService = logService;
        }

        public ITurnStep CreateStep(StepConfig config, string parentId = "")
        {
            string stepId = string.IsNullOrEmpty(parentId) 
                ? config.Type 
                : $"{parentId}.{config.Type}";

            return config.Type switch
            {
                "move" => new MoveStep(stepId, _movementService, _visualPipeline),
                "attack" => new AttackStep(stepId, _attackFactory, _attackResolver, _combatResolver, _visualPlanner, _passives, _visualPipeline, _deathPublisher, _contextAccessor, _logService.CreateLogger<AttackStep>()),
                "move_to_killed" => new MoveToKilledTargetStep(_movementService, _visualPipeline, _logService),
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
