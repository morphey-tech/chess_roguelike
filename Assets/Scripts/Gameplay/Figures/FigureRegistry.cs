using System.Collections.Generic;
using System.Linq;

namespace Project.Gameplay.Gameplay.Figures
{
    public sealed class FigureRegistry : IFigureRegistry
    {
        private readonly List<Figure> _figures = new();

        public void Register(Figure figure)
        {
            _figures.Add(figure);
        }

        public void Unregister(Figure figure)
        {
            _figures.Remove(figure);
        }

        public IEnumerable<Figure> GetTeam(Team team)
        {
            return _figures.Where(f => f.Team == team && !f.Stats.IsDead);
        }

        public IEnumerable<Figure> GetAll()
        {
            return _figures;
        }
    }
}