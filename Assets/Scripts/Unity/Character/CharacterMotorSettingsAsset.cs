using Project.Core.Character;
using UnityEngine;

namespace Project.Unity.Character
{
    [CreateAssetMenu(fileName = "CharacterMovementSettings", menuName = "Game/Character Movement Settings")]
    public class CharacterMotorSettingsAsset : ScriptableObject
    {
        [SerializeField] private CharacterMovementSettings _settings = new();
        
        public CharacterMovementSettings Settings => _settings;
    }
}


