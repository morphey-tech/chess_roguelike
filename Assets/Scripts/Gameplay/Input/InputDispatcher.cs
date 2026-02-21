using System;
using System.Collections.Generic;
using MessagePipe;
using Project.Core.Core.Grid;
using Project.Core.Core.Logging;
using Project.Core.Core.Physics;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Input.Messages;
using Project.Gameplay.Gameplay.Run;
using Project.Gameplay.Presentations;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer.Unity;

namespace Project.Gameplay.Gameplay.Input
{
    /// <summary>
    /// Dispatches input events via MessagePipe.
    /// Does NOT contain game logic - only translates input to events.
    /// Services subscribe to events they care about.
    /// </summary>
    public sealed class InputDispatcher : IStartable, IDisposable
    {
        private readonly InputActionAsset _inputActions;
        private readonly IPublisher<RawClickMessage> _rawClickPublisher;
        private readonly IPublisher<CellClickedMessage> _cellClickedPublisher;
        private readonly IPublisher<EndTurnRequestedMessage> _endTurnPublisher;
        private readonly IPublisher<CancelRequestedMessage> _cancelPublisher;
        private readonly IPublisher<FigureHoverChangedMessage> _figureHoverChangedPublisher;
        private readonly ILogger<InputDispatcher> _logger;
        private readonly RunHolder _runHolder;

        private InputActionMap _gameplayMap;
        private InputAction _clickAction;
        private InputAction _pointAction;
        private InputAction _endTurnAction;
        private InputAction _cancelAction;

        private Camera _camera;
        private int? _hoveredFigureId;
        
        // Reusable buffer for RaycastNonAlloc (max 16 figures in ray path)
        private readonly RaycastHit[] _raycastBuffer = new RaycastHit[16];

        public InputDispatcher(
            InputActionAsset inputActions,
            IPublisher<RawClickMessage> rawClickPublisher,
            IPublisher<CellClickedMessage> cellClickedPublisher,
            IPublisher<EndTurnRequestedMessage> endTurnPublisher,
            IPublisher<CancelRequestedMessage> cancelPublisher,
            IPublisher<FigureHoverChangedMessage> figureHoverChangedPublisher,
            RunHolder runHolder,
            ILogService logService)
        {
            _inputActions = inputActions;
            _rawClickPublisher = rawClickPublisher;
            _cellClickedPublisher = cellClickedPublisher;
            _endTurnPublisher = endTurnPublisher;
            _cancelPublisher = cancelPublisher;
            _figureHoverChangedPublisher = figureHoverChangedPublisher;
            _runHolder = runHolder;
            _logger = logService.CreateLogger<InputDispatcher>();
        }

        void IStartable.Start()
        {
            SetupActions();
            _logger.Info("InputDispatcher started");
        }

        private void SetupActions()
        {
            _gameplayMap = _inputActions.FindActionMap("Gameplay", true);

            _clickAction   = _gameplayMap.FindAction("Click", true);
            _pointAction   = _gameplayMap.FindAction("Point", true);
            _endTurnAction = _gameplayMap.FindAction("EndTurn", true);
            _cancelAction  = _gameplayMap.FindAction("Cancel", false);

            _clickAction.started += OnClickStarted;
            _pointAction.performed += OnPointPerformed;
            _endTurnAction.performed += OnEndTurnPerformed;
            if (_cancelAction != null)
                _cancelAction.performed += OnCancelPerformed;

            _gameplayMap.Enable();

            _logger.Info($"Input bound: {_gameplayMap.name}");
        }

        private void OnClickStarted(InputAction.CallbackContext context)
        {
            if (_camera == null)
            {
                _camera = Camera.main;
                if (_camera == null) return;
            }

            Vector2 screenPos = _pointAction.ReadValue<Vector2>();
            Ray ray = _camera.ScreenPointToRay(screenPos);

            // Always publish raw click for services that need it (PrepareService, etc.)
            _rawClickPublisher.Publish(new RawClickMessage(ray, screenPos));
            UpdateHoveredFigure(ray, screenPos);

            // Also publish cell click if we hit a cell
            if (Physics.Raycast(ray, out RaycastHit hit, PhysicsSettings.DefaultRaycastDistance, PhysicsSettings.CellLayerMask))
            {
                Vector3 p = hit.point;

                // Клетки спавнятся на (col, 0, row) - это центр клетки
                int col = Mathf.FloorToInt(p.x + 0.5f);
                int row = Mathf.FloorToInt(p.z + 0.5f);

                _logger.Debug($"Hit: {p}, Cell: row={row}, col={col}");

                GridPosition gridPos = new(row, col);
                _cellClickedPublisher.Publish(new CellClickedMessage(gridPos));
            }
        }

        private void OnPointPerformed(InputAction.CallbackContext context)
        {
            if (_camera == null)
            {
                _camera = Camera.main;
                if (_camera == null) return;
            }

            Vector2 screenPos = _pointAction.ReadValue<Vector2>();
            Ray ray = _camera.ScreenPointToRay(screenPos);
            UpdateHoveredFigure(ray, screenPos);
        }

        private void UpdateHoveredFigure(Ray ray, Vector2 screenPos)
        {
            int? hoveredFigureId = null;
            
            // Сначала пытаемся попасть в клетку — это даст нам "правильную" позицию
            if (Physics.Raycast(ray, out RaycastHit cellHit, PhysicsSettings.DefaultRaycastDistance, PhysicsSettings.CellLayerMask))
            {
                // Получаем позицию клетки
                Vector3 p = cellHit.point;
                int col = Mathf.FloorToInt(p.x + 0.5f);
                int row = Mathf.FloorToInt(p.z + 0.5f);
                
                // Теперь ищем фигуру именно на этой клетке через RaycastAll
                // Сортируем по distance и берём первую фигуру на нужной клетке
                int hitCount = Physics.RaycastNonAlloc(
                    ray,
                    _raycastBuffer,
                    PhysicsSettings.DefaultRaycastDistance,
                    PhysicsSettings.FigureLayerMask);
                
                if (hitCount > 0)
                {
                    System.Array.Sort(_raycastBuffer, 0, hitCount, RaycastHitComparer.Instance);
                    
                    for (int i = 0; i < hitCount; i++)
                    {
                        EntityLink entityLink = _raycastBuffer[i].collider.GetComponentInParent<EntityLink>();
                        if (entityLink?.GetEntity() is Figure figure)
                        {
                            // Проверяем, что фигура находится на этой клетке
                            var cell = _runHolder.Current?.CurrentStage?.Grid?.FindFigure(figure);
                            if (cell != null && cell.Position.Row == row && cell.Position.Column == col)
                            {
                                hoveredFigureId = figure.Id;
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                // Если не попали в клетку, просто берём ближайшую фигуру по raycast
                int hitCount = Physics.RaycastNonAlloc(
                    ray,
                    _raycastBuffer,
                    PhysicsSettings.DefaultRaycastDistance,
                    PhysicsSettings.FigureLayerMask);
                
                if (hitCount > 0)
                {
                    System.Array.Sort(_raycastBuffer, 0, hitCount, RaycastHitComparer.Instance);
                    
                    for (int i = 0; i < hitCount; i++)
                    {
                        EntityLink entityLink = _raycastBuffer[i].collider.GetComponentInParent<EntityLink>();
                        if (entityLink?.GetEntity() is Figure figure)
                        {
                            hoveredFigureId = figure.Id;
                            break;
                        }
                    }
                }
            }

            if (_hoveredFigureId == hoveredFigureId)
                return;

            _hoveredFigureId = hoveredFigureId;
            _figureHoverChangedPublisher.Publish(new FigureHoverChangedMessage(hoveredFigureId));
        }
        
        private sealed class RaycastHitComparer : IComparer<RaycastHit>
        {
            public static readonly RaycastHitComparer Instance = new();
            public int Compare(RaycastHit x, RaycastHit y) => x.distance.CompareTo(y.distance);
        }

        private void OnEndTurnPerformed(InputAction.CallbackContext context)
        {
            _logger.Debug("End turn requested");
            _endTurnPublisher.Publish(new EndTurnRequestedMessage());
        }

        private void OnCancelPerformed(InputAction.CallbackContext context)
        {
            _logger.Debug("Cancel requested");
            _cancelPublisher.Publish(new CancelRequestedMessage());
        }

        public void Dispose()
        {
            if (_clickAction != null)
            {
                _clickAction.started -= OnClickStarted;
            }
            if (_endTurnAction != null)
            {
                _endTurnAction.performed -= OnEndTurnPerformed;
            }
            if (_pointAction != null)
            {
                _pointAction.performed -= OnPointPerformed;
            }
            if (_cancelAction != null)
            {
                _cancelAction.performed -= OnCancelPerformed;
            }
            if (_hoveredFigureId.HasValue)
            {
                _hoveredFigureId = null;
                _figureHoverChangedPublisher.Publish(new FigureHoverChangedMessage(null));
            }
            if (_gameplayMap is { enabled: true })
            {
                _gameplayMap.Disable();
            }
        }
    }
}
