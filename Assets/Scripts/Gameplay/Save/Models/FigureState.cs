using System;
using System.Collections.Generic;

namespace Project.Gameplay.Gameplay.Save.Models
{
    [Serializable]
    public sealed class FigureState
    {
        public string Id { get; set; }
        public string TypeId { get; set; }
        public FigureLocation Location { get; set; }

        // Сохраняемые статы
        public int CurrentHp { get; set; }
        public int MaxHp { get; set; }
        public float Attack { get; set; }
        public float Defence { get; set; }
        public float Evasion { get; set; }

        // Пассивки (ID)
        public List<string> PassiveIds { get; set; } = new List<string>();

        // Статус-эффекты
        public List<EffectState> Effects { get; set; } = new List<EffectState>();

        public FigureState() { }

        public FigureState(string id, string typeId)
        {
            Id = id;
            TypeId = typeId;
            Location = FigureLocation.InHand();
        }
    }
}
