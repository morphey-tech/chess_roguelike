using System;
using System.Collections.Generic;
using System.Linq;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Artifacts.Triggers;

namespace Project.Gameplay.Gameplay.Artifacts
{
    /// <summary>
    /// Triggers artifact effects on game events.
    /// Uses cached trigger map for optimal performance.
    /// </summary>
    public sealed class ArtifactTriggerService
    {
        private readonly ArtifactService _artifactService;
        private readonly ILogger<ArtifactTriggerService> _logger;
        
        /// <summary>
        /// Cache: Trigger type → list of artifacts that respond to it.
        /// Rebuilt when artifacts change.
        /// </summary>
        private readonly Dictionary<ArtifactTriggerType, List<IArtifact>> _triggerMap = new();
        
        /// <summary>
        /// Cache is dirty and needs rebuild.
        /// </summary>
        private bool _isDirty = true;

        public ArtifactTriggerService(ArtifactService artifactService, ILogService logService)
        {
            _artifactService = artifactService;
            _logger = logService.CreateLogger<ArtifactTriggerService>();
            
            // Subscribe to artifact changes to invalidate cache
            // Note: Would need events on ArtifactService for this
        }

        /// <summary>
        /// Universal trigger method for all artifact events.
        /// Uses cached trigger map for O(1) lookup + O(n) execution where n = matching artifacts.
        /// </summary>
        public void Trigger(ArtifactTriggerContext context)
        {
            // Rebuild cache if dirty
            if (_isDirty)
            {
                RebuildCache();
            }

            // Fast lookup: only execute artifacts that respond to this trigger
            if (_triggerMap.TryGetValue(context.Trigger, out var list))
            {
                foreach (var artifact in list)
                {
                    try
                    {
                        ExecuteArtifact(artifact, context);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Error in artifact {artifact.ConfigId}: {ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Rebuild the trigger cache from current artifacts.
        /// Artifacts are sorted by priority for correct execution order.
        /// </summary>
        private void RebuildCache()
        {
            _triggerMap.Clear();
            _isDirty = false;

            foreach (var instance in _artifactService.Artifacts)
            {
                CacheArtifact(instance.Artifact);
            }

            // Sort all cached lists by priority (lower = first)
            foreach (var kvp in _triggerMap)
            {
                kvp.Value.Sort((a, b) => a.Priority.CompareTo(b.Priority));
            }

            _logger.Debug($"Artifact trigger cache rebuilt: {_triggerMap.Count} trigger types");
        }

        /// <summary>
        /// Cache a single artifact by its trigger interfaces.
        /// </summary>
        private void CacheArtifact(IArtifact artifact)
        {
            // Check each trigger interface and add to appropriate lists
            if (artifact is IOnBattleStart) AddToCache(ArtifactTriggerType.OnBattleStart, artifact);
            if (artifact is IOnBattleEnd) AddToCache(ArtifactTriggerType.OnBattleEnd, artifact);
            if (artifact is IOnUnitKill) AddToCache(ArtifactTriggerType.OnUnitKill, artifact);
            if (artifact is IOnUnitDeath) AddToCache(ArtifactTriggerType.OnUnitDeath, artifact);
            if (artifact is IOnAllyDeath) AddToCache(ArtifactTriggerType.OnAllyDeath, artifact);
            if (artifact is IOnDamageReceived) AddToCache(ArtifactTriggerType.OnDamageReceived, artifact);
            if (artifact is IOnReward) AddToCache(ArtifactTriggerType.OnReward, artifact);
            if (artifact is IOnMove) AddToCache(ArtifactTriggerType.OnMove, artifact);
            if (artifact is IOnAttack) AddToCache(ArtifactTriggerType.OnAttack, artifact);
            if (artifact is IOnTurnStart) AddToCache(ArtifactTriggerType.OnTurnStart, artifact);
            if (artifact is IOnTurnEnd) AddToCache(ArtifactTriggerType.OnTurnEnd, artifact);
        }

        private void AddToCache(ArtifactTriggerType trigger, IArtifact artifact)
        {
            if (!_triggerMap.ContainsKey(trigger))
            {
                _triggerMap[trigger] = new List<IArtifact>();
            }
            _triggerMap[trigger].Add(artifact);
        }

        /// <summary>
        /// Execute a single artifact with context.
        /// Passes stack count for stacked effects.
        /// </summary>
        private void ExecuteArtifact(IArtifact artifact, ArtifactTriggerContext context)
        {
            // Include stack count in context
            context.StackCount = GetArtifactStack(artifact);
            
            switch (context.Trigger)
            {
                case ArtifactTriggerType.OnBattleStart:
                    if (artifact is IOnBattleStart battleStart)
                        battleStart.OnBattleStart(context);
                    break;

                case ArtifactTriggerType.OnBattleEnd:
                    if (artifact is IOnBattleEnd battleEnd)
                        battleEnd.OnBattleEnd(context);
                    break;

                case ArtifactTriggerType.OnUnitKill:
                    if (artifact is IOnUnitKill unitKill)
                        unitKill.OnUnitKill(context);
                    break;

                case ArtifactTriggerType.OnUnitDeath:
                    if (artifact is IOnUnitDeath unitDeath)
                        unitDeath.OnUnitDeath(context);
                    break;

                case ArtifactTriggerType.OnAllyDeath:
                    if (artifact is IOnAllyDeath allyDeath)
                        allyDeath.OnAllyDeath(context);
                    break;

                case ArtifactTriggerType.OnDamageReceived:
                    if (artifact is IOnDamageReceived damageReceived)
                        damageReceived.OnDamageReceived(context);
                    break;

                case ArtifactTriggerType.OnReward:
                    if (artifact is IOnReward reward)
                        reward.OnReward(context);
                    break;

                case ArtifactTriggerType.OnMove:
                    if (artifact is IOnMove move)
                        move.OnMove(context);
                    break;

                case ArtifactTriggerType.OnAttack:
                    if (artifact is IOnAttack attack)
                        attack.OnAttack(context);
                    break;

                case ArtifactTriggerType.OnTurnStart:
                    if (artifact is IOnTurnStart turnStart)
                        turnStart.OnTurnStart(context);
                    break;

                case ArtifactTriggerType.OnTurnEnd:
                    if (artifact is IOnTurnEnd turnEnd)
                        turnEnd.OnTurnEnd(context);
                    break;
            }
        }

        private int GetArtifactStack(IArtifact artifact)
        {
            ArtifactInstance? instance = _artifactService.Artifacts
                .FirstOrDefault(a => a.Artifact == artifact);
            return instance?.Stack ?? 1;
        }

        /// <summary>
        /// Invalidate cache (call when artifacts change).
        /// </summary>
        public void InvalidateCache()
        {
            _isDirty = true;
        }

        // Convenience methods for common triggers

        public void TriggerBattleStart(int teamId = 0)
        {
            Trigger(ArtifactTriggerContext.CreateBattle(teamId));
        }

        public void TriggerBattleEnd(int teamId = 0)
        {
            Trigger(new ArtifactTriggerContext
            {
                Trigger = ArtifactTriggerType.OnBattleEnd,
                Value = teamId
            });
        }

        public void TriggerUnitKill(Figures.Figure killer, Figures.Figure victim)
        {
            Trigger(ArtifactTriggerContext.CreateKill(killer, victim));
        }

        public void TriggerUnitDeath(Figures.Figure victim, Figures.Figure? killer = null)
        {
            Trigger(ArtifactTriggerContext.CreateDeath(victim, killer));
        }

        public void TriggerAllyDeath(Figures.Figure ally, Figures.Figure? killer = null)
        {
            Trigger(ArtifactTriggerContext.CreateDeath(ally, killer));
        }

        public void TriggerDamageReceived(Figures.Figure target, int amount, Figures.Figure? source = null)
        {
            Trigger(ArtifactTriggerContext.CreateDamage(target, amount, source));
        }
    }
}
