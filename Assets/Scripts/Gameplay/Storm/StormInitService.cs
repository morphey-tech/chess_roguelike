using System;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Stage.Messages;
using Project.Gameplay.Gameplay.Stage.Phase;
using VContainer;

namespace Project.Gameplay.ShrinkingZone
{
    /// <summary>
    /// Сервис, который инициализирует зону после завершения BoardSpawnPhase
    /// </summary>
    public sealed class StormInitService : IDisposable
    {
        private readonly StormBattleService _stormBattle;
        private readonly ILogger<StormInitService> _logger;
        private readonly IDisposable _disposable;
        private string? _currentStageZoneConfigId;

        [Inject]
        private StormInitService(
            StormBattleService stormBattle,
            ISubscriber<string, StagePhaseMessage> stagePhaseSubscriber,
            ILogService logService)
        {
            _stormBattle = stormBattle;
            _logger = logService.CreateLogger<StormInitService>();

            DisposableBagBuilder bag = DisposableBag.CreateBuilder();
            stagePhaseSubscriber.Subscribe(StagePhaseMessage.PHASE_COMPLETED, OnStagePhase).AddTo(bag);
            _disposable = bag.Build();
        }

        public void Configure(string? zoneConfigId)
        {
            _currentStageZoneConfigId = zoneConfigId;
            _logger.Info($"Stage zone config set to: {zoneConfigId ?? "null"}");
        }

        private void OnStagePhase(StagePhaseMessage msg)
        {
            if (msg.PhaseId != PhaseIds.BoardSpawn)
            {
                return;
            }
            _stormBattle.InitializeForStage(_currentStageZoneConfigId).Forget();
            _logger.Info("BoardSpawnPhase completed, initializing zone");
        }

        void IDisposable.Dispose()
        {
            _disposable?.Dispose();
        }
    }
}
