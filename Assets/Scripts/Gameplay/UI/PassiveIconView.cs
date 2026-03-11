using System;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Project.Core.Core.Assets;
using Project.Core.Core.Configs.Passive;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Input.Messages;
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
        [SerializeField] private Image _iconImage = null!;

        private IAssetService _assetService = null!;
        private IPublisher<TooltipShowRequestMessage> _tooltipShowPublisher = null!;
        private IPublisher<TooltipHideRequestMessage> _tooltipHidePublisher = null!;
        private ILogger<PassiveIconView> _logger = null!;
        
        private PassiveConfig? _currentConfig;

        [Inject]
        private void Construct(
            IAssetService assetService,
            IPublisher<TooltipShowRequestMessage> tooltipShowPublisher,
            IPublisher<TooltipHideRequestMessage> tooltipHidePublisher,
            ILogService logService)
        {
            _assetService = assetService;
            _tooltipShowPublisher = tooltipShowPublisher;
            _tooltipHidePublisher = tooltipHidePublisher;
            _logger = logService.CreateLogger<PassiveIconView>();
        }

        public async UniTask Setup(PassiveConfig config)
        {
            _currentConfig = config;
            _iconImage.sprite = null;
            _iconImage.enabled = false;
            
            try
            {
                Sprite? sprite = await _assetService.LoadAssetAsync<Sprite>(config.Icon);
                
                if (sprite != null)
                {
                    _iconImage.sprite = sprite;
                    _iconImage.enabled = true;
                }
                else
                {
                    _logger.Warning($"[PassiveIconView.{config.Id}] sprite is null");
                }
            }
            catch (Exception e)
            {
                _logger.Error($"[PassiveIconView.{config.Id}] Exception: {e.Message}", e);
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
