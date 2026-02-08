using System;
using MessagePipe;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Combat.Damage;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using Project.Gameplay.Gameplay.Turn;
using Project.Gameplay.Gameplay.Visual;
using Project.Gameplay.Gameplay.Visual.Commands.Contexts;

namespace Project.Gameplay.Gameplay.Combat
{
    /// <summary>
    /// Applies damage after projectile impact to sync visuals with domain.
    /// </summary>
    public sealed class ProjectileHitHandler : IProjectileHitHandler
    {
        private readonly MovementService _movementService;
        private readonly PassiveTriggerService _passives;
        private readonly IPublisher<FigureDeathMessage> _deathPublisher;
        private readonly ActionContextAccessor _contextAccessor;
        private readonly IDamageTokenStore _tokenStore;
        private readonly IDamagePipeline _damagePipeline;
        private readonly ILogger<ProjectileHitHandler> _logger;

        public ProjectileHitHandler(
            MovementService movementService,
            PassiveTriggerService passives,
            IPublisher<FigureDeathMessage> deathPublisher,
            ActionContextAccessor contextAccessor,
            IDamageTokenStore tokenStore,
            IDamagePipeline damagePipeline,
            ILogService logService)
        {
            _movementService = movementService;
            _passives = passives;
            _deathPublisher = deathPublisher;
            _contextAccessor = contextAccessor;
            _tokenStore = tokenStore;
            _damagePipeline = damagePipeline;
            _logger = logService.CreateLogger<ProjectileHitHandler>();
        }

        public void OnProjectileHit(ProjectileVisualContext ctx)
        {
            ActionContext actionContext = _contextAccessor.Current;
            if (actionContext == null)
            {
                _logger.Warning("Projectile hit ignored: no active action context");
                return;
            }

            if (!IsHitValid(ctx))
            {
                _logger.Warning($"Projectile hit ignored: invalid hitId={ctx.HitId}");
                return;
            }

            BoardGrid grid = _movementService.Grid;
            if (grid == null)
            {
                _logger.Warning("Projectile hit: Grid not configured");
                return;
            }

            if (!_tokenStore.TryGet(ctx.HitId, out DamageToken token))
            {
                _logger.Warning($"Projectile hit ignored: token not found hitId={ctx.HitId}");
                return;
            }

            if (token.Consumed)
            {
                _logger.Warning($"Projectile hit ignored: token already consumed hitId={ctx.HitId}");
                _tokenStore.Remove(ctx.HitId);
                return;
            }

            Figure attacker = FindFigureById(grid, token.SourceEntityId);
            Figure target = FindFigureById(grid, token.TargetEntityId);
            if (target == null)
            {
                _logger.Warning($"Projectile hit: target {token.TargetEntityId} not found");
                _tokenStore.Remove(ctx.HitId);
                return;
            }

            BoardCell targetCell = grid.GetBoardCell(token.ExpectedPosition);
            if (targetCell?.OccupiedBy == null || targetCell.OccupiedBy.Id != token.TargetEntityId)
            {
                _logger.Warning($"Projectile hit ignored: target moved (id={token.TargetEntityId})");
                _tokenStore.Remove(ctx.HitId);
                return;
            }

            if (target.Stats.CurrentHp <= 0)
            {
                _logger.Warning($"Projectile hit ignored: target already dead (id={token.TargetEntityId})");
                _tokenStore.Remove(ctx.HitId);
                return;
            }

            DamageResult result = _damagePipeline.Calculate(new DamageContext(
                attacker,
                target,
                token.RawDamage,
                token.IsCritical,
                token.AttackId,
                Array.Empty<IDamageModifier>()));

            bool died = target.Stats.TakeDamage(result.Final);
            actionContext.LastDamageDealt = result.Final;

            if (died)
            {
                if (attacker != null)
                {
                    _passives.TriggerKill(attacker, target);
                    _passives.TriggerDeath(target, attacker);
                }

                targetCell?.RemoveFigure();

                _deathPublisher.Publish(new FigureDeathMessage(target.Id, target.Team));
                actionContext.LastAttackKilledTarget = true;
            }
            else
            {
                actionContext.LastAttackKilledTarget = false;
            }

            token.Consume();
            _tokenStore.Remove(token.Id);
        }

        private bool IsHitValid(ProjectileVisualContext ctx)
        {
            return ctx.HitId != Guid.Empty && _tokenStore.TryGet(ctx.HitId, out _);
        }

        private static Figure FindFigureById(BoardGrid grid, int figureId)
        {
            foreach (Figure figure in grid.GetAllFigures())
            {
                if (figure.Id == figureId)
                    return figure;
            }
            return null;
        }
    }
}
