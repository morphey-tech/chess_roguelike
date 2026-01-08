using UnityEngine;

namespace Project.Core.Interaction
{
    public readonly struct InteractionCandidate
    {
        public IInteractable Interactable { get; }
        public float Distance { get; }
        public float Angle { get; }
        public Vector3 Position { get; }
        public float Score { get; }
        
        public InteractionCandidate(IInteractable interactable, float distance, float angle, Vector3 position, float score)
        {
            Interactable = interactable;
            Distance = distance;
            Angle = angle;
            Position = position;
            Score = score;
        }
    }
}


