using Project.Core.Character;
using Project.Core.Config;
using UnityEngine;

namespace Project.Unity.Config
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Game/Game Config")]
    public class GameConfigAsset : ScriptableObject
    {
        [Header("Персонаж")]
        public CharacterMovementSettings CharacterMovement = new();

        [Header("Взаимодействие")]
        public InteractionConfig Interaction = new();

        [Header("Камера")]
        public CameraConfig Camera = new();

        [Header("UI")]
        public UIConfig UI = new();
    }
}
