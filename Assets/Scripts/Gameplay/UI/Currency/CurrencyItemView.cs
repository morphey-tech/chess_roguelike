using System;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Project.Core.Core.Assets;
using Project.Core.Core.Configs.Economy;
using Project.Gameplay.Gameplay.Configs;
using Project.Gameplay.Gameplay.Economy;
using Project.Gameplay.Gameplay.Input.Messages;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VContainer;

namespace Project.Gameplay.Gameplay.UI.Currency
{
    public class CurrencyItemView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _text;

        private ConfigProvider _configProvider = null!;
        private EconomyService _economyService = null!;
        private IAssetService _assetService = null!;
        private IPublisher<string, TooltipMessage> _tooltipPublisher;

        private ResourceDefinition _config;
        private IDisposable? _disposable;
        
        [Inject]
        private void Construct(ConfigProvider configProvider, 
            EconomyService economyService, IAssetService assetService,
            IPublisher<string, TooltipMessage> tooltipPublisher)
        {
            _configProvider = configProvider;
            _economyService = economyService;
            _assetService = assetService;
            _tooltipPublisher = tooltipPublisher;
        }

        public async UniTask Initialize(string id)
        {
            _disposable = _economyService.RunResources.GetProperty(id).Subscribe(OnChanged);
            ResourceDefinitionRepository repo = _configProvider.GetSync<ResourceDefinitionRepository>("resources_conf");
            _config = repo.Require(id);
            _icon.sprite = await _assetService.LoadAsync<Sprite>(_config.Icon);
        }

        private void OnChanged(int value)
        {
            _text.text = value.ToString();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            string content = $"<size=25><color=#FFFFFF>{_config.Name}</color></size>\n" +
                             $"<size=20><color=#D4B9B9>{_config.Description}</color></size>";
            Vector2 staticPosition = _icon.rectTransform.TransformPoint(Vector3.zero);
            _tooltipPublisher.Publish(TooltipMessage.SHOW, TooltipMessage.Show(content, staticPosition, true));
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _tooltipPublisher.Publish(TooltipMessage.HIDE, TooltipMessage.Hide());
        }
        
        private void OnDestroy()
        {
            _disposable?.Dispose();
            _assetService.Release(_icon.sprite);
        }
    }
}