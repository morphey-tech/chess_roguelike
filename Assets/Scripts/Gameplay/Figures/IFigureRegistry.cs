using System.Collections.Generic;
using Project.Core.Core.Combat;

namespace Project.Gameplay.Gameplay.Figures
{
    public interface IFigureRegistry
    {
        void Register(Figure figure);
        void Unregister(Figure figure);
        IEnumerable<Figure> GetTeam(Team team);
        IEnumerable<Figure> GetAll();
    }
}