using System;

namespace Project.Core.Core.Save
{
    [Serializable]
    public class SaveData
    {
        public string SlotId;
        public long SaveTimeTicks;
        public string SceneName;
        
        public DateTime SaveTime
        {
            get => new DateTime(SaveTimeTicks);
            set => SaveTimeTicks = value.Ticks;
        }
    }
}


