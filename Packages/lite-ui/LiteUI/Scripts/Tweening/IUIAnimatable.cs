using Cysharp.Threading.Tasks;

namespace LiteUI.Tweening
{
    /// <summary>
    /// Интерфейс для UI элементов с анимациями.
    /// Реализуй для интеграции с DialogManager, PopupManager и т.д.
    /// </summary>
    public interface IUIAnimatable
    {
        /// <summary>Анимация показа</summary>
        UniTask AnimateShow();
        
        /// <summary>Анимация скрытия</summary>
        UniTask AnimateHide();
    }
}

