using System;
using Project.Core.Core.Configs.Stats;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Combat.Contexts;
using Project.Gameplay.Gameplay.Combat.Damage;
using Project.Gameplay.Gameplay.Combat.Visual;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Combat.Effects.Impl
{
    /// <summary>
    /// Applies primary hit logic and records corresponding visual events.
    /// </summary>
    public sealed class PrimaryHitEffect : ICombatEffect
    {
        public CombatEffectPhase Phase => CombatEffectPhase.Damage;
        public int OrderInPhase => 0;

        private readonly Figure _attacker;
        private readonly Figure _target;
        private readonly GridPosition _attackerPosition;
        private readonly GridPosition _targetPosition;
        private readonly float _baseDamage;
        private readonly string _attackId;
        private readonly DeliveryType _delivery;
        private readonly string _projectileConfigId;

        public PrimaryHitEffect(
            Figure attacker,
            Figure target,
            GridPosition attackerPosition,
            GridPosition targetPosition,
            float baseDamage,
            string attackId,
            DeliveryType delivery,
            string projectileConfigId)
        {
            _attacker = attacker;
            _target = target;
            _attackerPosition = attackerPosition;
            _targetPosition = targetPosition;
            _baseDamage = baseDamage;
            _attackId = attackId;
            _delivery = delivery;
            _projectileConfigId = projectileConfigId;
        }

        public void Apply(CombatEffectContext context)
        {
            BeforeHitContext before = new()
            {
                Attacker = _attacker,
                Target = _target,
                Grid = context.Grid,
                BaseDamage = _baseDamage
            };

            // Trigger passives FIRST - they apply modifiers to stats
            context.Passives.TriggerBeforeHit(_attacker, _target, before);
            context.ActionContext.Actor.Effects.TriggerBeforeHit(before);

            // NOW calculate final damage after all modifiers are applied
            float atk = _attacker.Stats.Attack.Value;
            float def = _target.Stats.Defence.Value;
            float finalDamage = Math.Max(1f, atk - def);
            
            // Apply multiplier and bonus damage from passives
            finalDamage = finalDamage * before.DamageMultiplier + before.BonusDamage;
            
            if (finalDamage < 0)
            {
                finalDamage = 0;
            }

            bool isProjectile = _delivery == DeliveryType.Projectile;

            if (!isProjectile)
            {
                string attackType = string.IsNullOrEmpty(_attackId) ? _delivery.ToString() : _attackId;
                DamageContext damageContext = new(
                    _attacker,
                    _target,
                    finalDamage,
                    before.IsCritical,
                    before.IsDodged,
                    before.IsCancelled,
                    attackType,
                    Array.Empty<IDamageModifier>());
                (DamageResult damageResult, bool died) = context.DamageApplier.ApplyNoDeath(damageContext);
                context.ActionContext.LastDamageDealt = damageResult.Final;

                AddPrimaryDeliveryEvent(context, damageResult.Final, before.IsCritical);

                context.AddVisualEvent(new DamageVisualEvent(
                    _target.Id,
                    damageResult.Final,
                    before.IsCritical,
                    before.IsDodged,
                    string.IsNullOrEmpty(_attackId) ? _delivery.ToString() : _attackId));

                context.Logger.Info($"{_attacker} hit {_target} for {damageResult.Final} damage. HP: {_target.Stats.CurrentHp}/{_target.Stats.MaxHp}");

                AfterHitContext after = new()
                {
                    Attacker = _attacker,
                    Target = _target,
                    AttackerPosition = _attackerPosition,
                    TargetPosition = _targetPosition,
                    Grid = context.Grid,
                    DamageDealt = damageResult.Final,
                    TargetDied = died,
                    WasCritical = before.IsCritical,
                    WasDodged = before.IsDodged
                };

                context.Passives.TriggerAfterHit(_attacker, _target, after);
                context.ActionContext.Actor.Effects.TriggerAfterHit(after);
                foreach (ICombatEffect effect in after.Effects)
                {
                    context.AddEffect(effect);
                }

                if (died)
                {
                    context.Passives.TriggerKill(_attacker, _target);
                    context.Passives.TriggerDeath(_target, _attacker);

                    BoardCell targetCell = context.Grid.GetBoardCell(_targetPosition);
                    context.FigureLifeService.HandleDeathFromCombat(context, _target, targetCell);
                }
            }
            else
            {
                // Defer actual damage application to ProjectileHitApplyService.
                // Event carries raw base-for-pipeline damage and crit flag; pipeline is applied once on hit.
                AddPrimaryDeliveryEvent(context, finalDamage, before.IsCritical);
                context.AddVisualEvent(new ProjectileHitApplyEvent(
                    _attacker.Id,
                    _target.Id,
                    _targetPosition,
                    finalDamage,
                    before.IsCritical,
                    before.IsDodged,
                    before.IsCancelled,
                    _attackId));

                context.Logger.Info($"{_attacker} projectile hit (deferred): raw={finalDamage} to {_target}. HP: {_target.Stats.CurrentHp}/{_target.Stats.MaxHp}");
            }
        }

        private void AddPrimaryDeliveryEvent(CombatEffectContext context, float finalDamage, bool isCritical)
        {
            string attackType = string.IsNullOrEmpty(_attackId) ? _delivery.ToString() : _attackId;

            switch (_delivery)
            {
                case DeliveryType.Projectile:
                    context.AddVisualEvent(new ProjectileVisualEvent(
                        _attacker.Id,
                        _attackerPosition,
                        _targetPosition,
                        _target.Id,
                        _projectileConfigId,
                        finalDamage,
                        isCritical,
                        null,
                        attackType));
                    context.AddVisualEvent(new ProjectileImpactEvent(_targetPosition, null));
                    context.AddVisualEvent(new CleanupProjectileEvent());
                    break;
                case DeliveryType.Beam:
                    context.AddVisualEvent(new BeamVisualEvent(
                        _attacker.Id,
                        _attackerPosition,
                        _targetPosition,
                        _target.Id,
                        attackType));
                    break;
                case DeliveryType.Wave:
                    context.AddVisualEvent(new WaveVisualEvent(
                        _attacker.Id,
                        _attackerPosition,
                        _targetPosition,
                        _target.Id,
                        attackType));
                    break;
                default:
                    context.AddVisualEvent(new AttackVisualEvent(
                        _attacker.Id,
                        _targetPosition,
                        attackType));
                    break;
            }
        }

    }
}
