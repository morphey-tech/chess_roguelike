using System.Collections.Generic;
using Project.Gameplay.Gameplay.Combat;

namespace Project.Gameplay.Gameplay.Figures
{
    public class Figure
    {
        private static int _nextId;

        public int Id { get; } = ++_nextId;
        public string TypeId { get; }
        public string MovementId { get; }
        public string AttackId { get; }
        public FigureStats Stats { get; }
        public Team Team { get; }
        public List<IPassive> Passives { get; } = new();

        public Figure(string typeId, string movementId, string attackId, FigureStats stats, Team team)
        {
            TypeId = typeId;
            MovementId = movementId;
            AttackId = attackId;
            Stats = stats;
            Team = team;
        }

        public void AddPassive(IPassive passive)
        {
            if (passive != null)
                Passives.Add(passive);
        }

        public override string ToString() => $"{TypeId}#{Id}";
    }
}
