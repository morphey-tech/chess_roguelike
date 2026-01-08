using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace LiteUI.Element.Images
{
    public class DynamicRenderTexture : MonoBehaviour
    {
        private const int BACK_BUFFER_SIZE = 24;
        private const int ANTI_ALIASING = 2;

        [FormerlySerializedAs("_camera")]
        [SerializeField]
        private Camera _renderCamera = null!;
        [FormerlySerializedAs("_secondCamera")]
        [SerializeField]
        private Camera? _alternativeRenderCamera;
        [SerializeField]
        private RawImage _image = null!;

        private RenderTexture? _renderTexture;

        private void Start()
        {
            RectTransform rectTransform = _image.GetComponent<RectTransform>();
            Rect rect = rectTransform.rect;

            _renderTexture = RenderTexture.GetTemporary((int) rect.width, (int) rect.height, BACK_BUFFER_SIZE, RenderTextureFormat.ARGB32);
            _renderTexture.antiAliasing = ANTI_ALIASING;
            _image.color = Color.white;
            _image.texture = _renderTexture;
            _renderCamera.GetComponent<Camera>().targetTexture = _renderTexture;
            if (_alternativeRenderCamera != null) {
                _alternativeRenderCamera.GetComponent<Camera>().targetTexture = _renderTexture;
            }
        }

        private void OnDestroy()
        {
            if (_renderTexture == null) {
                return;
            }
            _image.texture = null!;
            if (_renderCamera != null) {
                _renderCamera.targetTexture = null;
            }
            if (_alternativeRenderCamera != null) {
                _alternativeRenderCamera.targetTexture = null;
            }
            RenderTexture.ReleaseTemporary(_renderTexture);
        }
    }
}
