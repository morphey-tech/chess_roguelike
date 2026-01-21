namespace Project.Gameplay.Gameplay.Figures
{
    public class Figure
    {
        public int Id { get; } 
        public string TypeId { get; }
        public string MovementId { get; }
        public FigureStats Stats { get; }
        public Team Team { get; }

        public Figure(int id, string typeId, string movementId, FigureStats stats, Team team)
        {
            Id = id;
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
