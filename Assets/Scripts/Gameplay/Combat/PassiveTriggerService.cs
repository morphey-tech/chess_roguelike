using System;
using System.Linq;
using Project.Gameplay.Gameplay.Combat.Contexts;
using Project.Gameplay.Gameplay.Combat.Triggers;
using Project.Gameplay.Gameplay.Figures;

namespace Project.Gameplay.Gameplay.Combat
{
    public sealed class PassiveTriggerService
    {
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

        public void TriggerTurnStart(Figure figure, TurnContext context)
        {
            Execute<IOnTurnStart>(figure, p => p.OnTurnStart(context));
        }

        public void TriggerTurnEnd(Figure figure, TurnContext context)
        {
            Execute<IOnTurnEnd>(figure, p => p.OnTurnEnd(context));
        }

        private void Execute<TTrigger>(Figure figure, Action<TTrigger> action)
        {
            foreach (IPassive passive in figure.Passives.OrderBy(p => p.Priority))
            {
                if (passive is TTrigger trigger)
                    action(trigger);
            }
        }

        private void ExecuteWithOwner<TTrigger>(Figure owner, Action<TTrigger, Figure> action)
        {
            foreach (IPassive passive in owner.Passives.OrderBy(p => p.Priority))
            {
                if (passive is TTrigger trigger)
                    action(trigger, owner);
            }
        }
    }
}
