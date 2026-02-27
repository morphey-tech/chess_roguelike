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
    public class FigureInfoWindow : ParameterWindow<FigureInfoWindow.Model>
    {
        [Header("Figure Info")]
        [SerializeField] private Image _figureIcon;
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

        [Header("Team")]
        [SerializeField] private Image _teamIndicator;
        [SerializeField] private Color _playerTeamColor = Color.green;
        [SerializeField] private Color _enemyTeamColor = Color.red;

        private readonly List<PassiveIconView> _activePassiveIcons = new();

        protected override void OnShow(Model model)
        {
            Figure figure = model.Figure;
            var stats = figure.Stats;
            var infoConfig = model.InfoConfig;
            var passiveConfigs = model.PassiveConfigs;

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

            // Отображаем статы
            _hpText.text = $"HP: {stats.CurrentHp.Value}/{stats.MaxHp}";
            _attackText.text = $"Атака: {stats.Attack.Value}";
            _defenceText.text = $"Защита: {stats.Defence.Value}";
            _evasionText.text = $"Уклонение: {stats.Evasion.Value}";
            _attackRangeText.text = $"Дальность: {stats.AttackRange}";

            // Отображаем команду
            _teamIndicator.color = figure.Team == Team.Player ? _playerTeamColor : _enemyTeamColor;

            // Отображаем пассивки
            RenderPassives(passiveConfigs);
        }

        protected override void OnHidden()
        {
            foreach (var icon in _activePassiveIcons)
            {
                if (icon != null)
                    Destroy(icon.gameObject);
            }
            _activePassiveIcons.Clear();
        }

        private void RenderPassives(List<PassiveConfig> passiveConfigs)
        {
            // Очищаем старые иконки
            foreach (var icon in _activePassiveIcons)
            {
                if (icon != null)
                    Destroy(icon.gameObject);
            }
            _activePassiveIcons.Clear();

            if (passiveConfigs.Count == 0)
                return;

            foreach (var passiveConfig in passiveConfigs)
            {
                var iconView = Instantiate(_passiveIconPrefab, _passivesContainer);
                iconView.Setup(passiveConfig);
                _activePassiveIcons.Add(iconView);
            }
        }

        public class Model
        {
            public Figure Figure { get; set; }
            public FigureInfoConfig? InfoConfig { get; set; }
            public List<PassiveConfig> PassiveConfigs { get; set; } = new();
        }
    }
}
