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

        private TooltipMessage(string content, Vector2 position)
        {
            Content = content;
            Position = position;
        }

        public static TooltipMessage Show(string content, Vector2 position)
        {
            return new TooltipMessage(content, position);
        }

        public static TooltipMessage Hide()
        {
            return new TooltipMessage(string.Empty, Vector2.zero);
        }
    }
}
