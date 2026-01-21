namespace Project.Gameplay.Gameplay.Figures
{
    public class Figure : Entity
    {
        public string TypeId { get; }
        public string MovementId { get; }
        public FigureStats Stats { get; }
        public Team Team { get; }

        public Figure(int id, string typeId, string movementId, FigureStats stats, Team team) : base(id)
        {
            TypeId = typeId;
            MovementId = movementId;
            Stats = stats;
            Team = team;
        }

        public override string ToString()
        {
            return $"{TypeId}#{Id}";
        }
    }
}
