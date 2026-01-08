using JetBrains.Annotations;
using UnityEngine;

namespace LiteUI.UI.Component
{
    [PublicAPI]
    public class UIIdComponent : MonoBehaviour
    {
        [field: SerializeField]
        public string Alias { get; private set; } = null!;
    }
}
