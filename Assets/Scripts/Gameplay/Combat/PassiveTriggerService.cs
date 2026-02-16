using System;
using System.Collections.Generic;
using System.Linq;
using Project.Gameplay.Gameplay.Combat.Contexts;
using Project.Gameplay.Gameplay.Combat.Triggers;
using Project.Gameplay.Gameplay.Economy;
using Project.Gameplay.Gameplay.Figures;
using VContainer;

namespace Project.Gameplay.Gameplay.Combat
{
    public sealed class PassiveTriggerService
    {
        private readonly IFigureRegistry _figureRegistry;
        private readonly EconomyService _economyService;

        [Inject]
        private PassiveTriggerService(IFigureRegistry figureRegistry, EconomyService economyService)
        {
            _figureRegistry = figureRegistry;
            _economyService = economyService;
        }

        public void TriggerBeforeHit(Figure attacker, Figure target, BeforeHitContext context)
        {
            ExecuteWithOwner<IOnBeforeHit>(attacker, (p, owner) => p.OnBeforeHit(owner, context));
            ExecuteWithOwner<IOnBeforeHit>(target, (p, owner) => p.OnBeforeHit(owner, context));
        }

        public void TriggerAfterHit(Figure attacker, Figure target, AfterHitContext context)
        {
            ExecuteWithOwner<IOnAfterHit>(attacker, (p, owner) => p.OnAfterHit(owner, context));
            ExecuteWithOwner<IOnAfterHit>(target, (p, owner) => p.OnAfterHit(owner, context));
        }

        public void TriggerKill(Figure killer, Figure victim)
        {
            var context = new KillContext { Killer = killer, Victim = victim };
            Execute<IOnKill>(killer, p => p.OnKill(context));
        }

        public void TriggerDeath(Figure victim, Figure killer)
        {
            var context = new DeathContext { Victim = victim, Killer = killer };
            Execute<IOnDeath>(victim, p => p.OnDeath(context));
        }

        public void TriggerMove(Figure figure, MoveContext context)
        {
            Execute<IOnMove>(figure, p => p.OnMove(context));
        }

        public void TriggerTurnStart(TurnContext context)
        {
            IEnumerable<Figure> team = _figureRegistry.GetTeam(context.Team);
            foreach (Figure figure in team)
            {
                Execute<IOnTurnStart>(figure, p => p.OnTurnStart(figure, context));
            }
        }

        public void TriggerTurnEnd(TurnContext context)
        {
            IEnumerable<Figure> team = _figureRegistry.GetTeam(context.Team);
            foreach (Figure figure in team)
            {
                 Execute<IOnTurnEnd>(figure, p => p.OnTurnEnd(figure, context));
            }
        }

        /// <summary>
        /// Gathers all passives for a figure: its own + global item passives.
        /// </summary>
        private IEnumerable<IPassive> GetAllPassives(Figure figure)
        {
            IEnumerable<IPassive> figurePassives = figure.Passives;
            IReadOnlyList<IPassive> itemPassives = _economyService.GetAllItemPassives();

            return itemPassives.Count > 0
                ? figurePassives.Concat(itemPassives)
                : figurePassives;
        }

        private void Execute<TTrigger>(Figure figure, Action<TTrigger> action)
        {
            foreach (IPassive passive in GetAllPassives(figure).OrderBy(p => p.Priority))
            {
                if (passive is TTrigger trigger)
                    action(trigger);
            }
        }

        private void ExecuteWithOwner<TTrigger>(Figure owner, Action<TTrigger, Figure> action)
        {
            foreach (IPassive passive in GetAllPassives(owner).OrderBy(p => p.Priority))
            {
                if (passive is TTrigger trigger)
                    action(trigger, owner);
            }
        }
    }
}
