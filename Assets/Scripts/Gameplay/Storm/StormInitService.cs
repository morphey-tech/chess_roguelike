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
    public sealed class StormInitService : IDisposable
    {
        private readonly StormBattleService _stormBattle;
        private readonly ILogger<StormInitService> _logger;
        private readonly IDisposable _subscriptions;
        private string? _currentStageZoneConfigId;

        [Inject]
        private StormInitService(
            StormBattleService stormBattle,
            ISubscriber<PhaseStartedMessage> phaseStartedSubscriber,
            ISubscriber<PhaseCompletedMessage> phaseCompletedSubscriber,
            ILogService logService)
        {
            _stormBattle = stormBattle;
            _logger = logService.CreateLogger<StormInitService>();

            DisposableBagBuilder bag = DisposableBag.CreateBuilder();
            phaseStartedSubscriber.Subscribe(OnPhaseStarted).AddTo(bag);
            phaseCompletedSubscriber.Subscribe(OnPhaseCompleted).AddTo(bag);
            _subscriptions = bag.Build();
        }

        public void SetCurrentStageZoneConfig(string? zoneConfigId)
        {
            _currentStageZoneConfigId = zoneConfigId;
            _logger.Info($"Stage zone config set to: {zoneConfigId ?? "null"}");
        }

        private void OnPhaseStarted(PhaseStartedMessage msg)
        {
            _logger.Debug($"Phase started: {msg.PhaseId}");
        }

        private void OnPhaseCompleted(PhaseCompletedMessage msg)
        {
            _logger.Debug($"Phase completed: {msg.PhaseId}");
            if (msg.PhaseId == PhaseIds.BoardSpawn)
            {
                _logger.Info("BoardSpawnPhase completed, initializing zone");
                _stormBattle.InitializeForStage(_currentStageZoneConfigId).Forget();
            }
        }

        void IDisposable.Dispose()
        {
            _subscriptions?.Dispose();
        }
    }
}
