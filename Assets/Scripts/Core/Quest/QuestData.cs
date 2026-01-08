using System;
using System.Collections.Generic;

namespace Project.Core.Quest
{
    public enum QuestState
    {
        Locked,
        Available,
        Active,
        Completed,
        Failed
    }
    
    [Serializable]
    public class QuestData
    {
        public string Id;
        public string Title;
        public string Description;
        public List<QuestObjective> Objectives = new();
        public List<QuestOutcome> Outcomes = new();
        public List<string> RequiredQuestIds = new();
    }
    
    [Serializable]
    public class QuestObjective
    {
        public string Id;
        public string Description;
        public int RequiredCount = 1;
        public int CurrentCount;
        public bool IsCompleted => CurrentCount >= RequiredCount;
    }
    
    [Serializable]
    public class QuestOutcome
    {
        public string Id;
        public string Description;
        public string NextQuestId;
        public List<QuestRewardData> Rewards = new();
        public List<string> RequiredObjectiveIds = new();
    }
    
    [Serializable]
    public class QuestRewardData
    {
        public string RewardType;
        public string RewardId;
        public int Amount;
    }
}


