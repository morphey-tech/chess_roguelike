using UnityEngine;

namespace Project.Gameplay.Gameplay.Input.Messages
{
    /// <summary>
    /// Запрос на показ/скрытие tooltip.
    /// </summary>
    public readonly struct TooltipMessage
    {
        public const string SHOW = "tooltipShow";
        public const string HIDE = "tooltipHide";

        public readonly string Content;
        public readonly Vector2 Position;
        public readonly bool UseStaticPosition;

        private TooltipMessage(string content, Vector2 position, bool useStaticPosition)
        {
            Content = content;
            Position = position;
            UseStaticPosition = useStaticPosition;
        }

        public static TooltipMessage Show(string content, Vector2 position, bool useStaticPosition = false)
        {
            return new TooltipMessage(content, position, useStaticPosition);
        }

        public static TooltipMessage Hide()
        {
            return new TooltipMessage(string.Empty, Vector2.zero, false);
        }
    }
}
