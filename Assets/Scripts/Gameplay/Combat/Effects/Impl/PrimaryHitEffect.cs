using System;
using Project.Core.Core.Configs.Stats;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Combat.Damage;
using Project.Gameplay.Gameplay.Combat.Contexts;
using Project.Gameplay.Gameplay.Combat.Effects;
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
        private readonly int _baseDamage;
        private readonly string _attackId;
        private readonly DeliveryType _delivery;
        private readonly string _projectileConfigId;
        private readonly IDamageTokenStore _tokenStore;
        private readonly IDamagePipeline _damagePipeline;

        public PrimaryHitEffect(
            Figure attacker,
            Figure target,
            GridPosition attackerPosition,
            GridPosition targetPosition,
            int baseDamage,
            string attackId,
            DeliveryType delivery,
            string projectileConfigId,
            IDamageTokenStore tokenStore,
            IDamagePipeline damagePipeline)
        {
            _attacker = attacker;
            _target = target;
            _attackerPosition = attackerPosition;
            _targetPosition = targetPosition;
            _baseDamage = baseDamage;
            _attackId = attackId;
            _delivery = delivery;
            _projectileConfigId = projectileConfigId;
            _tokenStore = tokenStore;
            _damagePipeline = damagePipeline;
        }

        public void Apply(CombatEffectContext context)
        {
            var before = new BeforeHitContext
            {
                Attacker = _attacker,
                Target = _target,
                BaseDamage = _baseDamage
            };

            context.Passives.TriggerBeforeHit(_attacker, _target, before);

            int finalDamage = (int)(before.BaseDamage * before.DamageMultiplier) + before.BonusDamage;
            if (finalDamage < 0) finalDamage = 0;

            DamageResult damageResult = _damagePipeline.Calculate(new DamageContext(
                _attacker,
                _target,
                finalDamage,
                before.IsCritical,
                _attackId,
                Array.Empty<IDamageModifier>()));

            AddPrimaryDeliveryEvent(context, damageResult.Final, before.IsCritical);

            context.AddVisualEvent(new DamageVisualEvent(
                _target.Id,
                damageResult.Final,
                before.IsCritical,
                string.IsNullOrEmpty(_attackId) ? _delivery.ToString() : _attackId));

            bool isDeferred = IsDeferredDelivery(_delivery);
            bool died = isDeferred
                ? damageResult.Final >= _target.Stats.CurrentHp
                : _target.Stats.TakeDamage(damageResult.Final);

            if (!isDeferred)
            {
                context.ActionContext.LastDamageDealt = damageResult.Final;
            }

            context.Logger.Info($"{_attacker} hit {_target} for {finalDamage} damage. HP: {_target.Stats.CurrentHp}/{_target.Stats.MaxHp}");

            var after = new AfterHitContext
            {
                Attacker = _attacker,
                Target = _target,
                AttackerPosition = _attackerPosition,
                TargetPosition = _targetPosition,
                Grid = context.Grid,
                DamageDealt = damageResult.Final,
                TargetDied = died,
                WasCritical = before.IsCritical
            };

            context.Passives.TriggerAfterHit(_attacker, _target, after);
            foreach (ICombatEffect effect in after.Effects)
            {
                context.AddEffect(effect);
            }

            if (!isDeferred && died)
            {
                context.Passives.TriggerKill(_attacker, _target);
                context.Passives.TriggerDeath(_target, _attacker);

                BoardCell targetCell = context.Grid.GetBoardCell(_targetPosition);
                context.AddEffect(new KillEffect(_target, targetCell, "primary"));
            }
        }

        private void AddPrimaryDeliveryEvent(CombatEffectContext context, int finalDamage, bool isCritical)
        {
            string attackType = string.IsNullOrEmpty(_attackId) ? _delivery.ToString() : _attackId;

            switch (_delivery)
            {
                case DeliveryType.Projectile:
                    DamageToken token = CreateToken(finalDamage, isCritical);
                    _tokenStore.Add(token);
                    context.AddVisualEvent(new ProjectileVisualEvent(
                        _attacker.Id,
                        _attackerPosition,
                        _targetPosition,
                        _target.Id,
                        _projectileConfigId,
                        finalDamage,
                        isCritical,
                        null,
                        token.Id,
                        token.CreatedAt,
                        attackType));
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

        private static bool IsDeferredDelivery(DeliveryType delivery)
        {
            return delivery == DeliveryType.Projectile;
        }

        private DamageToken CreateToken(int finalDamage, bool isCritical)
        {
            Guid id = Guid.NewGuid();
            float createdAt = (float)DateTime.UtcNow.Subtract(DateTime.UnixEpoch).TotalSeconds;
            return new DamageToken(
                id,
                _attacker.Id,
                _target.Id,
                _targetPosition,
                finalDamage,
                createdAt,
                _delivery,
                isCritical,
                _attackId);
        }
    }
}
