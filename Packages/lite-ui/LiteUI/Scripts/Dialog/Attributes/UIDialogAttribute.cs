using System;

namespace LiteUI.Dialog.Attributes
{
    /// <summary>
    /// Атрибут для маркировки класса как диалога.
    /// Анимации настраиваются через UITweenAnimator или наследование от AnimatedDialogBase.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class UIDialogAttribute : Attribute
    {
        public UIDialogAttribute()
        {
        }
    }
}
