using System;
using System.Collections.Generic;
using Project.Core.Core.Configs.Figure;
using Project.Core.Core.Configs.Passive;
using Project.Core.Window;
using Project.Gameplay.Gameplay.Figures;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Gameplay.UI
{
    /// <summary>
    /// Окно отображения информации о выбранной фигуре.
    /// Показывает название, описание, статы и пассивки иконками.
    /// </summary>
    public class FigureInfoWindow : ParameterWindow<FigureInfoWindow.FigureInfoModel>
    {
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
        [SerializeField] private Transform _passivesContainer;
        [SerializeField] private PassiveIconView _passiveIconPrefab;

        private readonly List<PassiveIconView> _activePassiveIcons = new();

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
                PassiveIconView? iconView = Instantiate(_passiveIconPrefab, _passivesContainer);
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
