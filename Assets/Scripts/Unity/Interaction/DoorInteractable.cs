using Cysharp.Threading.Tasks;
using Project.Core.Interaction;
using Project.Core.Logging;
using UnityEngine;
using VContainer;
using ILogger = Project.Core.Logging.ILogger;

namespace Project.Unity.Interaction
{
    public class DoorInteractable : InteractableBase
    {
        [Header("Дверь")]
        [SerializeField] private Transform _doorPivot;
        [SerializeField] private float _openAngle = 90f;
        [SerializeField] private float _openSpeed = 2f;
        [SerializeField] private bool _startOpen = false;
        
        [Header("Блокировка")]
        [SerializeField] private bool _locked = false;
        [SerializeField] private string _requiredKeyId;
        
        [Header("Звуки")]
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _openSound;
        [SerializeField] private AudioClip _closeSound;
        [SerializeField] private AudioClip _lockedSound;
        
        private ILogger _logger;
        private bool _isOpen;
        private bool _isAnimating;
        private float _currentAngle;
        private float _targetAngle;
        
        private InteractionDisplayData _openDisplayData;
        private InteractionDisplayData _closeDisplayData;
        private InteractionDisplayData _lockedDisplayData;
        
        public bool IsOpen => _isOpen;
        public bool IsLocked => _locked;
        
        [Inject]
        public void Construct(ILogService logService)
        {
            _logger = logService.CreateLogger<DoorInteractable>();
        }
        
        private void Awake()
        {
            _openDisplayData = new InteractionDisplayData
            {
                PromptText = "Open",
                ActionKey = "E"
            };
            
            _closeDisplayData = new InteractionDisplayData
            {
                PromptText = "Close",
                ActionKey = "E"
            };
            
            _lockedDisplayData = new InteractionDisplayData
            {
                PromptText = "Locked",
                ActionKey = "E"
            };
            
            if (_startOpen)
            {
                _isOpen = true;
                _currentAngle = _openAngle;
                _targetAngle = _openAngle;
                ApplyRotation();
            }
        }
        
        public new InteractionDisplayData DisplayData
        {
            get
            {
                if (_locked) return _lockedDisplayData;
                return _isOpen ? _closeDisplayData : _openDisplayData;
            }
        }
        
        protected override bool CanInteractInternal()
        {
            return !_isAnimating;
        }
        
        protected override void OnInteractInternal()
        {
            if (_locked)
            {
                PlaySound(_lockedSound);
                _logger?.Debug("Door is locked");
                return;
            }
            
            ToggleDoor().Forget();
        }
        
        private async UniTaskVoid ToggleDoor()
        {
            _isAnimating = true;
            _isOpen = !_isOpen;
            _targetAngle = _isOpen ? _openAngle : 0f;
            
            PlaySound(_isOpen ? _openSound : _closeSound);
            _logger?.Debug($"Door {(_isOpen ? "opening" : "closing")}");
            
            while (!Mathf.Approximately(_currentAngle, _targetAngle))
            {
                _currentAngle = Mathf.MoveTowards(_currentAngle, _targetAngle, _openSpeed * Time.deltaTime * 60f);
                ApplyRotation();
                await UniTask.Yield(destroyCancellationToken);
            }
            
            _isAnimating = false;
        }
        
        private void ApplyRotation()
        {
            if (_doorPivot != null)
            {
                _doorPivot.localRotation = Quaternion.Euler(0f, _currentAngle, 0f);
            }
        }
        
        private void PlaySound(AudioClip clip)
        {
            if (_audioSource != null && clip != null)
            {
                _audioSource.PlayOneShot(clip);
            }
        }
        
        public void Unlock()
        {
            _locked = false;
            _logger?.Debug("Door unlocked");
        }
        
        public void Lock()
        {
            _locked = true;
            _logger?.Debug("Door locked");
        }
        
        public bool TryUnlockWithKey(string keyId)
        {
            if (!_locked) return true;
            if (string.IsNullOrEmpty(_requiredKeyId)) return false;
            
            if (keyId == _requiredKeyId)
            {
                Unlock();
                return true;
            }
            
            return false;
        }
    }
}


