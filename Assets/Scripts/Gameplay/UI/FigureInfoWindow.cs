using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Project.Core.Core.Configs.Figure;
using Project.Core.Core.Configs.Passive;
using Project.Core.Window;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.UI;
using TMPro;
using UnityEngine;
using VContainer;

namespace Project.Gameplay.UI
{
    public class FigureInfoWindow : ParameterWindow<FigureInfoWindow.FigureInfoModel>
    {
        [Header("Root")]
        [SerializeField] private RectTransform _root;

        [Header("Figure Info")]
        [SerializeField] private TextMeshProUGUI _figureName;
        [SerializeField] private TextMeshProUGUI _figureDescription;

        [Header("Stats")]
        [SerializeField] private TextMeshProUGUI _hpText;
        [SerializeField] private TextMeshProUGUI _attackText;
        [SerializeField] private TextMeshProUGUI _defenceText;
        [SerializeField] private TextMeshProUGUI _evasionText;
        [SerializeField] private TextMeshProUGUI _attackRangeText;

        [Header("Passives")]
        [SerializeField] private RectTransform _passivesContainer;
        [SerializeField] private PassiveIconView _passiveIconPrefab;

        private readonly List<PassiveIconView> _activePassiveIcons = new();
        private IUIAssetService _iuiAssetService;

        [Inject]
        private void Construct(IUIAssetService iuiAssetService)
        {
            _iuiAssetService = iuiAssetService;
        }

        protected override void OnShow(FigureInfoModel figureInfoModel)
        {
            Figure figure = figureInfoModel.Figure;
            FigureStats stats = figure.Stats;
            FigureInfoConfig? infoConfig = figureInfoModel.InfoConfig;
            List<PassiveConfig> passiveConfigs = figureInfoModel.PassiveConfigs;

            if (infoConfig != null)
            {
                _figureName.text = infoConfig.Name;
                _figureDescription.text = infoConfig.Description;
            }
            else
            {
                _figureName.text = figure.TypeId;
                _figureDescription.text = string.Empty;
            }

            _hpText.text = $"HP: {stats.CurrentHp.Value}/{stats.MaxHp}";
            _attackText.text = $"Атака: {stats.Attack.Value}";
            _defenceText.text = $"Защита: {stats.Defence.Value}";
            _evasionText.text = $"Уклонение: {stats.Evasion.Value}";
            _attackRangeText.text = $"Дальность: {stats.AttackRange}";
            
            RenderPassives(passiveConfigs).Forget();
        }

        //Надо бы это всё дело на пулы перетащить либо еще куда, дестрой просто так - фе
        protected override void OnHidden()
        {
            foreach (PassiveIconView? icon in _activePassiveIcons)
            {
                if (icon != null)
                {
                    Destroy(icon.gameObject);
                }
            }
            _activePassiveIcons.Clear();
        }

        private async UniTask RenderPassives(List<PassiveConfig> passiveConfigs)
        {
            foreach (PassiveIconView? icon in _activePassiveIcons)
            {
                if (icon != null)
                {
                    Destroy(icon.gameObject);
                }
            }
            _activePassiveIcons.Clear();

            if (passiveConfigs.Count == 0)
            {
                return;
            }

            List<UniTask> setupTasks = new(passiveConfigs.Count);
            foreach (PassiveConfig? passiveConfig in passiveConfigs)
            {
                PassiveIconView? iconView = _iuiAssetService.Instantiate(_passiveIconPrefab,
                    _passivesContainer);
                setupTasks.Add(iconView.Setup(passiveConfig));
                _activePassiveIcons.Add(iconView);
            }
            await UniTask.WhenAll(setupTasks)
                .AttachExternalCancellation(gameObject.GetCancellationTokenOnDestroy());
        }

        public class FigureInfoModel
        {
            public Figure Figure { get; set; }
            public FigureInfoConfig? InfoConfig { get; set; }  // Готовый конфиг из FigureConfig.InfoId
            public List<PassiveConfig> PassiveConfigs { get; set; } = new();
        }
    }
}
