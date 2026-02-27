using UnityEngine;

namespace Project.Gameplay.Gameplay.Input.Messages
{
    /// <summary>
    /// Запрос на показ tooltip.
    /// </summary>
    public readonly struct TooltipShowRequestMessage
    {
        public string Content { get; }
        public Vector2 Position { get; }

        public TooltipShowRequestMessage(string content, Vector2 position)
        {
            Content = content;
            Position = position;
        }
    }
}
