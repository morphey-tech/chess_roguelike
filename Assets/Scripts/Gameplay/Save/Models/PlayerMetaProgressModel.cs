using System;

namespace Project.Gameplay.Gameplay.Save.Models
{
    [Serializable]
    public sealed class PlayerMetaProgressModel
    {
        public string[] UnlockedKings { get; set; }
        public string[] UnlockedSuits { get; set; }
    }
}