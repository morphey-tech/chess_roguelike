using System;
using System.Collections.Generic;
using System.Linq;
using Project.Core.Logging;
using Project.Core.Quest;
using UniRx;
using VContainer;

namespace Project.Gameplay.Quest
{
    public class QuestService : IQuestService, IDisposable
    {
        private readonly Dictionary<string, QuestData> _questDatabase = new();
        private readonly Dictionary<string, QuestState> _questStates = new();
        private readonly Dictionary<string, QuestData> _activeQuestData = new();
        private readonly Subject<QuestEventData> _onQuestEvent = new();
        private readonly ILogger _logger;
        private bool _disposed;
        
        public IObservable<QuestEventData> OnQuestEvent => _onQuestEvent;
        
        [Inject]
        public QuestService(ILogService logService)
        {
            _logger = logService.CreateLogger<QuestService>();
            _logger.Info("Initialized");
        }
        
        public void RegisterQuest(QuestData questData)
        {
            _questDatabase[questData.Id] = questData;
            
            if (!_questStates.ContainsKey(questData.Id))
            {
                _questStates[questData.Id] = questData.RequiredQuestIds.Count == 0 
                    ? QuestState.Available 
                    : QuestState.Locked;
            }
            
            _logger.Debug($"Registered quest: {questData.Id}");
        }
        
        public void StartQuest(string questId)
        {
            ThrowIfDisposed();
            
            if (!_questDatabase.TryGetValue(questId, out QuestData questData))
            {
                _logger.Error($"Quest not found: {questId}");
                return;
            }
            
            if (!IsQuestAvailable(questId))
            {
                _logger.Warning($"Quest not available: {questId}");
                return;
            }
            
            QuestData activeData = CloneQuestData(questData);
            _activeQuestData[questId] = activeData;
            _questStates[questId] = QuestState.Active;
            
            _logger.Info($"Started quest: {questData.Title}");
            _onQuestEvent.OnNext(new QuestEventData(QuestEventType.Started, questId));
        }
        
        public void CompleteObjective(string questId, string objectiveId, int count = 1)
        {
            ThrowIfDisposed();
            
            if (!_activeQuestData.TryGetValue(questId, out QuestData questData))
            {
                _logger.Warning($"Quest not active: {questId}");
                return;
            }
            
            QuestObjective objective = questData.Objectives.FirstOrDefault(o => o.Id == objectiveId);
            if (objective == null)
            {
                _logger.Warning($"Objective not found: {objectiveId}");
                return;
            }
            
            if (objective.IsCompleted) return;
            
            objective.CurrentCount += count;
            
            _logger.Info($"Objective updated: {objective.Description} ({objective.CurrentCount}/{objective.RequiredCount})");
            _onQuestEvent.OnNext(new QuestEventData(QuestEventType.ObjectiveUpdated, questId, objectiveId));
            
            if (objective.IsCompleted)
            {
                _logger.Info($"Objective completed: {objective.Description}");
                _onQuestEvent.OnNext(new QuestEventData(QuestEventType.ObjectiveCompleted, questId, objectiveId));
                
                CheckAutoOutcome(questId, questData);
            }
        }
        
        public void ChooseOutcome(string questId, string outcomeId)
        {
            ThrowIfDisposed();
            
            if (!_activeQuestData.TryGetValue(questId, out QuestData questData))
            {
                _logger.Warning($"Quest not active: {questId}");
                return;
            }
            
            QuestOutcome outcome = questData.Outcomes.FirstOrDefault(o => o.Id == outcomeId);
            if (outcome == null)
            {
                _logger.Warning($"Outcome not found: {outcomeId}");
                return;
            }
            
            if (!IsOutcomeAvailable(questData, outcome))
            {
                _logger.Warning($"Outcome not available: {outcomeId}");
                return;
            }
            
            _logger.Info($"Outcome chosen: {outcome.Description}");
            _onQuestEvent.OnNext(new QuestEventData(QuestEventType.OutcomeChosen, questId, outcomeId: outcomeId));
            
            GrantRewards(questId, outcome.Rewards);
            CompleteQuest(questId, outcome);
        }
        
        public void FailQuest(string questId)
        {
            ThrowIfDisposed();
            
            if (!_activeQuestData.ContainsKey(questId))
            {
                _logger.Warning($"Quest not active: {questId}");
                return;
            }
            
            _questStates[questId] = QuestState.Failed;
            _activeQuestData.Remove(questId);
            
            _logger.Info($"Quest failed: {questId}");
            _onQuestEvent.OnNext(new QuestEventData(QuestEventType.Failed, questId));
        }
        
        public QuestState GetQuestState(string questId)
        {
            return _questStates.TryGetValue(questId, out QuestState state) ? state : QuestState.Locked;
        }
        
        public QuestData GetQuestData(string questId)
        {
            if (_activeQuestData.TryGetValue(questId, out QuestData activeData))
            {
                return activeData;
            }
            
            return _questDatabase.TryGetValue(questId, out QuestData data) ? data : null;
        }
        
        public List<QuestData> GetActiveQuests()
        {
            return _activeQuestData.Values.ToList();
        }
        
        public List<QuestData> GetAvailableQuests()
        {
            return _questDatabase.Values
                .Where(q => IsQuestAvailable(q.Id))
                .ToList();
        }
        
        public bool IsQuestAvailable(string questId)
        {
            if (!_questStates.TryGetValue(questId, out QuestState state))
            {
                return false;
            }
            
            if (state != QuestState.Available && state != QuestState.Locked)
            {
                return false;
            }
            
            if (!_questDatabase.TryGetValue(questId, out QuestData questData))
            {
                return false;
            }
            
            foreach (string requiredId in questData.RequiredQuestIds)
            {
                if (!IsQuestCompleted(requiredId))
                {
                    return false;
                }
            }
            
            return true;
        }
        
        public bool IsQuestCompleted(string questId)
        {
            return _questStates.TryGetValue(questId, out QuestState state) && state == QuestState.Completed;
        }
        
        private void CompleteQuest(string questId, QuestOutcome outcome)
        {
            _questStates[questId] = QuestState.Completed;
            _activeQuestData.Remove(questId);
            
            _logger.Info($"Quest completed: {questId}");
            _onQuestEvent.OnNext(new QuestEventData(QuestEventType.Completed, questId, outcomeId: outcome.Id));
            
            if (!string.IsNullOrEmpty(outcome.NextQuestId))
            {
                UnlockQuest(outcome.NextQuestId);
            }
            
            UpdateLockedQuests();
        }
        
        private void UnlockQuest(string questId)
        {
            if (_questStates.TryGetValue(questId, out QuestState state) && state == QuestState.Locked)
            {
                if (IsQuestAvailable(questId))
                {
                    _questStates[questId] = QuestState.Available;
                    _logger.Info($"Quest unlocked: {questId}");
                }
            }
        }
        
        private void UpdateLockedQuests()
        {
            foreach (string questId in _questStates.Keys.ToList())
            {
                if (_questStates[questId] == QuestState.Locked && IsQuestAvailable(questId))
                {
                    _questStates[questId] = QuestState.Available;
                    _logger.Debug($"Quest now available: {questId}");
                }
            }
        }
        
        private void CheckAutoOutcome(string questId, QuestData questData)
        {
            List<QuestOutcome> availableOutcomes = questData.Outcomes
                .Where(o => IsOutcomeAvailable(questData, o))
                .ToList();
            
            if (availableOutcomes.Count == 1)
            {
                ChooseOutcome(questId, availableOutcomes[0].Id);
            }
        }
        
        private bool IsOutcomeAvailable(QuestData questData, QuestOutcome outcome)
        {
            if (outcome.RequiredObjectiveIds.Count == 0)
            {
                return questData.Objectives.All(o => o.IsCompleted);
            }
            
            foreach (string objectiveId in outcome.RequiredObjectiveIds)
            {
                QuestObjective objective = questData.Objectives.FirstOrDefault(o => o.Id == objectiveId);
                if (objective == null || !objective.IsCompleted)
                {
                    return false;
                }
            }
            
            return true;
        }
        
        private void GrantRewards(string questId, List<QuestRewardData> rewards)
        {
            foreach (QuestRewardData reward in rewards)
            {
                _logger.Info($"Reward: {reward.RewardType} - {reward.RewardId} x{reward.Amount}");
                _onQuestEvent.OnNext(new QuestEventData(QuestEventType.RewardGranted, questId, reward: reward));
            }
        }
        
        private QuestData CloneQuestData(QuestData source)
        {
            return new QuestData
            {
                Id = source.Id,
                Title = source.Title,
                Description = source.Description,
                RequiredQuestIds = new List<string>(source.RequiredQuestIds),
                Objectives = source.Objectives.Select(o => new QuestObjective
                {
                    Id = o.Id,
                    Description = o.Description,
                    RequiredCount = o.RequiredCount,
                    CurrentCount = 0
                }).ToList(),
                Outcomes = source.Outcomes.Select(o => new QuestOutcome
                {
                    Id = o.Id,
                    Description = o.Description,
                    NextQuestId = o.NextQuestId,
                    RequiredObjectiveIds = new List<string>(o.RequiredObjectiveIds),
                    Rewards = new List<QuestRewardData>(o.Rewards)
                }).ToList()
            };
        }
        
        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(QuestService));
            }
        }
        
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            
            _onQuestEvent.Dispose();
            _questDatabase.Clear();
            _questStates.Clear();
            _activeQuestData.Clear();
            
            _logger.Info("Disposed");
        }
    }
}


