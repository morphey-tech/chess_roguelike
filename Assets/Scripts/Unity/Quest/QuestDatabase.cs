using System.Collections.Generic;
using UnityEngine;

namespace Project.Unity.Quest
{
    [CreateAssetMenu(fileName = "QuestDatabase", menuName = "Game/Quest Database")]
    public class QuestDatabase : ScriptableObject
    {
        public List<QuestDataAsset> Quests = new();
    }
}


