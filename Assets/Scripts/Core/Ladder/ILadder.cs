using UnityEngine;

namespace Project.Core.Ladder
{
    public interface ILadder
    {
        Vector3 BottomPoint { get; }
        Vector3 TopPoint { get; }
        Vector3 Forward { get; }
        Vector3 GetPositionOnLadder(float t);
        Vector3 GetClosestPoint(Vector3 position, out float t);
        bool CanExitTop(out Vector3 exitPosition);
        bool CanExitBottom(out Vector3 exitPosition);
    }
}

