using System.Collections.Generic;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Combat;
using Project.Gameplay.Gameplay.Figures.StatusEffects;
using Project.Gameplay.Gameplay.Turn;

namespace Project.Gameplay.Gameplay.Figures
{
    public class Figure : Entity
    {
        public string TypeId { get; }
        public string MovementId { get; }
        public string AttackId { get; }
        public string TurnPatternsId { get; }
        public Team Team { get; }

        public FigureStats Stats { get; }
        public TurnPattern? TurnPattern { get; private set; }
        public List<IPassive> BasePassives { get; } = new();
        public StatusEffectSystem Effects { get; }
        public bool MovedThisTurn { get; set; }
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
            Effects = new(this);
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
            BasePassives.Add(passive);
        }

        public override string ToString() => $"{TypeId}#{Id}";
    }
}
