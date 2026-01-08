using UnityEngine;

namespace LiteUI.Element.Progress
{
    public class ProgressDotItem : MonoBehaviour
    {
        [SerializeField]
        private GameObject _onItem = null!;
        [SerializeField]
        private GameObject _offItem = null!;

        public bool On
        {
            set
            {
                _onItem.SetActive(value);
                _offItem.SetActive(!value);
            }
        }
    }
}
