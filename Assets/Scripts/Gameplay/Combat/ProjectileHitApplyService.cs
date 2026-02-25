using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Project.Gameplay.Gameplay.Combat.Damage;
using Project.Gameplay.Gameplay.Combat.Visual;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using Project.Gameplay.Gameplay.Loot;
using Project.Gameplay.Gameplay.Turn;
using Project.Gameplay.Gameplay.Visual;
using Project.Gameplay.Gameplay.Visual.Commands;
using Project.Gameplay.Gameplay.Visual.Commands.Contexts;
using Project.Gameplay.Gameplay.Visual.Commands.Impl;
using VContainer;

namespace Project.Gameplay.Gameplay.Combat
{
    public sealed class ProjectileHitApplyService : IProjectileHitApplyService
    {
        private readonly ActionContextAccessor _contextAccessor;
        private readonly DamageApplier _damageApplier;
        private readonly LootService _lootService;

        [Inject]
        private ProjectileHitApplyService(
            ActionContextAccessor contextAccessor,
            DamageApplier damageApplier,
            LootService lootService)
        {
            _contextAccessor = contextAccessor;
            _damageApplier = damageApplier;
            _lootService = lootService;
        }

        public UniTask ApplyAsync(ProjectileHitApplyEvent evt, IPresenterProvider presenters)
        {
            ActionContext ctx = _contextAccessor.Current;
            if (ctx?.Grid == null)
            {
                return UniTask.CompletedTask;
            }

            BoardGrid grid = ctx.Grid;
            Figure attacker = FindFigureById(grid, evt.AttackerId);
            Figure target = FindFigureById(grid, evt.TargetId);
            if (target == null)
            {
                return UniTask.CompletedTask;
            }

            BoardCell targetCell = grid.GetBoardCell(evt.TargetPosition);
            if (targetCell?.OccupiedBy != target || target.Stats.CurrentHp <= 0)
            {
                return UniTask.CompletedTask;
            }

            DamageContext dmgCtx = new(
                attacker,
                target,
                evt.Damage,
                evt.IsCritical,
                evt.IsDodged,
                evt.IsCancelled,
                evt.AttackId ?? "ranged",
                Array.Empty<IDamageModifier>());

            (DamageResult result, bool died) = _damageApplier.ApplyDamageOnly(dmgCtx, targetCell);

            ctx.LastDamageDealt = result.Final;
            ctx.LastAttackKilledTarget = died;

            if (died && presenters.QueueAppender != null)
            {
                List<IVisualCommand> extra = new List<IVisualCommand>
                {
                    new DeathCommand(new DeathVisualContext(target.Id, null))
                };
                if (!string.IsNullOrEmpty(target.LootTableId))
                {
                    LootResult lootResult = _lootService.Roll(target.LootTableId);
                    if (lootResult is { IsEmpty: false })
                        extra.Add(new LootCommand(new LootVisualContext(targetCell.Position, lootResult)));
                }
                presenters.QueueAppender.EnqueueCommands(extra);
            }
            return UniTask.CompletedTask;
        }

        private static Figure FindFigureById(BoardGrid grid, int figureId)
        {
            return grid.GetAllFigures().FirstOrDefault(f => f.Id == figureId);
        }
    }
}
