using Project.Core.Core.Configs.Passive;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Gameplay.UI
{
    /// <summary>
    /// Компонент отображения иконки пассивки.
    /// </summary>
    public class PassiveIconView : MonoBehaviour
    {
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _tooltipText;

        public void Setup(PassiveConfig config)
        {
            if (_iconImage != null && !string.IsNullOrEmpty(config.Icon))
            {
                // TODO: Загрузка иконки через AssetService
                // _assetService.LoadAssetAsync<Sprite>(config.Icon).OnCompleted(sprite =>
                // {
                //     _iconImage.sprite = sprite;
                //     _iconImage.enabled = true;
                // });
            }

            if (_tooltipText != null)
            {
                _tooltipText.text = $"{config.Name}\n{config.Description}";
            }
        }
    }
}