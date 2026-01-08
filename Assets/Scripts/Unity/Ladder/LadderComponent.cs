using Project.Core.Ladder;
using UnityEngine;

namespace Project.Unity.Ladder
{
    public class LadderComponent : MonoBehaviour, ILadder
    {
        [Header("Точки лестницы")]
        [SerializeField] private Transform _bottomPoint;
        [SerializeField] private Transform _topPoint;
        
        [Header("Точки выхода")]
        [SerializeField] private Transform _topExitPoint;
        [SerializeField] private Transform _bottomExitPoint;
        
        [Header("Настройки")]
        [SerializeField] private float _exitCheckRadius = 0.4f;
        [SerializeField] private LayerMask _obstacleLayers = ~0;
        
        public Vector3 BottomPoint => _bottomPoint != null ? _bottomPoint.position : transform.position;
        public Vector3 TopPoint => _topPoint != null ? _topPoint.position : transform.position + Vector3.up * 3f;
        public Vector3 Forward => transform.forward;
        
        public Vector3 GetPositionOnLadder(float t)
        {
            return Vector3.Lerp(BottomPoint, TopPoint, t);
        }
        
        public Vector3 GetClosestPoint(Vector3 position, out float t)
        {
            Vector3 ladderDir = TopPoint - BottomPoint;
            Vector3 toPosition = position - BottomPoint;
            
            t = Mathf.Clamp01(Vector3.Dot(toPosition, ladderDir.normalized) / ladderDir.magnitude);
            return GetPositionOnLadder(t);
        }
        
        public bool CanExitTop(out Vector3 exitPosition)
        {
            exitPosition = _topExitPoint != null 
                ? _topExitPoint.position 
                : TopPoint + Forward * 0.5f;
            
            return !Physics.CheckSphere(
                exitPosition + Vector3.up * 0.5f,
                _exitCheckRadius,
                _obstacleLayers,
                QueryTriggerInteraction.Ignore
            );
        }
        
        public bool CanExitBottom(out Vector3 exitPosition)
        {
            exitPosition = _bottomExitPoint != null 
                ? _bottomExitPoint.position 
                : BottomPoint - Forward * 0.5f;
            
            return !Physics.CheckSphere(
                exitPosition + Vector3.up * 0.5f,
                _exitCheckRadius,
                _obstacleLayers,
                QueryTriggerInteraction.Ignore
            );
        }
        
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(BottomPoint, TopPoint);
            
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(BottomPoint, 0.2f);
            Gizmos.DrawWireSphere(TopPoint, 0.2f);
            
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(GetPositionOnLadder(0.5f), Forward);
        }
    }
}


