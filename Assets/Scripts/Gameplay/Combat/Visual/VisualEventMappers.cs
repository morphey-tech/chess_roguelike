using System;
using System.Collections.Generic;
using Project.Gameplay.Gameplay.Visual.Commands;
using Project.Gameplay.Gameplay.Visual.Commands.Contexts;
using Project.Gameplay.Gameplay.Visual.Commands.Impl;

namespace Project.Gameplay.Gameplay.Combat.Visual
{
    public sealed class AttackVisualEventMapper : IVisualEventMapper
    {
        public Type EventType => typeof(AttackVisualEvent);
        public IEnumerable<IVisualCommand> Map(ICombatVisualEvent visualEvent)
        {
            var evt = (AttackVisualEvent)visualEvent;
            yield return new AttackCommand(new AttackVisualContext(
                evt.AttackerId,
                evt.TargetPosition,
                evt.AttackType));
        }
    }

    public sealed class ProjectileVisualEventMapper : IVisualEventMapper
    {
        public Type EventType => typeof(ProjectileVisualEvent);
        public IEnumerable<IVisualCommand> Map(ICombatVisualEvent visualEvent)
        {
            var evt = (ProjectileVisualEvent)visualEvent;
            yield return new FlyProjectileCommand(new ProjectileVisualContext(
                evt.AttackerId,
                evt.TargetId,
                evt.From,
                evt.To,
                evt.ProjectileConfigId,
                evt.Damage,
                evt.IsCritical,
                evt.ImpactFxId,
                evt.AttackType));
        }
    }

    public sealed class ProjectileImpactEventMapper : IVisualEventMapper
    {
        public Type EventType => typeof(ProjectileImpactEvent);
        public IEnumerable<IVisualCommand> Map(ICombatVisualEvent visualEvent)
        {
            var evt = (ProjectileImpactEvent)visualEvent;
            yield return new ImpactCommand(new ImpactVisualContext(evt.Position, evt.ImpactFxId));
        }
    }

    public sealed class CleanupProjectileEventMapper : IVisualEventMapper
    {
        public Type EventType => typeof(CleanupProjectileEvent);
        public IEnumerable<IVisualCommand> Map(ICombatVisualEvent visualEvent)
        {
            yield return new CleanupProjectileCommand();
        }
    }

    public sealed class ProjectileHitApplyEventMapper : IVisualEventMapper
    {
        public Type EventType => typeof(ProjectileHitApplyEvent);
        public IEnumerable<IVisualCommand> Map(ICombatVisualEvent visualEvent)
        {
            var evt = (ProjectileHitApplyEvent)visualEvent;
            yield return new ProjectileHitApplyCommand(evt);
        }
    }

    public sealed class BeamVisualEventMapper : IVisualEventMapper
    {
        public Type EventType => typeof(BeamVisualEvent);
        public IEnumerable<IVisualCommand> Map(ICombatVisualEvent visualEvent)
        {
            var evt = (BeamVisualEvent)visualEvent;
            yield return new BeamCommand(new BeamVisualContext(
                evt.AttackerId,
                evt.TargetId,
                evt.From,
                evt.To,
                evt.AttackType));
        }
    }

    public sealed class WaveVisualEventMapper : IVisualEventMapper
    {
        public Type EventType => typeof(WaveVisualEvent);
        public IEnumerable<IVisualCommand> Map(ICombatVisualEvent visualEvent)
        {
            var evt = (WaveVisualEvent)visualEvent;
            yield return new WaveCommand(new WaveVisualContext(
                evt.AttackerId,
                evt.TargetId,
                evt.From,
                evt.To,
                evt.AttackType));
        }
    }

    public sealed class DamageVisualEventMapper : IVisualEventMapper
    {
        public Type EventType => typeof(DamageVisualEvent);
        public IEnumerable<IVisualCommand> Map(ICombatVisualEvent visualEvent)
        {
            var evt = (DamageVisualEvent)visualEvent;
            
            yield return new DamageTextCommand(new DamageVisualContext(
                evt.TargetId,
                evt.Amount,
                evt.IsCritical,
                evt.DamageType));
            
            yield return new DamageCommand(new DamageVisualContext(
                evt.TargetId,
                evt.Amount,
                evt.IsCritical,
                evt.DamageType));
        }
    }

    public sealed class HealVisualEventMapper : IVisualEventMapper
    {
        public Type EventType => typeof(HealVisualEvent);
        public IEnumerable<IVisualCommand> Map(ICombatVisualEvent visualEvent)
        {
            var evt = (HealVisualEvent)visualEvent;
            yield return new HealCommand(new HealVisualContext(evt.TargetId, evt.Amount));
        }
    }

    public sealed class PushVisualEventMapper : IVisualEventMapper
    {
        public Type EventType => typeof(PushVisualEvent);
        public IEnumerable<IVisualCommand> Map(ICombatVisualEvent visualEvent)
        {
            var evt = (PushVisualEvent)visualEvent;
            yield return new PushCommand(new PushVisualContext(evt.TargetId, evt.From, evt.To));
        }
    }

    public sealed class MoveVisualEventMapper : IVisualEventMapper
    {
        public Type EventType => typeof(MoveVisualEvent);
        public IEnumerable<IVisualCommand> Map(ICombatVisualEvent visualEvent)
        {
            var evt = (MoveVisualEvent)visualEvent;
            yield return new MoveCommand(new MoveVisualContext(evt.FigureId, evt.To));
        }
    }

    public sealed class DeathVisualEventMapper : IVisualEventMapper
    {
        public Type EventType => typeof(DeathVisualEvent);
        public IEnumerable<IVisualCommand> Map(ICombatVisualEvent visualEvent)
        {
            var evt = (DeathVisualEvent)visualEvent;
            yield return new DeathCommand(new DeathVisualContext(evt.FigureId, evt.Reason));
        }
    }

    public sealed class LootVisualEventMapper : IVisualEventMapper
    {
        public Type EventType => typeof(LootVisualEvent);
        public IEnumerable<IVisualCommand> Map(ICombatVisualEvent visualEvent)
        {
            var evt = (LootVisualEvent)visualEvent;
            yield return new LootCommand(new LootVisualContext(evt.DropPosition, evt.Loot));
        }
    }
}
