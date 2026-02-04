using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Attack;
using Project.Gameplay.Gameplay.Combat;
using Project.Gameplay.Gameplay.Combat.Effects;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using Project.Gameplay.Gameplay.Visual;

namespace Project.Gameplay.Gameplay.Turn.Steps.Impl
{
    /// <summary>
    /// Executes attack action.
    /// 
    /// PIPELINE:
    /// 1. Domain: CombatResolver creates effects (no visuals)
    /// 2. Domain: Effects apply game logic, queue visual commands via IVisualCommandSink
    /// 3. Visual: VisualPipeline plays all animations
    /// </summary>
    public sealed class AttackStep : ITurnStep
    {
        public string Id { get; }

        private readonly AttackStrategyFactory _attackFactory;
        private readonly CombatResolver _combatResolver;
        private readonly PassiveTriggerService _passives;
        private readonly VisualPipeline _visualPipeline;
        private readonly IPublisher<FigureDeathMessage> _deathPublisher;
        private readonly ILogger<AttackStep> _logger;

        public AttackStep(
            string id,
            AttackStrategyFactory attackFactory,
            CombatResolver combatResolver,
            PassiveTriggerService passives,
            VisualPipeline visualPipeline,
            IPublisher<FigureDeathMessage> deathPublisher,
            ILogger<AttackStep> logger)
        {
            Id = id;
            _attackFactory = attackFactory;
            _combatResolver = combatResolver;
            _passives = passives;
            _visualPipeline = visualPipeline;
            _deathPublisher = deathPublisher;
            _logger = logger;
        }

        public async UniTask ExecuteAsync(ActionContext context)
        {
            BoardCell targetCell = context.Grid.GetBoardCell(context.To);
            Figure defender = targetCell?.OccupiedBy;

            if (defender == null || defender.Team == context.Actor.Team)
                return;

            IAttackStrategy attackStrategy = _attackFactory.Get(context.Actor.AttackId);
            
            if (!attackStrategy.CanAttack(context.Actor, context.From, context.To, context.Grid))
                return;

            HitContext hitContext = attackStrategy.CreateHitContext(
                context.Actor, 
                defender, 
                context.From, 
                context.To, 
                context.Grid);
            
            hitContext.AttackId = attackStrategy.Id;

            // === DOMAIN PHASE ===
            CombatResult result = _combatResolver.Resolve(hitContext);

            using VisualScope scope = _visualPipeline.BeginScope();
            CombatEffectContext effectContext = new(
                context,
                context.Grid,
                _deathPublisher,
                _passives,
                scope,
                _logger);

            ApplyEffects(result.Effects, effectContext);

            // === VISUAL PHASE ===
            await scope.PlayAsync();
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
    }
}
