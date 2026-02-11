using Cysharp.Threading.Tasks;
using Project.Gameplay.Gameplay.Combat.Effects;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Combat
{
    /// <summary>
    /// Единая точка обработки смерти юнита: визуал, лут, снятие с доски, сообщение.
    /// </summary>
    public interface IFigureLifeService
    {
        /// <summary>Смерть в контексте боя — добавляет визуал-события в контекст, снимает с доски.</summary>
        void HandleDeathFromCombat(CombatEffectContext context, Figure unit, BoardCell cell);

        /// <summary>Только домен: снять с доски, Publish. Визуал/лут — отдельными командами в очереди.</summary>
        void HandleDeathDomainOnly(Figure unit, BoardCell cell);

        /// <summary>Смерть вне цепочки (если команды не могут дописать в очередь) — сразу визуал и лут.</summary>
        UniTask HandleDeathDirectAsync(Figure unit, BoardCell cell);
    }
}
