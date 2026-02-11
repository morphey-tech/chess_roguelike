using System;
using MessagePipe;
using Project.Core.Core.Grid;
using Project.Core.Core.Logging;
using Project.Core.Core.Physics;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Input.Messages;
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

        private InputActionMap _gameplayMap;
        private InputAction _clickAction;
        private InputAction _pointAction;
        private InputAction _endTurnAction;
        private InputAction _cancelAction;

        private Camera _camera;
        private int? _hoveredFigureId;

        public InputDispatcher(
            InputActionAsset inputActions,
            IPublisher<RawClickMessage> rawClickPublisher,
            IPublisher<CellClickedMessage> cellClickedPublisher,
            IPublisher<EndTurnRequestedMessage> endTurnPublisher,
            IPublisher<CancelRequestedMessage> cancelPublisher,
            IPublisher<FigureHoverChangedMessage> figureHoverChangedPublisher,
            ILogService logService)
        {
            _inputActions = inputActions;
            _rawClickPublisher = rawClickPublisher;
            _cellClickedPublisher = cellClickedPublisher;
            _endTurnPublisher = endTurnPublisher;
            _cancelPublisher = cancelPublisher;
            _figureHoverChangedPublisher = figureHoverChangedPublisher;
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
            UpdateHoveredFigure(ray);

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
            UpdateHoveredFigure(ray);
        }

        private void UpdateHoveredFigure(Ray ray)
        {
            int? hoveredFigureId = null;
            if (Physics.Raycast(ray, out RaycastHit hit, PhysicsSettings.DefaultRaycastDistance, PhysicsSettings.FigureLayerMask))
            {
                EntityLink entityLink = hit.collider.GetComponentInParent<EntityLink>();
                if (entityLink?.GetEntity() is Figure figure)
                {
                    hoveredFigureId = figure.Id;
                }
            }

            if (_hoveredFigureId == hoveredFigureId)
                return;

            _hoveredFigureId = hoveredFigureId;
            _figureHoverChangedPublisher.Publish(new FigureHoverChangedMessage(hoveredFigureId));
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
