using Project.Core.Core.Triggers;
using Project.Gameplay.Gameplay.Combat.Contexts;
using Project.Gameplay.Gameplay.Figures;

namespace Project.Gameplay.Gameplay.Combat
{
    /// <summary>
    /// Gameplay-specific extension methods for TriggerService.
    /// Provides convenient methods for common trigger operations.
    /// </summary>
    public static class TriggerServiceExtensions
    {
        /// <summary>
        /// Execute BeforeHit triggers in all phases with full trace logging.
        /// </summary>
        public static bool TriggerBeforeHit(this TriggerService service, Figure attacker, Figure target, BeforeHitContext context)
        {
            foreach (TriggerPhase phase in TriggerPhases.DamagePipeline)
            {
                TriggerContext triggerContext = TriggerContextBuilder
                    .For(TriggerType.OnBeforeHit, phase, TriggerSource.Combat)
                    .WithActor(attacker)
                    .WithTarget(target)
                    .WithValue((int)context.BaseDamage)
                    .WithData(context)
                    .Build();

                TriggerResult result = service.Execute(TriggerType.OnBeforeHit, phase, triggerContext);
                if (result == TriggerResult.Cancel)
                {
                    context.IsCancelled = true;
                    return false;
                }

                // Update context with modified value
                context.BonusDamage += triggerContext.CurrentValue - triggerContext.BaseValue;
            }
            return true;
        }

        /// <summary>
        /// Execute BeforeHit triggers for a specific phase.
        /// </summary>
        public static bool TriggerBeforeHit(this TriggerService service, Figure attacker, Figure target, BeforeHitContext context, TriggerPhase phase)
        {
            TriggerContext triggerContext = TriggerContextBuilder.For(TriggerType.OnBeforeHit, phase)
                .WithActor(attacker)
                .WithTarget(target)
                .WithValue((int)context.BaseDamage)
                .WithData(context)
                .Build();

            TriggerResult result = service.Execute(TriggerType.OnBeforeHit, phase, triggerContext);
            if (result == TriggerResult.Cancel)
            {
                context.IsCancelled = true;
                return false;
            }
            return true;
        }

        /// <summary>
        /// Execute AfterHit triggers.
        /// </summary>
        public static void TriggerAfterHit(this TriggerService service, Figure attacker, Figure target, AfterHitContext context)
        {
            TriggerContext triggerContext = TriggerContextBuilder.For(TriggerType.OnAfterHit, TriggerPhase.AfterApplication)
                .WithActor(attacker)
                .WithTarget(target)
                .WithValue((int)context.DamageDealt)
                .WithData(context)
                .Build();

            service.Execute(TriggerType.OnAfterHit, TriggerPhase.AfterApplication, triggerContext);
        }

        /// <summary>
        /// Execute Kill triggers.
        /// </summary>
        public static void TriggerKill(this TriggerService service, Figure killer, Figure victim)
        {
            TriggerContext context = TriggerContextBuilder.For(TriggerType.OnUnitKill, TriggerPhase.OnDeath)
                .WithActor(killer)
                .WithTarget(victim)
                .Build();

            service.Execute(TriggerType.OnUnitKill, TriggerPhase.OnDeath, context);
        }

        /// <summary>
        /// Execute Death triggers.
        /// </summary>
        public static void TriggerDeath(this TriggerService service, Figure victim, Figure killer)
        {
            TriggerContext context = TriggerContextBuilder.For(TriggerType.OnUnitDeath, TriggerPhase.OnDeath)
                .WithActor(killer)
                .WithTarget(victim)
                .Build();

            service.Execute(TriggerType.OnUnitDeath, TriggerPhase.OnDeath, context);
        }

        /// <summary>
        /// Execute Move triggers in all phases.
        /// </summary>
        public static void TriggerMove(this TriggerService service, Figure figure, MoveContext context)
        {
            foreach (TriggerPhase phase in TriggerPhases.MovementPipeline)
            {
                TriggerContext triggerContext = TriggerContextBuilder.For(TriggerType.OnMove, phase)
                    .WithActor(figure)
                    .WithData(context)
                    .Build();

                service.Execute(TriggerType.OnMove, phase, triggerContext);
            }
        }

        /// <summary>
        /// Execute TurnStart triggers for all figures on a team.
        /// </summary>
        public static void TriggerTurnStart(this TriggerService service, IFigureRegistry figureRegistry, TurnContext context)
        {
            foreach (Figure figure in figureRegistry.GetTeam(context.Team))
            {
                foreach (TriggerPhase phase in TriggerPhases.TurnPipeline)
                {
                    if (phase != TriggerPhase.OnTurnStart) continue;

                    TriggerContext figureContext = TriggerContextBuilder.For(TriggerType.OnTurnStart, phase)
                        .WithActor(figure)
                        .WithData(context)
                        .Build();

                    service.Execute(TriggerType.OnTurnStart, phase, figureContext);
                }
            }
        }

        /// <summary>
        /// Execute BattleStart triggers for a figure.
        /// </summary>
        public static void TriggerBattleStart(this TriggerService service, Figure figure)
        {
            TriggerContext context = TriggerContextBuilder.For(TriggerType.OnBattleStart, TriggerPhase.OnTurnStart)
                .WithActor(figure)
                .Build();

            service.Execute(TriggerType.OnBattleStart, TriggerPhase.OnTurnStart, context);
        }

        /// <summary>
        /// Execute BattleEnd triggers for a figure.
        /// </summary>
        public static void TriggerBattleEnd(this TriggerService service, Figure figure)
        {
            TriggerContext context = TriggerContextBuilder.For(TriggerType.OnBattleEnd, TriggerPhase.AfterTurn)
                .WithActor(figure)
                .Build();

            service.Execute(TriggerType.OnBattleEnd, TriggerPhase.AfterTurn, context);
        }

        /// <summary>
        /// Execute DamageReceived triggers.
        /// </summary>
        public static void TriggerDamageReceived(this TriggerService service, Figure target, int amount, Figure? source = null)
        {
            TriggerContext context = TriggerContextBuilder.For(TriggerType.OnDamageReceived, TriggerPhase.AfterApplication)
                .WithActor(source ?? target)
                .WithTarget(target)
                .WithValue(amount)
                .Build();

            service.Execute(TriggerType.OnDamageReceived, TriggerPhase.AfterApplication, context);
        }
    }
}
