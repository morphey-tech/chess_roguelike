using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Project.Core.Core.Assets;
using Project.Core.Core.Combat;
using Project.Core.Core.Configs.Figure;
using Project.Core.Core.Grid;
using Project.Core.Core.Logging;
using Project.Core.Core.Physics;
using Project.Core.Core.World;
using Project.Gameplay.Gameplay.Configs;
using Project.Gameplay.Gameplay.Figures;
using Project.Core.Core.Configs.Gameplay;
using Project.Gameplay.Gameplay.Selection;
using Project.Gameplay.Gameplay.Shutdown;
using Project.Gameplay.Gameplay.Visual.Commands.Contexts;
using Project.Gameplay.Presentations;
using Project.Unity.Presentations;
using Project.Unity.Unity.Views.Presentations;
using UnityEngine;
using VContainer;

namespace Project.Unity.Unity.Views
{
    public sealed class FigurePresenter : IFigurePresenter, IGameShutdownCleanup
    {
        private const string FigureControllerAssetKey = "figure_controller";
        private const int MoveAnimationDurationMs = 300;
        private const int AttackAnimationDurationMs = 200;
        private const int DeathAnimationDurationMs = 500;
        private const int PushAnimationDurationMs = 200;

        private readonly EntityService _entityService;
        private readonly ConfigProvider _configProvider;
        private readonly IWorldRoot _worldRoot;
        private readonly IAssetService _assetService;
        private readonly ILogger<FigurePresenter> _logger;
        private GameplayConfig? _gameplayConfig;
        private FigureShatterConfigRepository? _shatterConfigRepo;

        private readonly Dictionary<int, GameObject> _figures = new();
        private readonly Dictionary<int, FigureVisualSet> _visuals = new();
        private readonly Dictionary<int, GridPosition> _positions = new();

        private sealed class FigureVisualSet
        {
            public FigureMovePresenter? Move { get; set; }
            public FigureAttackPresenter? Attack { get; set; }
            public FigureDeathPresenter? Death { get; set; }
            public FigureHitPresenter? Hit { get; set; }
            public DamageTextPresenter? DamageText { get; set; }
            public FigureHealthPresenter? Health { get; set; }
        }

        [Inject]
        private FigurePresenter(
            EntityService entityService,
            ConfigProvider configProvider,
            IWorldRoot worldRoot,
            IAssetService assetService,
            ILogService logService)
        {
            _entityService = entityService;
            _configProvider = configProvider;
            _worldRoot = worldRoot;
            _assetService = assetService;
            _logger = logService.CreateLogger<FigurePresenter>();
        }

        public async UniTask CreateFigure(Figure figure, string viewAssetKey, GridPosition pos, Team team)
        {
            _gameplayConfig ??= await _configProvider.Get<GameplayConfig>("gameplay_conf");

            // Загрузить репозиторий конфигов разрушения если еще не загружен
            if (_shatterConfigRepo == null && !string.IsNullOrEmpty(_gameplayConfig.FigureShatterConfigId))
            {
                _logger.Info($"Loading shatter config repo: {_gameplayConfig.FigureShatterConfigId}");
                _shatterConfigRepo = await _configProvider.Get<FigureShatterConfigRepository>("figure_shatter_conf");
                _logger.Info($"Shatter config repo loaded: {_shatterConfigRepo != null}");
            }

            Vector3 worldPos = GetCellTopPosition(pos);
            
            // Spawn controller prefab
            EntityLink controllerLink = await _entityService.SpawnView(
                figure,
                FigureControllerAssetKey,
                worldPos,
                Quaternion.identity,
                _worldRoot.FigureRoot);

            if (controllerLink == null)
            {
                _logger.Error($"Failed to instantiate figure controller for {figure}");
                return;
            }

            GameObject controller = controllerLink.gameObject;
            
            // IMMEDIATELY hide to prevent flicker - before any other operations
            controller.transform.localScale = Vector3.zero;

            _figures[figure.Id] = controller;
            _positions[figure.Id] = pos;

            // Spawn view as child of controller (still hidden because parent scale is 0)
            GameObject? viewGO = await _entityService.InstantiateAsChild(viewAssetKey, controller.transform);
            if (viewGO == null)
            {
                _logger.Warning($"Failed to instantiate view '{viewAssetKey}' for {figure}");
            }
            
            FigureSelectPresenter? selectPresenter = controllerLink.GetComponent<FigureSelectPresenter>();
            if (selectPresenter != null)
                selectPresenter.InitSelecting();
                
            FigureVisualSet visuals = new()
            {
                Move = controllerLink.GetComponentInChildren<FigureMovePresenter>(true),
                Attack = controllerLink.GetComponentInChildren<FigureAttackPresenter>(true),
                Death = controllerLink.GetComponentInChildren<FigureDeathPresenter>(true),
                Hit = controllerLink.GetComponentInChildren<FigureHitPresenter>(true),
                DamageText = controllerLink.GetComponentInChildren<DamageTextPresenter>(true),
                Health = controllerLink.GetComponentInChildren<FigureHealthPresenter>(true)
            };
            _visuals[figure.Id] = visuals;

            // Инициализируем FigureHealthPresenter
            if (visuals.Health != null)
            {
                await visuals.Health.Init(controllerLink);
            }

            // Применяем начальную видимость HP бара
            ApplyInitialHpBarVisibility(visuals.Health, figure, _gameplayConfig);

            // Настроить FigureDeathShatterPresenter если есть
            FigureDeathShatterPresenter? shatterPresenter = controllerLink.GetComponentInChildren<FigureDeathShatterPresenter>(true);
            if (shatterPresenter != null && _shatterConfigRepo != null && !string.IsNullOrEmpty(_gameplayConfig?.FigureShatterConfigId))
            {
                FigureShatterConfig? shatterConfig = _shatterConfigRepo.Get(_gameplayConfig.FigureShatterConfigId);
                if (shatterConfig != null)
                {
                    _logger.Info($"Setting shatter config for figure {figure.Id}: {shatterConfig.Id}");
                    shatterPresenter.SetConfig(shatterConfig);
                }
                else
                {
                    _logger.Warning($"Shatter config not found: {_gameplayConfig.FigureShatterConfigId}");
                }
            }
            else
            {
                if (shatterPresenter == null)
                    _logger.Warning("FigureDeathShatterPresenter not found on figure controller");
            }

            // Face enemy figures towards player
            if (team == Team.Enemy)
            {
                controller.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            }

            FigureSpawnPresenter? spawnPresenter = controllerLink.GetComponent<FigureSpawnPresenter>();
            if (spawnPresenter != null)
            {
                spawnPresenter.PlaySpawnAsync().Forget();
            }
            else
            {
                controller.transform.localScale = Vector3.one;
            }

            _logger.Info($"Figure {figure} ({viewAssetKey}) created at ({pos.Row}, {pos.Column}), team: {team}");
        }

        public async UniTask MoveFigureAsync(int figureId, GridPosition to)
        {
            if (!_figures.TryGetValue(figureId, out GameObject figureGO))
            {
                _logger.Warning($"No figure {figureId} to move");
                return;
            }

            Vector3 newWorldPos = GetCellTopPosition(to);

            if (_visuals.TryGetValue(figureId, out FigureVisualSet visuals) && visuals.Move != null)
            {
                await visuals.Move.PlayMoveAsync(newWorldPos);
            }
            else
            {
                figureGO.transform.position = newWorldPos;
                await UniTask.Delay(MoveAnimationDurationMs);
            }

            _positions[figureId] = to;
            _logger.Info($"Figure {figureId} moved to ({to.Row}, {to.Column})");
        }

        public UniTask RemoveFigureAsync(int figureId)
        {
            if (!_figures.TryGetValue(figureId, out GameObject _))
                return UniTask.CompletedTask;
            CleanupFigure(figureId);
            return UniTask.CompletedTask;
        }

        private void CleanupFigure(int figureId)
        {
            _figures.Remove(figureId);
            _visuals.Remove(figureId);
            _positions.Remove(figureId);
            
            _entityService.Destroy(figureId);
            _logger.Debug($"Figure {figureId} removed");
        }

        public async UniTask PlayAttackAsync(int figureId, GridPosition target)
        {
            if (_visuals.TryGetValue(figureId, out FigureVisualSet visuals) && visuals.Attack != null)
            {
                Vector3 targetPos = GetCellTopPosition(target);
                await visuals.Attack.PlayAttackAsync(targetPos);
            }
            else
            {
                await UniTask.Delay(AttackAnimationDurationMs);
            }
        }

        public async UniTask PlayDamageEffectAsync(int figureId)
        {
            if (!_figures.TryGetValue(figureId, out GameObject figureGO))
                return;

            if (_visuals.TryGetValue(figureId, out FigureVisualSet visuals) && visuals.Hit != null)
            {
                await visuals.Hit.PlayHitAsync();
                return;
            }
            await PlaySimpleDamageEffect(figureGO);
        }

        public async UniTask PlayHealEffectAsync(int figureId)
        {
            if (!_figures.TryGetValue(figureId, out GameObject figureGO))
                return;

            await PlaySimpleHealEffect(figureGO);
        }

        public async UniTask ShowDamageText(int figureId, DamageVisualContext ctx)
        {
            if (!_visuals.TryGetValue(figureId, out FigureVisualSet? visualSet))
            {
                return;
            }
            if (visualSet.DamageText == null)
            {
                return;
            }
            await visualSet.DamageText.ShowFor(ctx);
        }
        
        public void ShowFigureHealthBar(int figureId)
        {
            FigureHealthPresenter? health = GetHealthPresenter(figureId);
            health?.Show();
        }

        public void HideFigureHealthBar(int figureId)
        {
            FigureHealthPresenter? health = GetHealthPresenter(figureId);
            health?.Hide();
        }

        public void SetDamagePreview(int figureId, float? damage)
        {
            FigureHealthPresenter? health = GetHealthPresenter(figureId);
            health?.SetDamagePreview(damage);
        }

        public async UniTask PlayDeathEffectAsync(int figureId)
        {
            HideFigureHealthBar(figureId);

            if (_visuals.TryGetValue(figureId, out FigureVisualSet visuals) && visuals.Death != null)
            {
                await visuals.Death.PlayDeathAsync();
            }
            else
            {
                await UniTask.Delay(DeathAnimationDurationMs);
            }
        }

        public async UniTask PlayPushEffectAsync(int figureId, GridPosition from, GridPosition to)
        {
            if (!_figures.TryGetValue(figureId, out GameObject figureGO))
                return;

            Vector3 newWorldPos = GetCellTopPosition(to);

            if (_visuals.TryGetValue(figureId, out FigureVisualSet visuals) && visuals.Move != null)
            {
                // Could have a special push animation, for now use move
                await visuals.Move.PlayMoveAsync(newWorldPos);
            }
            else
            {
                figureGO.transform.position = newWorldPos;
                await UniTask.Delay(PushAnimationDurationMs);
            }

            _positions[figureId] = to;
            _logger.Info($"Figure {figureId} pushed from ({from.Row}, {from.Column}) to ({to.Row}, {to.Column})");
        }

        private static readonly int ColorId = Shader.PropertyToID("_Color");

        private async UniTask PlaySimpleDamageEffect(GameObject figureGO)
        {
            if (figureGO == null) return;

            Renderer[] renderers = figureGO.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
            {
                _logger.Warning($"No renderers found on {figureGO.name}");
                return;
            }

            var originalMaterials = new List<(Renderer r, Material[] originals)>();
            Color flashColor = Color.red;
            
            foreach (Renderer r in renderers)
            {
                if (r == null) continue;
                originalMaterials.Add((r, r.materials));
            }

            const int flashDelayMs = 80;
            const int flashCount = 2;

            for (int i = 0; i < flashCount && figureGO != null; i++)
            {
                foreach (var (r, _) in originalMaterials)
                {
                    if (r == null) continue;
                    foreach (Material mat in r.materials)
                    {
                        if (mat.HasProperty(ColorId))
                            mat.SetColor(ColorId, flashColor);
                    }
                }
                
                await UniTask.Delay(flashDelayMs);
                if (figureGO == null) return;
                
                foreach (var (r, _) in originalMaterials)
                {
                    if (r == null) continue;
                    foreach (Material mat in r.materials)
                    {
                        if (mat.HasProperty(ColorId))
                            mat.SetColor(ColorId, Color.white);
                    }
                }
                
                await UniTask.Delay(flashDelayMs);
            }
        }

        private async UniTask PlaySimpleHealEffect(GameObject figureGO)
        {
            if (figureGO == null) return;

            Renderer[] renderers = figureGO.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return;

            Color healColor = Color.green;
            const int flashDelayMs = 100;
            const int flashCount = 2;

            for (int i = 0; i < flashCount && figureGO != null; i++)
            {
                foreach (Renderer r in renderers)
                {
                    if (r == null) continue;
                    foreach (Material mat in r.materials)
                    {
                        if (mat.HasProperty(ColorId))
                            mat.SetColor(ColorId, healColor);
                    }
                }
                
                await UniTask.Delay(flashDelayMs);
                if (figureGO == null) return;
                
                foreach (Renderer r in renderers)
                {
                    if (r == null) continue;
                    foreach (Material mat in r.materials)
                    {
                        if (mat.HasProperty(ColorId))
                            mat.SetColor(ColorId, Color.white);
                    }
                }
                
                await UniTask.Delay(flashDelayMs);
            }
        }

        public void Clear()
        {
            foreach (GameObject figure in _figures.Values)
            {
                if (figure != null)
                    Object.Destroy(figure);
            }
            _figures.Clear();
            _visuals.Clear();
            _positions.Clear();
            _logger.Debug("Figures cleared");
        }

        void IGameShutdownCleanup.Cleanup()
        {
            Clear();
        }

        private FigureHealthPresenter? GetHealthPresenter(int figureId)
        {
            if (!_visuals.TryGetValue(figureId, out FigureVisualSet visuals))
                return null;
            return visuals.Health;
        }

        private static void ApplyInitialHpBarVisibility(FigureHealthPresenter? health, Figure figure, GameplayConfig config)
        {
            if (health == null || figure == null || config == null)
                return;

            bool visible = HpBarVisibilityPolicy.ShouldShow(
                config.HpBarVisibilityModeAllies,
                config.HpBarVisibilityModeEnemies,
                figure.Team,
                isHovered: false,
                hasFriendlySelection: false);
            health.SetVisible(visible);
        }

        private static Vector3 GetCellTopPosition(GridPosition gridPos)
        {
            const float surfaceY = 0f;

            Vector3 rayOrigin = new(gridPos.Column, PhysicsSettings.CellRaycastHeight, gridPos.Row);

            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit,
                PhysicsSettings.CellRaycastHeight * 2f, PhysicsSettings.CellLayerMask))
            {
                return hit.point;
            }

            return new Vector3(gridPos.Column, surfaceY, gridPos.Row);
        }
    }
}
