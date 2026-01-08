using System;
using LiteUI.Common.Logger;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using LiteUI.Addressable.Service;
using LiteUI.Binding.Attributes;
using LiteUI.Common.Extensions;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using VContainer;

namespace LiteUI.Element.Images
{
    [UIController("ImageElement"), PublicAPI, RequireComponent(typeof(Image))]
    public class ImageElement : MonoBehaviour
    {
        private static readonly IUILogger _logger = LoggerFactory.GetLogger<ImageElement>();

        private AddressableManager? _addressableManager;
        
        private string? _assetId;
        private Image? _image;
        private Sprite? _addressablesSprite;

        public Action<Exception>? LoadAssetErrorDelegate { get; set; }

        [Inject]
        public void Construct(AddressableManager addressableManager)
        {
            _addressableManager = addressableManager;
        }

        private void OnDestroy()
        {
            if (_addressablesSprite == null || _addressableManager == null) {
                return;
            }
            _addressableManager.Release(_addressablesSprite);
            _addressablesSprite = null!;
        }

        private async UniTask UpdateSprite(string assetId)
        {
            try {
                if (_addressableManager == null) {
                    _logger.Warn($"AddressableManager not binded to load sprite={assetId} object={name}");
                    return;
                }
                Sprite sprite = await _addressableManager.LoadAssetAsync<Sprite>(assetId);
                if (this.IsDestroyed() || !assetId.Equals(_assetId)) {
                    _addressableManager.Release(sprite);
                    return;
                }
                Sprite = sprite;
            } catch (Exception e) {
                if (LoadAssetErrorDelegate != null) {
                    LoadAssetErrorDelegate.Invoke(e);
                    return;
                }

                if (e is OperationCanceledException) {
                    return;
                }
                _logger.Error($"Error while load image={assetId}", e);
            }
        }

        public async UniTask UpdateSpriteAsync(string assetId)
        {
          _assetId = assetId;
          await UpdateSprite(assetId);
        }

        public Color Color
        {
            set => Texture.color = value;
            get => Texture.color;
        }

        public string? ImageId
        {
            set
            {
                _assetId = value;
                if (_assetId == null) {
                    Sprite = null;
                    return;
                }
                UpdateSprite(_assetId).Forget();
            }
        }

        public AssetReference? ImageAsset
        {
            set
            {
                _assetId = value?.AssetGUID;
                if (string.IsNullOrEmpty(_assetId)) {
                    _assetId = null;
                    Sprite = null;
                    return;
                }
                UpdateSprite(_assetId).Forget();
            }
        }

        private Sprite? Sprite
        {
            set
            {
                if (_addressablesSprite != null && _addressableManager != null) {
                    _addressableManager.Release(_addressablesSprite);
                }
                _addressablesSprite = value;
                Texture.overrideSprite = value!;
            }
        }

        private Image Texture
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
