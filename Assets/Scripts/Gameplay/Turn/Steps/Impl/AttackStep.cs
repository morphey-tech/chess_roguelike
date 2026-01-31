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

namespace Project.Gameplay.Gameplay.Turn.Steps.Impl
{
    public sealed class AttackStep : ITurnStep
    {
        public string Id { get; }

        private readonly AttackStrategyFactory _attackFactory;
        private readonly CombatResolver _combatResolver;
        private readonly PassiveTriggerService _passives;
        private readonly IFigurePresenter _figurePresenter;
        private readonly IPublisher<FigureDeathMessage> _deathPublisher;
        private readonly ILogger<AttackStep> _logger;

        public AttackStep(
            string id,
            AttackStrategyFactory attackFactory,
            CombatResolver combatResolver,
            PassiveTriggerService passives,
            IFigurePresenter figurePresenter,
            IPublisher<FigureDeathMessage> deathPublisher,
            ILogger<AttackStep> logger)
        {
            Id = id;
            _attackFactory = attackFactory;
            _combatResolver = combatResolver;
            _passives = passives;
            _figurePresenter = figurePresenter;
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

            CombatResult result = _combatResolver.Resolve(hitContext);

            var effectContext = new CombatEffectContext(
                context,
                context.Grid,
                _figurePresenter,
                _deathPublisher,
                _passives,
                _logger);

            // Apply all effects, including any dynamically added ones
            await ApplyEffectsAsync(result.Effects, effectContext);
        }

        private async UniTask ApplyEffectsAsync(IEnumerable<ICombatEffect> effects, CombatEffectContext context)
        {
            var queue = new Queue<ICombatEffect>(effects);
            int effectIndex = 0;

            _logger.Debug($"=== Effect Pipeline Start ({queue.Count} effects) ===");

            while (queue.Count > 0)
            {
                ICombatEffect effect = queue.Dequeue();
                
                _logger.Debug($"[{effectIndex++}] {effect.GetType().Name} (Phase: {effect.Phase}, Order: {effect.OrderInPhase})");
                
                await effect.ApplyAsync(context);

                // Process any effects added during Apply (e.g., KillEffect from DealDamageEffect)
                if (context.PendingEffects.Count > 0)
                {
                    // Sort pending effects by Phase, then OrderInPhase
                    var sorted = context.PendingEffects
                        .OrderBy(e => e.Phase)
                        .ThenBy(e => e.OrderInPhase)
                        .ToList();
                    
                    foreach (var pending in sorted)
                    {
                        _logger.Debug($"  + Queued: {pending.GetType().Name} (Phase: {pending.Phase})");
                        queue.Enqueue(pending);
                    }
                    context.PendingEffects.Clear();
                }
            }
            
            _logger.Debug($"=== Effect Pipeline Complete ===");
        }
    }
}
