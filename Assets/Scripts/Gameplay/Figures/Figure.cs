using System.Collections.Generic;
using Project.Core.Core.Grid;
using Project.Core.Core.Triggers;
using Project.Gameplay.Gameplay.Combat;
using Project.Gameplay.Gameplay.Figures.StatusEffects;
using Project.Gameplay.Gameplay.Turn;

namespace Project.Gameplay.Gameplay.Figures
{
    public class Figure : Entity, ITriggerEntity
    {
        public string TypeId { get; }
        public string MovementId { get; }
        public string AttackId { get; }
        public string TurnPatternsId { get; }
        public string? InfoId { get; }  // Ссылка на FigureInfoConfig
        public Team Team { get; }

        public FigureStats Stats { get; }
        public TurnPattern? TurnPattern { get; private set; }
        public List<IPassive> BasePassives { get; } = new();
        public StatusEffectSystem Effects { get; }
        public bool MovedThisTurn { get; set; }
        public GridPosition? PreviousPosition { get; set; }
        public string? LootTableId { get; set; }

        // ITriggerEntity implementation
        string ITriggerEntity.Id => TriggerId;
        public string TriggerId => base.Id.ToString();

        /// <summary>
        /// Numeric entity ID for visual and grid systems.
        /// </summary>
        public int EntityId => base.Id;

        private readonly TriggerService? _triggerService;

        public Figure(int id, string typeId, string movementId, string attackId,
            string turnPatternsId, FigureStats stats, Team team, string? infoId = null,
            TriggerService? triggerService = null) : base(id)
        {
            TypeId = typeId;
            MovementId = movementId;
            AttackId = attackId;
            TurnPatternsId = turnPatternsId;
            InfoId = infoId;
            Stats = stats;
            Team = team;
            _triggerService = triggerService;
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
            _triggerService?.Register(passive);
        }

        public void RemovePassive(IPassive passive)
        {
            if (BasePassives.Remove(passive))
            {
                _triggerService?.Unregister(passive);
            }
        }
    }

    public static class FigureExtensions
    {
        public static string ToString(this Figure figure) => $"{figure.TypeId}#{figure.Id}";
    }
}
