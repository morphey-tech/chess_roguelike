using System;
using Project.Core.Player;

namespace Project.Core.Save
{
    [Serializable]
    public class SaveData
    {
        public string SlotId;
        public long SaveTimeTicks;
        public PlayerSaveData Player;
        public string SceneName;
        
        public DateTime SaveTime
        {
            get => new DateTime(SaveTimeTicks);
            set => SaveTimeTicks = value.Ticks;
        }
    }
}


