using System;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Stage.Messages;
using Project.Gameplay.Gameplay.Stage.Phase;
using VContainer;
using VContainer.Unity;

namespace Project.Gameplay.ShrinkingZone
{
    /// <summary>
    /// Сервис, который инициализирует зону после завершения BoardSpawnPhase
    /// </summary>
    public sealed class ZoneInitService : IDisposable
    {
        private readonly ZoneBattleService _zoneBattle;
        private readonly ILogger<ZoneInitService> _logger;
        private readonly IDisposable _subscriptions;
        private string? _currentStageZoneConfigId;

        [Inject]
        public ZoneInitService(
            ZoneBattleService zoneBattle,
            ISubscriber<PhaseStartedMessage> phaseStartedSubscriber,
            ISubscriber<PhaseCompletedMessage> phaseCompletedSubscriber,
            ILogService logService)
        {
            _zoneBattle = zoneBattle;
            _logger = logService.CreateLogger<ZoneInitService>();

            DisposableBagBuilder bag = DisposableBag.CreateBuilder();
            phaseStartedSubscriber.Subscribe(OnPhaseStarted).AddTo(bag);
            phaseCompletedSubscriber.Subscribe(OnPhaseCompleted).AddTo(bag);
            _subscriptions = bag.Build();
        }

        public void SetCurrentStageZoneConfig(string? zoneConfigId)
        {
            _currentStageZoneConfigId = zoneConfigId;
            _logger.Info($"[ZONE INIT] Stage zone config set to: {zoneConfigId ?? "null"}");
        }

        private void OnPhaseStarted(PhaseStartedMessage msg)
        {
            _logger.Debug($"[ZONE INIT] Phase started: {msg.PhaseId}");
        }

        private void OnPhaseCompleted(PhaseCompletedMessage msg)
        {
            _logger.Debug($"[ZONE INIT] Phase completed: {msg.PhaseId}");
            if (msg.PhaseId == PhaseIds.BoardSpawn)
            {
                _logger.Info("[ZONE INIT] BoardSpawnPhase completed, initializing zone");
                _zoneBattle.InitializeForStage(_currentStageZoneConfigId).Forget();
            }
        }

        void IDisposable.Dispose()
        {
            _subscriptions?.Dispose();
        }
    }
}
