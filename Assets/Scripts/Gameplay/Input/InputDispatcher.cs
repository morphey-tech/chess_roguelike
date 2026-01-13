using System;
using MessagePipe;
using Project.Core.Core.Grid;
using Project.Core.Core.Logging;
using Project.Core.Core.Physics;
using Project.Gameplay.Gameplay.Input.Messages;
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
        private readonly IPublisher<CellClickedMessage> _cellClickedPublisher;
        private readonly IPublisher<EndTurnRequestedMessage> _endTurnPublisher;
        private readonly IPublisher<CancelRequestedMessage> _cancelPublisher;
        private readonly ILogger<InputDispatcher> _logger;

        private InputActionMap _gameplayMap;
        private InputAction _clickAction;
        private InputAction _pointAction;
        private InputAction _endTurnAction;

        private Camera _camera;

        public InputDispatcher(
            InputActionAsset inputActions,
            IPublisher<CellClickedMessage> cellClickedPublisher,
            IPublisher<EndTurnRequestedMessage> endTurnPublisher,
            IPublisher<CancelRequestedMessage> cancelPublisher,
            ILogService logService)
        {
            _inputActions = inputActions;
            _cellClickedPublisher = cellClickedPublisher;
            _endTurnPublisher = endTurnPublisher;
            _cancelPublisher = cancelPublisher;
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

            _clickAction.started += OnClickStarted;
            _endTurnAction.performed += OnEndTurnPerformed;

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

            if (!Physics.Raycast(ray, out RaycastHit hit, PhysicsSettings.DefaultRaycastDistance, PhysicsSettings.CellLayerMask))
            {
                _logger.Warning($"Raycast missed! Screen: {screenPos}, Ray: {ray.origin} -> {ray.direction}");
                return;
            }

            Vector3 p = hit.point;
            
            // Клетки спавнятся на (col, 0, row) - это центр клетки
            // Если pivot в центре, то клетка (0,0) занимает X: -0.5..0.5, Z: -0.5..0.5
            // Добавляем 0.5 чтобы компенсировать
            int col = Mathf.FloorToInt(p.x + 0.5f);
            int row = Mathf.FloorToInt(p.z + 0.5f);

            _logger.Debug($"Hit: {p}, Cell: row={row}, col={col}");

            GridPosition gridPos = new(row, col);
            _cellClickedPublisher.Publish(new CellClickedMessage(gridPos));
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
            if (_gameplayMap is { enabled: true })
            {
                _gameplayMap.Disable();
            }
        }
    }
}
