using Project.Core.Character;
using Project.Unity.Character;
using UnityEngine;

namespace Project.Unity.Ladder
{
    [RequireComponent(typeof(Collider))]
    public class LadderTrigger : MonoBehaviour
    {
        [SerializeField] private LadderComponent _ladder;
        [SerializeField] private bool _autoGrab = false;
        
        private void Reset()
        {
            _ladder = GetComponentInParent<LadderComponent>();
            
            Collider col = GetComponent<Collider>();
            col.isTrigger = true;
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (!_autoGrab) return;
            if (_ladder == null) return;
            
            CharacterMotor motor = other.GetComponent<CharacterMotor>();
            if (motor != null && motor.State != CharacterState.OnLadder)
            {
                motor.EnterLadder(_ladder);
            }
        }
    }
}


