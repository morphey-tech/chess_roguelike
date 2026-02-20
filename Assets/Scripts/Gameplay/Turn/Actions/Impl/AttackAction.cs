using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Attack;
using Project.Gameplay.Gameplay.Combat;
using Project.Gameplay.Gameplay.Combat.Visual;
using Project.Gameplay.Gameplay.Combat.Effects;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using Project.Gameplay.Gameplay.Loot;
using Project.Gameplay.Gameplay.Turn;
using Project.Gameplay.Gameplay.Visual;
using Project.Gameplay.Gameplay.Visual.Commands;
using Project.Core.Core.Configs.Stats;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Attack.Rules;

namespace Project.Gameplay.Gameplay.Turn.Actions.Impl
{
    /// <summary>
    /// Executes attack action.
    ///
    /// PIPELINE:
    /// 1. Domain: CombatResolver creates effects (no visuals)
    /// 2. Domain: Effects apply game logic, record visual events
    /// 3. Presentation: CombatVisualPlanner builds visual commands from events
    /// 4. Visual: VisualPipeline plays all animations
    /// </summary>
    public sealed class AttackAction : ICombatAction
    {
        public string Id { get; }

        private readonly AttackStrategyFactory _attackFactory;
        private readonly IAttackResolver _attackResolver;
        private readonly CombatResolver _combatResolver;
        private readonly ICombatVisualPlanner _visualPlanner;
        private readonly PassiveTriggerService _passives;
        private readonly VisualPipeline _visualPipeline;
        private readonly IPublisher<FigureDeathMessage> _deathPublisher;
        private readonly LootService _lootService;
        private readonly DamageApplier _damageApplier;
        private readonly IFigureLifeService _figureLifeService;
        private readonly ActionContextAccessor _contextAccessor;
        private readonly IAttackQueryService _attackQueryService;
        private readonly AttackRuleService _attackRuleService;
        private readonly ILogger<AttackAction> _logger;

        public AttackAction(
            string id,
            AttackStrategyFactory attackFactory,
            IAttackResolver attackResolver,
            CombatResolver combatResolver,
            ICombatVisualPlanner visualPlanner,
            PassiveTriggerService passives,
            VisualPipeline visualPipeline,
            IPublisher<FigureDeathMessage> deathPublisher,
            LootService lootService,
            DamageApplier damageApplier,
            IFigureLifeService figureLifeService,
            ActionContextAccessor contextAccessor,
            IAttackQueryService attackQueryService,
            AttackRuleService attackRuleService,
            ILogger<AttackAction> logger)
        {
            Id = id;
            _attackFactory = attackFactory;
            _attackResolver = attackResolver;
            _combatResolver = combatResolver;
            _visualPlanner = visualPlanner;
            _passives = passives;
            _visualPipeline = visualPipeline;
            _deathPublisher = deathPublisher;
            _lootService = lootService;
            _damageApplier = damageApplier;
            _figureLifeService = figureLifeService;
            _contextAccessor = contextAccessor;
            _attackQueryService = attackQueryService;
            _attackRuleService = attackRuleService;
            _logger = logger;
        }

        public bool CanExecute(ActionContext context)
        {
            BoardCell targetCell = context.Grid.GetBoardCell(context.To);
            Figure defender = targetCell?.OccupiedBy;

            if (defender == null || defender.Team == context.Actor.Team)
                return false;

            if (context.Actor.AttackId == "profiled")
            {
                AttackProfile? profile = _attackResolver.Resolve(context.Actor, context.From, context.To, context.Grid);
                return profile != null;
            }
            else
            {
                // Use AttackRuleService to validate attack (includes DesperationRule, TauntRule, etc.)
                var attackContext = new AttackRuleContext(
                    context.Actor,
                    defender,
                    context.From,
                    context.To,
                    context.Grid);
                return _attackRuleService.CanAttack(attackContext);
            }
        }

        public IReadOnlyCollection<ActionPreview> GetPreviews(Figure actor, GridPosition from, BoardGrid grid)
        {
            IList<ActionPreview> result = new List<ActionPreview>();
            foreach (GridPosition enemyPos in _attackQueryService.GetTargets(actor, from, grid))
            {
                BoardCell enemyCell = grid.GetBoardCell(enemyPos);
                result.Add(new ActionPreview
                {
                    MoveTo = enemyCell.Position,
                    Target = enemyCell.OccupiedBy
                });
            }

            return new ReadOnlyCollection<ActionPreview>(result);
        }

        public async UniTask ExecuteAsync(ActionContext context)
        {
            if (!CanExecute(context))
                return;

            BoardCell targetCell = context.Grid.GetBoardCell(context.To);
            Figure defender = targetCell?.OccupiedBy;

            if (defender == null || defender.Team == context.Actor.Team)
                return;

            HitContext hitContext;

            if (context.Actor.AttackId == "profiled")
            {
                AttackProfile? profile = _attackResolver.Resolve(context.Actor, context.From, context.To, context.Grid);
                if (profile == null)
                    return;

                hitContext = new HitContext
                {
                    Attacker = context.Actor,
                    Target = defender,
                    Profile = profile,
                    AttackerPosition = context.From,
                    TargetPosition = context.To,
                    Grid = context.Grid,
                    HitType = MapHitType(profile.Type),
                    AttackerMovesOnKill = false,
                    AttackId = profile.Type.ToString(),
                    Delivery = profile.Delivery,
                    Pattern = profile.Pattern,
                    ProjectileConfigId = profile.ProjectileConfigId
                };
            }
            else
            {
                // Use AttackRuleService to validate attack (includes DesperationRule, TauntRule, etc.)
                var attackContext = new AttackRuleContext(
                    context.Actor,
                    defender,
                    context.From,
                    context.To,
                    context.Grid);
                if (!_attackRuleService.CanAttack(attackContext))
                    return;

                IAttackStrategy attackStrategy = _attackFactory.Get(context.Actor.AttackId);
                hitContext = attackStrategy.CreateHitContext(
                    context.Actor,
                    defender,
                    context.From,
                    context.To,
                    context.Grid);
                hitContext.AttackId = attackStrategy.Id;
                hitContext.Delivery = attackStrategy.Delivery;
                hitContext.Pattern = HitPattern.Single;
            }

            context.ActionExecuted = true;

            // === DOMAIN PHASE ===
            CombatResult result = _combatResolver.Resolve(hitContext);

            await _lootService.EnsureLoadedAsync();
            using VisualScope scope = _visualPipeline.BeginScope();

            var visualEvents = new List<ICombatVisualEvent>();
            CombatEffectContext effectContext = new(
                context,
                context.Grid,
                _deathPublisher,
                _passives,
                _lootService,
                _damageApplier,
                _figureLifeService,
                visualEvents,
                _logger);

            ApplyEffects(result.Effects, effectContext);

            // Build and enqueue visual plan for main hits + secondary effects
            VisualCombatPlan plan = _visualPlanner.Build(result, visualEvents);
            foreach (IVisualCommand cmd in plan.Commands)
                scope.Enqueue(cmd);

            // === VISUAL PHASE ===
            _contextAccessor.Set(context);
            try
            {
                await scope.PlayAsync();
            }
            finally
            {
                _contextAccessor.Clear();
            }
        }

        private void ApplyEffects(IEnumerable<ICombatEffect> effects, CombatEffectContext context)
        {
            var queue = new Queue<ICombatEffect>(effects);
            int effectIndex = 0;

            _logger.Debug($"=== Effect Pipeline Start ({queue.Count} effects) ===");

            while (queue.Count > 0)
            {
                ICombatEffect effect = queue.Dequeue();

                _logger.Debug($"[{effectIndex++}] {effect.GetType().Name} (Phase: {effect.Phase}, Order: {effect.OrderInPhase})");

                effect.Apply(context);

                if (context.PendingEffects.Count > 0)
                {
                    var sorted = context.PendingEffects
                        .OrderBy(e => e.Phase)
                        .ThenBy(e => e.OrderInPhase)
                        .ToList();

                    foreach (ICombatEffect? pending in sorted)
                    {
                        _logger.Debug($"  + Queued: {pending.GetType().Name} (Phase: {pending.Phase})");
                        queue.Enqueue(pending);
                    }
                    context.PendingEffects.Clear();
                }
            }

            _logger.Debug("=== Effect Pipeline Complete ===");
        }

        private static HitType MapHitType(AttackType type)
        {
            return type switch
            {
                AttackType.Melee => HitType.Melee,
                AttackType.Ranged => HitType.Ranged,
                AttackType.Magic => HitType.Magic,
                _ => HitType.Melee
            };
        }
    }
}
