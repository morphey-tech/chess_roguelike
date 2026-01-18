using UnityEngine;

namespace Project.Gameplay.Gameplay.Input.Messages
{
    /// <summary>
    /// Raw click event with ray info. Services decide how to handle it.
    /// </summary>
    public readonly struct RawClickMessage
    {
        public Ray Ray { get; }
        public Vector2 ScreenPosition { get; }

        public RawClickMessage(Ray ray, Vector2 screenPosition)
        {
            Ray = ray;
            ScreenPosition = screenPosition;
        }
    }
}
