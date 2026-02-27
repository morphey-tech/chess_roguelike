using Project.Core.Core.Assets;
using Project.Core.Core.Configs.Passive;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Project.Gameplay.UI
{
    /// <summary>
    /// Компонент отображения иконки пассивки.
    /// </summary>
    public class PassiveIconView : MonoBehaviour
    {
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _tooltipText;

        private IAssetService _assetService;

        [Inject]
        private void Construct(IAssetService assetService)
        {
            _assetService = assetService;
        }

        public async void Setup(PassiveConfig config)
        {
            // Загрузка иконки пассивки
            if (_iconImage != null && !string.IsNullOrEmpty(config.Icon))
            {
                var sprite = await _assetService.LoadAssetAsync<Sprite>(config.Icon);
                if (sprite != null)
                {
                    _iconImage.sprite = sprite;
                    _iconImage.enabled = true;
                }
            }

            // Установка тултипа
            if (_tooltipText != null)
            {
                _tooltipText.text = $"{config.Name}\n{config.Description}";
            }
        }
    }
}