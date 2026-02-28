using Cysharp.Threading.Tasks;
using MessagePipe;
using Project.Core.Core.Assets;
using Project.Core.Core.Configs.Passive;
using Project.Gameplay.Gameplay.Input.Messages;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VContainer;

namespace Project.Gameplay.UI
{
    /// <summary>
    /// Компонент отображения иконки пассивки.
    /// </summary>
    public class PassiveIconView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _tooltipText;

        private IAssetService _assetService;
        private IPublisher<TooltipShowRequestMessage> _tooltipShowPublisher;
        private IPublisher<TooltipHideRequestMessage> _tooltipHidePublisher;
        
        private PassiveConfig? _currentConfig;

        [Inject]
        private void Construct(
            IAssetService assetService,
            IPublisher<TooltipShowRequestMessage> tooltipShowPublisher,
            IPublisher<TooltipHideRequestMessage> tooltipHidePublisher)
        {
            _assetService = assetService;
            _tooltipShowPublisher = tooltipShowPublisher;
            _tooltipHidePublisher = tooltipHidePublisher;
        }

        public async UniTask Setup(PassiveConfig config)
        {
            _currentConfig = config;
            
            if (_iconImage != null && !string.IsNullOrEmpty(config.Icon))
            {
                Sprite? sprite = await _assetService.LoadAssetAsync<Sprite>(config.Icon);
                if (sprite != null)
                {
                    _iconImage.sprite = sprite;
                    _iconImage.enabled = true;
                }
            }

            if (_tooltipText != null)
            {
                _tooltipText.text = $"{config.Name}\n{config.Description}";
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_currentConfig == null)
            {
                return;
            }

            string content = $"{_currentConfig.Name}\n{_currentConfig.Description}";
            _tooltipShowPublisher.Publish(new TooltipShowRequestMessage(content, eventData.position));
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _tooltipHidePublisher.Publish(new TooltipHideRequestMessage());
        }
    }
}
