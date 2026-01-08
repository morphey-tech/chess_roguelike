using UnityEngine;

namespace LiteUI.UI.Balloon.Model
{
    public class BalloonViewModel : MonoBehaviour
    {
        [field: SerializeField]
        public Vector2 Offset { get; private set; }
    }
}
