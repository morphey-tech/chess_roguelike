using System;
using System.Collections.Generic;
using System.Linq;
using Project.Gameplay.Gameplay.Artifacts.Triggers;
using Project.Core.Core.Logging;
using VContainer;

namespace Project.Gameplay.Gameplay.Artifacts
{
    /// <summary>
    /// Triggers artifact effects on game events (battle start, kill, death, etc.).
    /// Similar to PassiveTriggerService but for artifacts.
    /// </summary>
    public sealed class ArtifactTriggerService
    {
        private readonly ArtifactService _artifactService;
        private readonly ILogger<ArtifactTriggerService> _logger;

        [Inject]
        private ArtifactTriggerService(ArtifactService artifactService, ILogService logService)
        {
            _artifactService = artifactService;
            _logger = logService.CreateLogger<ArtifactTriggerService>();
        }

        public void TriggerBattleStart()
        {
            _logger.Debug("Triggering OnBattleStart artifacts");
            
            foreach (ArtifactInstance? instance in _artifactService.Artifacts)
            {
                if (instance.Artifact is IOnBattleStart trigger)
                {
                    try
                    {
                        trigger.OnBattleStart(new BattleContext(0));
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Error in artifact {instance.ConfigId}.OnBattleStart: {ex.Message}");
                    }
                }
            }
        }

        public void TriggerBattleEnd()
        {
            _logger.Debug("Triggering OnBattleEnd artifacts");
            
            foreach (ArtifactInstance? instance in _artifactService.Artifacts)
            {
                if (instance.Artifact is IOnBattleEnd trigger)
                {
                    try
                    {
                        trigger.OnBattleEnd(new BattleContext(0));
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Error in artifact {instance.ConfigId}.OnBattleEnd: {ex.Message}");
                    }
                }
            }
        }

        public void TriggerUnitKill(int killerId, int victimId)
        {
            _logger.Debug($"Triggering OnUnitKill artifacts (killer={killerId}, victim={victimId})");
            
            foreach (ArtifactInstance? instance in _artifactService.Artifacts)
            {
                if (instance.Artifact is IOnUnitKill trigger)
                {
                    try
                    {
                        trigger.OnUnitKill(new KillContext(killerId, victimId));
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Error in artifact {instance.ConfigId}.OnUnitKill: {ex.Message}");
                    }
                }
            }
        }

        public void TriggerUnitDeath(int victimId, int? killerId = null)
        {
            _logger.Debug($"Triggering OnUnitDeath artifacts (victim={victimId})");
            
            foreach (ArtifactInstance? instance in _artifactService.Artifacts)
            {
                if (instance.Artifact is IOnUnitDeath trigger)
                {
                    try
                    {
                        trigger.OnUnitDeath(new DeathContext(victimId, killerId));
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Error in artifact {instance.ConfigId}.OnUnitDeath: {ex.Message}");
                    }
                }
            }
        }

        public void TriggerAllyDeath(int allyId, int? killerId = null)
        {
            _logger.Debug($"Triggering OnAllyDeath artifacts (ally={allyId})");
            
            foreach (ArtifactInstance? instance in _artifactService.Artifacts)
            {
                if (instance.Artifact is IOnAllyDeath trigger)
                {
                    try
                    {
                        trigger.OnAllyDeath(new DeathContext(allyId, killerId));
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Error in artifact {instance.ConfigId}.OnAllyDeath: {ex.Message}");
                    }
                }
            }
        }

        public void TriggerDamageReceived(int targetId, int amount)
        {
            _logger.Debug($"Triggering OnDamageReceived artifacts (target={targetId}, amount={amount})");
            
            foreach (ArtifactInstance? instance in _artifactService.Artifacts)
            {
                if (instance.Artifact is IOnDamageReceived trigger)
                {
                    try
                    {
                        trigger.OnDamageReceived(new DamageContext(targetId, amount));
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Error in artifact {instance.ConfigId}.OnDamageReceived: {ex.Message}");
                    }
                }
            }
        }

        public void TriggerReward(int choicesCount)
        {
            _logger.Debug($"Triggering OnReward artifacts (choices={choicesCount})");
            
            foreach (ArtifactInstance? instance in _artifactService.Artifacts)
            {
                if (instance.Artifact is IOnReward trigger)
                {
                    try
                    {
                        trigger.OnReward(new RewardContext(choicesCount));
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Error in artifact {instance.ConfigId}.OnReward: {ex.Message}");
                    }
                }
            }
        }
    }
}
