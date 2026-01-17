using System;

namespace Project.Gameplay.Gameplay.Save.Models
{
    [Serializable]
    public sealed class PlayerRunStateModel
    {
        public string StageId { get; set; }
        public int KingHp { get; set; }
        public int Seed { get; set; }
    }
}