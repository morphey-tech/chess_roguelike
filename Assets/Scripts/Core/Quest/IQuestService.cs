using System;
using System.Collections.Generic;

namespace Project.Core.Quest
{
    public interface IQuestService
    {
        IObservable<QuestEventData> OnQuestEvent { get; }
        
        void StartQuest(string questId);
        void CompleteObjective(string questId, string objectiveId, int count = 1);
        void ChooseOutcome(string questId, string outcomeId);
        void FailQuest(string questId);
        
        QuestState GetQuestState(string questId);
        QuestData GetQuestData(string questId);
        List<QuestData> GetActiveQuests();
        List<QuestData> GetAvailableQuests();
        
        bool IsQuestAvailable(string questId);
        bool IsQuestCompleted(string questId);
    }
    
    public enum QuestEventType
    {
        Started,
        ObjectiveUpdated,
        ObjectiveCompleted,
        OutcomeChosen,
        Completed,
        Failed,
        RewardGranted
    }
    
    public readonly struct QuestEventData
    {
        public QuestEventType Type { get; }
        public string QuestId { get; }
        public string ObjectiveId { get; }
        public string OutcomeId { get; }
        public QuestRewardData Reward { get; }
        
        public QuestEventData(QuestEventType type, string questId, string objectiveId = null, string outcomeId = null, QuestRewardData reward = null)
        {
            Type = type;
            QuestId = questId;
            ObjectiveId = objectiveId;
            OutcomeId = outcomeId;
            Reward = reward;
        }
    }
}


