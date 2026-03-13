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
        [SerializeField] private Vector2 _tooltipOffset;

        private IAssetService _assetService = null!;
        private IPublisher<string, TooltipMessage> _tooltipPublisher = null!;
        private ILogger<PassiveIconView> _logger = null!;

        private PassiveConfig? _currentConfig;

        [Inject]
        private void Construct(
            IAssetService assetService,
            IPublisher<string, TooltipMessage> tooltipPublisher,
            ILogService logService)
        {
            _assetService = assetService;
            _tooltipPublisher = tooltipPublisher;
            _logger = logService.CreateLogger<PassiveIconView>();
        }

        public async UniTask Setup(PassiveConfig config)
        {
            _currentConfig = config;
            _iconImage.sprite = null;
            _iconImage.enabled = false;

            try
            {
                Sprite? sprite = await _assetService.LoadAsync<Sprite>(config.Icon);

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

            string content = $"<size=25><color=#FFFFFF>{_currentConfig.Name}</color></size>\n" +
                             $"<size=20><color=#D4B9B9>{_currentConfig.Description}</color></size>";
            
            Vector2 staticPosition = _iconImage.rectTransform.TransformPoint(Vector3.zero);
            staticPosition.x += _tooltipOffset.x;
            staticPosition.y += _tooltipOffset.y;

            _tooltipPublisher.Publish(TooltipMessage.SHOW,
                TooltipMessage.Show(content, staticPosition, useStaticPosition: true));
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _tooltipPublisher.Publish(TooltipMessage.HIDE, TooltipMessage.Hide());
        }
    }
}
