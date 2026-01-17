using System;

namespace Project.Gameplay.Gameplay.Save.Models
{
    [Serializable]
    public sealed class PlayerLoadoutModel
    {
        public string KingId { get; set; }
        public string SuiteId { get; set; }

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(KingId) && !string.IsNullOrEmpty(SuiteId);
        }
    }
}