using System;
using Project.Gameplay.Gameplay.Save.Models;

namespace Project.Gameplay.Gameplay.Save
{
    [Serializable]
    public sealed class SaveSnapshot
    {
        public string SlotId { get; set; }
        public DateTime SaveTime { get; set; }
        public string SceneId { get; set; }
        public int Version { get; set; }
        
        public PlayerLoadoutModel? Loadout { get; set; }
        public PlayerRunStateModel? Run { get; set; }
        public PlayerMetaProgressModel? MetaProgress { get; set; }
    }
}