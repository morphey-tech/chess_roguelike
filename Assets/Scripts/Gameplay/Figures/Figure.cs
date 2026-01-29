using System.Collections.Generic;
using Project.Gameplay.Gameplay.Combat;
using Project.Gameplay.Gameplay.Turn;

namespace Project.Gameplay.Gameplay.Figures
{
    public class Figure : Entity
    {
        public string TypeId { get; }
        public string MovementId { get; }
        public string AttackId { get; }
        public string TurnPatternSetId { get; }
        public FigureStats Stats { get; }
        public Team Team { get; }
        public List<IPassive> Passives { get; } = new();
        public TurnPatternSet TurnPatternSet { get; private set; }

        public Figure(int id, string typeId, string movementId, string attackId, string turnPatternSetId, FigureStats stats, Team team) : base(id)
        {
            TypeId = typeId;
            MovementId = movementId;
            AttackId = attackId;
            TurnPatternSetId = turnPatternSetId;
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
