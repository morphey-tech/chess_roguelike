using UnityEngine;
using UnityEngine.UI;

namespace LiteUI.UI.Component
{
    [RequireComponent(typeof(Image))]
    public class ImageMaterialSwitcher : MonoBehaviour
    {
        [SerializeField]
        private Material _defaultMaterial = null!;

        [SerializeField]
        private Material _alternativeMaterial = null!;

        private Image? _image;

        public bool Alternative
        {
            set => Image.material = value ? _alternativeMaterial : _defaultMaterial;
        }

        private Image Image
        {
            get
            {
                if (_image == null) {
                    _image = GetComponent<Image>();
                }
                return _image;
            }
        }
    }
}
