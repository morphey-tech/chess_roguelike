namespace Project.Gameplay.Gameplay.Figures
{
    public class Figure
    {
        private static int _nextId;

        public int Id { get; } = ++_nextId;
        public string TypeId { get; }
        public string MovementId { get; }
        public FigureStats Stats { get; }
        public Team Team { get; }

        public Figure(string typeId, string movementId, FigureStats stats, Team team)
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
