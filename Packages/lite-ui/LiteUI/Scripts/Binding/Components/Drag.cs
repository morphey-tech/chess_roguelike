using UnityEngine;

namespace LiteUI.Binding.Components
{
    public class Drag
    {
        public Vector2 StartPosition { get; }
        public Vector2 Position { get; }
        public Vector2 Delta { get; }
        public GameObject Target { get; }

        public Drag(Vector2 startPosition, Vector2 position, Vector2 delta, GameObject target)
        {
            StartPosition = startPosition;
            Position = position;
            Delta = delta;
            Target = target;
        }
    }
}
