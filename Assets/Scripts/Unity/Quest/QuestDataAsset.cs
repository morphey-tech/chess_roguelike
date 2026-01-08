using System.Collections.Generic;
using Project.Core.Quest;
using UnityEngine;

namespace Project.Unity.Quest
{
    [CreateAssetMenu(fileName = "Quest", menuName = "Game/Quest")]
    public class QuestDataAsset : ScriptableObject
    {
        [Header("Основное")]
        public string Id;
        public string Title;
        [TextArea(3, 5)]
        public string Description;
        
        [Header("Требования")]
        public List<QuestDataAsset> RequiredQuests = new();
        
        [Header("Цели")]
        public List<QuestObjectiveData> Objectives = new();
        
        [Header("Исходы")]
        public List<QuestOutcomeData> Outcomes = new();
        
        public QuestData ToQuestData()
        {
            QuestData data = new QuestData
            {
                Id = Id,
                Title = Title,
                Description = Description,
                RequiredQuestIds = new List<string>(),
                Objectives = new List<QuestObjective>(),
                Outcomes = new List<QuestOutcome>()
            };
            
            foreach (QuestDataAsset required in RequiredQuests)
            {
                if (required != null)
                {
                    data.RequiredQuestIds.Add(required.Id);
                }
            }
            
            foreach (QuestObjectiveData obj in Objectives)
            {
                data.Objectives.Add(new QuestObjective
                {
                    Id = obj.Id,
                    Description = obj.Description,
                    RequiredCount = obj.RequiredCount
                });
            }
            
            foreach (QuestOutcomeData outcome in Outcomes)
            {
                QuestOutcome outcomeData = new QuestOutcome
                {
                    Id = outcome.Id,
                    Description = outcome.Description,
                    NextQuestId = outcome.NextQuest != null ? outcome.NextQuest.Id : null,
                    RequiredObjectiveIds = new List<string>(outcome.RequiredObjectiveIds),
                    Rewards = new List<QuestRewardData>()
                };
                
                foreach (QuestRewardInfo reward in outcome.Rewards)
                {
                    outcomeData.Rewards.Add(new QuestRewardData
                    {
                        RewardType = reward.Type,
                        RewardId = reward.Id,
                        Amount = reward.Amount
                    });
                }
                
                data.Outcomes.Add(outcomeData);
            }
            
            return data;
        }
        
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(Id))
            {
                Id = name;
            }
        }
    }
    
    [System.Serializable]
    public class QuestObjectiveData
    {
        public string Id;
        public string Description;
        public int RequiredCount = 1;
    }
    
    [System.Serializable]
    public class QuestOutcomeData
    {
        public string Id;
        [TextArea(2, 3)]
        public string Description;
        public QuestDataAsset NextQuest;
        public List<string> RequiredObjectiveIds = new();
        public List<QuestRewardInfo> Rewards = new();
    }
    
    [System.Serializable]
    public class QuestRewardInfo
    {
        public string Type;
        public string Id;
        public int Amount = 1;
    }
}


