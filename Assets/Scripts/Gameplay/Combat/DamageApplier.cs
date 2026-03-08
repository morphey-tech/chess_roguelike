using Cysharp.Threading.Tasks;
using Project.Core.Core.Triggers;
using Project.Gameplay.Gameplay.Combat.Damage;
using Project.Gameplay.Gameplay.Combat.Effects;
using Project.Gameplay.Gameplay.Grid;
using VContainer;

namespace Project.Gameplay.Gameplay.Combat
{
    /// <summary>
    /// Единая точка применения урона: pipeline → TakeDamage → при смерти LifeService.
    /// Ближний бой и снаряд вызывают только Apply (или ApplyDirect).
    /// </summary>
    public sealed class DamageApplier
    {
        private readonly IDamagePipeline _pipeline;
        private readonly TriggerService _triggerService;
        private readonly IFigureLifeService _lifeService;

        [Inject]
        private DamageApplier(
            IDamagePipeline pipeline,
            TriggerService triggerService,
            IFigureLifeService lifeService)
        {
            _pipeline = pipeline;
            _triggerService = triggerService;
            _lifeService = lifeService;
        }

        /// <summary>
        /// Применить урон в контексте боя. Возвращает (результат, умер ли цель).
        /// Визуал урона добавляет вызывающий эффект; смерть обрабатывает LifeService (события в контекст).
        /// </summary>
        public (DamageResult result, bool died) Apply(CombatEffectContext context, DamageContext dmgCtx)
        {
            DamageResult result = _pipeline.Calculate(dmgCtx);
            if (result.Cancelled)
            {
                return (result, false);
            }

            bool died = dmgCtx.Target.Stats.TakeDamage(result.Final);

            if (died)
            {
                _triggerService.TriggerKill(dmgCtx.Attacker, dmgCtx.Target);
                _triggerService.TriggerDeath(dmgCtx.Target, dmgCtx.Attacker);

                BoardCell cell = context.Grid.FindFigure(dmgCtx.Target);
                _lifeService.HandleDeathFromCombat(context, dmgCtx.Target, cell);
            }

            return (result, died);
        }

        /// <summary>
        /// Применить урон без обработки смерти (только pipeline + TakeDamage).
        /// Нужен для фаз, где death-flow выполняется позже в фиксированном порядке.
        /// </summary>
        public (DamageResult result, bool died) ApplyNoDeath(DamageContext dmgCtx)
        {
            DamageResult result = _pipeline.Calculate(dmgCtx);
            if (result.Cancelled)
            {
                return (result, false);
            }
            bool died = dmgCtx.Target.Stats.TakeDamage(result.Final);
            return (result, died);
        }

        /// <summary>
        /// Только урон + домен смерти (снять с доски, пассивки, Publish). Без визуала/лута — их добавляют в очередь снаружи.
        /// </summary>
        public (DamageResult result, bool died) ApplyDamageOnly(DamageContext dmgCtx, BoardCell targetCell)
        {
            DamageResult result = _pipeline.Calculate(dmgCtx);
            if (result.Cancelled)
            {
                return (result, false);
            }

            bool died = dmgCtx.Target.Stats.TakeDamage(result.Final);
            if (died)
            {
                _triggerService.TriggerKill(dmgCtx.Attacker, dmgCtx.Target);
                _triggerService.TriggerDeath(dmgCtx.Target, dmgCtx.Attacker);
                _lifeService.HandleDeathDomainOnly(dmgCtx.Target, targetCell);
            }

            return (result, died);
        }

        /// <summary>
        /// Применить урон вне цепочки и сразу визуал/лут (fallback, когда нельзя дописать в очередь).
        /// </summary>
        public async UniTask<(DamageResult result, bool died)> ApplyDirectAsync(DamageContext dmgCtx, BoardCell targetCell)
        {
            (DamageResult result, bool died) = ApplyDamageOnly(dmgCtx, targetCell);
            if (died)
            {
                await _lifeService.HandleDeathDirectAsync(dmgCtx.Target, targetCell);
            }
            return (result, died);
        }
    }
}
