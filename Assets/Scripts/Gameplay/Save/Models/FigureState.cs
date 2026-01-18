using System;

namespace Project.Gameplay.Gameplay.Save.Models
{
    [Serializable]
    public sealed class FigureState
    {
        public string Id { get; set; }
        public string TypeId { get; set; }
        public FigureLocation Location { get; set; }

        public FigureState() { }

        public FigureState(string id, string typeId)
        {
            Id = id;
            TypeId = typeId;
            Location = FigureLocation.InHand();
        }
    }
}
