using System.Collections.Generic;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Combat;
using Project.Gameplay.Gameplay.Turn;

namespace Project.Gameplay.Gameplay.Figures
{
    public class Figure : Entity
    {
        public string TypeId { get; }
        public string MovementId { get; }
        public string AttackId { get; }
        public string TurnPatternsId { get; }
        public FigureStats Stats { get; }
        public Team Team { get; }
        public List<IPassive> Passives { get; } = new();
        public TurnPattern? TurnPattern { get; private set; }

        /// <summary>True if this figure moved this turn (reset at start of each team's turn). Used e.g. by Fortify.</summary>
        public bool MovedThisTurn { get; set; }
        
        /// <summary>Previous position before movement. Used by passives to calculate movement distance.</summary>
        public GridPosition? PreviousPosition { get; set; }
        
        public string? LootTableId { get; set; }

        public Figure(int id, string typeId, string movementId, string attackId,
            string turnPatternsId, FigureStats stats, Team team) : base(id)
        {
            TypeId = typeId;
            MovementId = movementId;
            AttackId = attackId;
            TurnPatternsId = turnPatternsId;
            Stats = stats;
            Team = team;
        }

        public void SetTurnPatternSet(TurnPattern pattern)
        {
            TurnPattern = pattern;
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
