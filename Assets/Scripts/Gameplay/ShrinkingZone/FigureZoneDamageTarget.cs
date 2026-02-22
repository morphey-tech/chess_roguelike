using Project.Core.Core.ShrinkingZone.Core;
using Project.Gameplay.Gameplay.Figures;

namespace Project.Gameplay.ShrinkingZone
{
    /// <summary>
    /// Адаптер Figure для работы с ZoneShrinkSystem
    /// </summary>
    public class FigureZoneDamageTarget : IZoneDamageTarget
    {
        private readonly Figure _figure;

        public int MaxHP => _figure.Stats.MaxHp;

        public FigureZoneDamageTarget(Figure figure)
        {
            _figure = figure;
        }

        public void TakeDamage(int damage)
        {
            _figure.Stats.TakeDamage(damage);
        }
    }
}
