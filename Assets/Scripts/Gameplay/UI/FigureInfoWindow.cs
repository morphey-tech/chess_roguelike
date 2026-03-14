#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Project.Core.Core.Configs.Figure;
using Project.Core.Core.Configs.Passive;
using Project.Core.Core.Logging;
using Project.Core.Window;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.UI.Project.Gameplay.Gameplay.UI;
using TMPro;
using UnityEngine;
using VContainer;

namespace Project.Gameplay.UI
{
    public class FigureInfoWindow : ParameterWindow<FigureInfoWindow.FigureInfoModel>
    {
        [Header("Root")]
        [SerializeField] private RectTransform _root = null!;

        [Header("Figure Info")]
        [SerializeField] private TextMeshProUGUI _figureName = null!;
        [SerializeField] private TextMeshProUGUI _figureDescription = null!;

        [Header("Stats")]
        [SerializeField] private TextMeshProUGUI _hpText = null!;
        [SerializeField] private TextMeshProUGUI _attackText = null!;
        [SerializeField] private TextMeshProUGUI _defenceText = null!;
        [SerializeField] private TextMeshProUGUI _evasionText = null!;
        [SerializeField] private TextMeshProUGUI _attackRangeText = null!;

        [Header("Passives")]
        [SerializeField] private RectTransform _passivesContainer = null!;

        public Figure? CurrentFigure { get; private set; }
        
        private IUIAssetService _assetService = null!;
        private ILogger<FigureInfoWindow> _logger = null!;

        private PassiveIconView? _passiveIconViewPrefab;
        private readonly List<PassiveIconView> _activePassiveIcons = new();
        private readonly List<PassiveIconView> _pooledPassiveIcons = new();

        private CancellationTokenSource? _cts;

        [Inject]
        private void Construct(IUIAssetService assetService, ILogService logService)
        {
            _assetService = assetService ?? throw new ArgumentNullException(nameof(assetService));
            _logger = logService.CreateLogger<FigureInfoWindow>();
        }

        public async UniTask PreloadPassiveIconPrefab()
        {
            if (_passiveIconViewPrefab != null)
            {
                return;
            }
            _passiveIconViewPrefab = await _assetService.CreateAsync<PassiveIconView>("PassiveIconView", parent: null);
            _passiveIconViewPrefab.gameObject.SetActive(false);
        }

        protected override void OnShow(FigureInfoModel value)
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            UpdateFigureInfo(value);
            RenderPassives(value.PassiveConfigs, _cts.Token).Forget();
        }

        private void UpdateFigureInfo(FigureInfoModel model)
        {
            CurrentFigure = model.Figure;
            FigureStats stats = CurrentFigure.Stats;
            FigureInfoConfig? infoConfig = model.InfoConfig;

            _figureName.text = infoConfig?.Name ?? CurrentFigure.TypeId;
            _figureDescription.text = infoConfig?.Description ?? string.Empty;

            _hpText.text = $"{stats.CurrentHp.Value}/{stats.MaxHp}";
            _attackText.text = $"{stats.Attack.Value}";
            _defenceText.text = $"{stats.Defence.Value}";
            _evasionText.text = $"{stats.Evasion.Value}";
            _attackRangeText.text = $"{stats.AttackRange}";

            SetStatColor(_attackText, stats.Attack.Value, stats.Attack.BaseValue);
            SetStatColor(_defenceText, stats.Defence.Value, stats.Defence.BaseValue);
            SetStatColor(_evasionText, stats.Evasion.Value, stats.Evasion.BaseValue);
        }

        private static void SetStatColor(TextMeshProUGUI text, float current, float baseValue)
        {
            text.color = current > baseValue ? Color.green
                        : current < baseValue ? Color.red
                        : Color.white;
        }

        protected override void OnHidden()
        {
            _cts?.Cancel();
            foreach (PassiveIconView? icon in _activePassiveIcons)
            {
                icon.gameObject.SetActive(false);
            }
            _activePassiveIcons.Clear();
            CurrentFigure = null;
        }

        private async UniTask RenderPassives(List<PassiveConfig> passiveConfigs, CancellationToken token)
        {
            foreach (PassiveIconView? icon in _activePassiveIcons)
            {
                icon.gameObject.SetActive(false);
                _pooledPassiveIcons.Add(icon);
            }
            _activePassiveIcons.Clear();

            if (_passiveIconViewPrefab == null || passiveConfigs.Count == 0)
            {
                return;
            }

            foreach (PassiveConfig? passiveConfig in passiveConfigs)
            {
                token.ThrowIfCancellationRequested();

                PassiveIconView iconView;
                if (_pooledPassiveIcons.Count > 0)
                {
                    iconView = _pooledPassiveIcons[0];
                    _pooledPassiveIcons.RemoveAt(0);
                }
                else
                {
                    iconView = _assetService.Instantiate(_passiveIconViewPrefab, _passivesContainer);
                }

                iconView.transform.localScale = Vector3.zero;
                iconView.gameObject.SetActive(true);
                _activePassiveIcons.Add(iconView);

                try
                {
                    await iconView.Setup(passiveConfig).AttachExternalCancellation(token);
                    await iconView.transform.DOScale(Vector3.one, 0.15f)
                        .SetEase(Ease.OutBack)
                        .AsyncWaitForCompletion();
                }
                catch (OperationCanceledException)
                {
                    iconView.gameObject.SetActive(false);
                    break;
                }
                catch (Exception e)
                {
                    _logger.Error($"[FigureInfoWindow] Failed to setup passive icon: {e}");
                    iconView.gameObject.SetActive(false);
                }
            }
        }

        private void OnDestroy()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }

        public class FigureInfoModel
        {
            public Figure Figure { get; set; } = null!;
            public FigureInfoConfig? InfoConfig { get; set; }
            public List<PassiveConfig> PassiveConfigs { get; set; } = new();
        }

    }
}