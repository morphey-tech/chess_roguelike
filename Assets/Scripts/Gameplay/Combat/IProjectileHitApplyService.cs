using Cysharp.Threading.Tasks;
using Project.Gameplay.Gameplay.Combat.Visual;
using Project.Gameplay.Gameplay.Visual;

namespace Project.Gameplay.Gameplay.Combat
{
    /// <summary>
    /// Применяет урон снаряда в момент команды (после Impact). Смерть/лут — только добавление команд в очередь, без вложенного визуала.
    /// </summary>
    public interface IProjectileHitApplyService
    {
        UniTask ApplyAsync(ProjectileHitApplyEvent evt, IPresenterProvider presenters);
    }
}
