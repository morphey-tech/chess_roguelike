using System.Collections.Generic;
using Project.Gameplay.Gameplay.Combat;

using System.Collections.Generic;
using Project.Gameplay.Gameplay.Combat;

namespace Project.Gameplay.Gameplay.Figures
{
    public class Figure
    {
        public int Id { get; } 
        public string TypeId { get; }
        public string MovementId { get; }
        public string AttackId { get; }
        public FigureStats Stats { get; }
        public Team Team { get; }
        public List<IPassive> Passives { get; } = new();

        public Figure(int id, string typeId, string movementId, string attackId, FigureStats stats, Team team)
        {
            Id = id;
            TypeId = typeId;
            MovementId = movementId;
            AttackId = attackId;
            Stats = stats;
            Team = team;
        }

        public void AddPassive(IPassive? passive)
        {
            if (passive == null)
            {
                return;
            }
            Passives.Add(passive);
        }

        public override string ToString() => $"{TypeId}#{Id}";
    }
}
