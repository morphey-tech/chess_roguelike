using System.Collections.Generic;
using Project.Gameplay.Gameplay.Combat;
using Project.Gameplay.Gameplay.Turn;

namespace Project.Gameplay.Gameplay.Figures
{
    public class Figure
    {
        public int Id { get; } 
        public string TypeId { get; }
        public string MovementId { get; }
        public string AttackId { get; }
        public string TurnPatternsId { get; }
        public FigureStats Stats { get; }
        public Team Team { get; }
        public List<IPassive> Passives { get; } = new();
        public TurnPatternSet TurnPatternSet { get; private set; }

        public Figure(int id, string typeId, string movementId, string attackId, string turnPatternsId, FigureStats stats, Team team)
        {
            Id = id;
            TypeId = typeId;
            MovementId = movementId;
            AttackId = attackId;
            TurnPatternsId = turnPatternsId;
            Stats = stats;
            Team = team;
        }

        public void SetTurnPatternSet(TurnPatternSet patternSet)
        {
            TurnPatternSet = patternSet;
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
