using System;
using System.Collections.Generic;
using Project.Core.Core.Assets;
using Project.Core.Core.Configs.Figure;
using Project.Core.Core.Configs.Passive;
using Project.Core.Window;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.UI;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;

namespace Project.Gameplay.UI
{
    /// <summary>
    /// Окно отображения информации о выбранной фигуре.
    /// Показывает название, описание, статы и пассивки иконками.
    /// 
    /// Структура префаба:
    /// - FigureInfoWindow (корневой, растянут на весь экран через WindowController)
    ///   └── Root (RectTransform, Vertical Layout Group)
    ///       ├── Header (RectTransform)
    ///       │   ├── FigureName (Text)
    ///       │   └── FigureDescription (Text)
    ///       ├── StatsContainer (RectTransform)
    ///       │   ├── HPText (Text)
    ///       │   ├── AttackText (Text)
    ///       │   ├── DefenceText (Text)
    ///       │   ├── EvasionText (Text)
    ///       │   └── AttackRangeText (Text)
    ///       └── PassivesContainer (RectTransform, Horizontal Layout)
    ///           └── PassiveIconPrefab (PassiveIconView) [disabled]
    /// </summary>
    public class FigureInfoWindow : ParameterWindow<FigureInfoWindow.FigureInfoModel>
    {
        [Header("Root")]
        [SerializeField] private RectTransform _root;

        [Header("Figure Info")]
        [SerializeField] private Text _figureName;
        [SerializeField] private Text _figureDescription;

        [Header("Stats")]
        [SerializeField] private Text _hpText;
        [SerializeField] private Text _attackText;
        [SerializeField] private Text _defenceText;
        [SerializeField] private Text _evasionText;
        [SerializeField] private Text _attackRangeText;

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

        private static void AddLayoutComponents(RectTransform rectTransform, float spacing, TextAnchor alignment)
        {
            // VerticalLayoutGroup
            if (rectTransform.GetComponent<VerticalLayoutGroup>() == null)
            {
                var layout = rectTransform.gameObject.AddComponent<VerticalLayoutGroup>();
                layout.spacing = spacing;
                layout.padding = new RectOffset(10, 10, 10, 10);
                layout.childAlignment = alignment;
                layout.childForceExpandWidth = true;
                layout.childForceExpandHeight = false;
                layout.childControlWidth = true;
                layout.childControlHeight = false;
            }

            // ContentSizeFitter
            if (rectTransform.GetComponent<ContentSizeFitter>() == null)
            {
                var fitter = rectTransform.gameObject.AddComponent<ContentSizeFitter>();
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            }
        }

        private static void AddHorizontalLayoutComponents(RectTransform rectTransform, float spacing, TextAnchor alignment)
        {
            // HorizontalLayoutGroup
            if (rectTransform.GetComponent<HorizontalLayoutGroup>() == null)
            {
                var layout = rectTransform.gameObject.AddComponent<HorizontalLayoutGroup>();
                layout.spacing = spacing;
                layout.padding = new RectOffset(10, 10, 10, 10);
                layout.childAlignment = alignment;
                layout.childForceExpandWidth = false;
                layout.childForceExpandHeight = false;
                layout.childControlWidth = false;
                layout.childControlHeight = false;
            }

            // ContentSizeFitter
            if (rectTransform.GetComponent<ContentSizeFitter>() == null)
            {
                var fitter = rectTransform.gameObject.AddComponent<ContentSizeFitter>();
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            }
        }

        protected override void OnShow(FigureInfoModel figureInfoModel)
        {
            Figure figure = figureInfoModel.Figure;
            FigureStats stats = figure.Stats;
            FigureInfoConfig? infoConfig = figureInfoModel.InfoConfig;
            List<PassiveConfig> passiveConfigs = figureInfoModel.PassiveConfigs;

            // Отображаем основную информацию
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

            RenderPassives(passiveConfigs);

            // Принудительное обновление layout
            if (_root != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(_root);
            }
        }

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

        private void RenderPassives(List<PassiveConfig> passiveConfigs)
        {
            // Очищаем старые иконки
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

            foreach (PassiveConfig? passiveConfig in passiveConfigs)
            {
                PassiveIconView? iconView = _iuiAssetService.Instantiate(_passiveIconPrefab, _passivesContainer);
                iconView.Setup(passiveConfig);
                _activePassiveIcons.Add(iconView);
            }
        }

        public class FigureInfoModel
        {
            public Figure Figure { get; set; }
            public FigureInfoConfig? InfoConfig { get; set; }
            public List<PassiveConfig> PassiveConfigs { get; set; } = new();
        }
    }
}
