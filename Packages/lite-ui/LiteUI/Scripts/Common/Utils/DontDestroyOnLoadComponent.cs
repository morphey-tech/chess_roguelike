using UnityEngine;

namespace LiteUI.Common.Utils
{
    public class DontDestroyOnLoadComponent : MonoBehaviour
    {
        private void Start()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}
