using System;

namespace LiteUI.Popup.Model
{
    public enum PopupAlign
    {
        TOP,
        BOTTOM,
        LEFT,
        RIGHT
    }

    public static class PopupAlignExtension
    {
        public static PopupAlign InversedAlign(this PopupAlign align)
        {
            switch (align) {
                case PopupAlign.TOP:
                    return PopupAlign.BOTTOM;
                case PopupAlign.BOTTOM:
                    return PopupAlign.TOP;
                case PopupAlign.LEFT:
                    return PopupAlign.RIGHT;
                case PopupAlign.RIGHT:
                    return PopupAlign.LEFT;
                default:
                    throw new InvalidOperationException("Popup align error");
            }
        }
    }
}
