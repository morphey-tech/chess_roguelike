using Cysharp.Threading.Tasks;
using Project.Core.Core.Logging;
using Project.Core.Core.Save;
using Project.Core.Core.Scene;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Project.Unity.Unity.Bootstrap
{
    public class MainMenuController : MonoBehaviour
    {
        [Header("Кнопки")]
        [SerializeField] private Button _newGameButton;
        [SerializeField] private Button _continueButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _quitButton;

        [Header("Панели")]
        [SerializeField] private GameObject _mainPanel;
        [SerializeField] private GameObject _settingsPanel;

        [Header("Настройки")]
        [SerializeField] private string _gameSceneName = "GameScene";
        [SerializeField] private string _defaultSaveSlot = "slot_0";

        private ILogger<MainMenuController> _logger = null!;
        private ISceneService _sceneService = null!;
        private ISaveService _saveService = null!;
        
        [Inject]
        private void Construct(ILogService logService,
            ISceneService sceneService, ISaveService saveService)
        {
            _logger = logService.CreateLogger<MainMenuController>();
            _sceneService = sceneService;
            _saveService = saveService;
        }

        private void Start()
        {
            SetupButtons();
            ShowMainPanel();
            _continueButton.interactable = false;
            RefreshContinueButtonAsync().Forget();
        }

        private void SetupButtons()
        {
            _newGameButton.onClick.AddListener(OnNewGameClicked);
            _continueButton.onClick.AddListener(OnContinueClicked);
            _settingsButton.onClick.AddListener(OnSettingsClicked);
            _quitButton.onClick.AddListener(OnQuitClicked);
        }

        private void OnNewGameClicked()
        {
            _logger.Info("[MainMenu] New game");
            StartGameAsync(loadSave: false).Forget();
        }

        private void OnContinueClicked()
        {
            _logger.Info("[MainMenu] Continue");
            ContinueGameAsync().Forget();
        }

        private async UniTaskVoid ContinueGameAsync()
        {
            SetButtonsInteractable(false);
            try
            {
                bool loaded = await _saveService.LoadAsync(_defaultSaveSlot);
                if (!loaded)
                {
                    _logger.Warning($"[MainMenu] Continue failed: slot '{_defaultSaveSlot}' not found");
                    await RefreshContinueButtonAsync();
                    _newGameButton.interactable = true;
                    _settingsButton.interactable = true;
                    _quitButton.interactable = true;
                    return;
                }

                await StartGameAsync(loadSave: true);
            }
            catch (System.Exception ex)
            {
                _logger.Error("[MainMenu] Continue failed with exception", ex);
                await RefreshContinueButtonAsync();
                _newGameButton.interactable = true;
                _settingsButton.interactable = true;
                _quitButton.interactable = true;
            }
        }

        private async UniTask StartGameAsync(bool loadSave)
        {
            SetButtonsInteractable(false);
            _logger.Info(loadSave
                ? $"[MainMenu] Starting game from save slot '{_defaultSaveSlot}'"
                : "[MainMenu] Starting new game");
            await _sceneService.LoadAsync(
                _gameSceneName,
                SceneLoadParams.Default,
                new SceneTransitionData());
        }

        private async UniTask RefreshContinueButtonAsync()
        {
            bool hasSave = await _saveService.HasSaveAsync(_defaultSaveSlot);
            _continueButton.interactable = hasSave;
            _logger.Debug($"[MainMenu] Continue available: {hasSave} (slot='{_defaultSaveSlot}')");
        }

        private void OnSettingsClicked()
        {
            _logger.Info("[MainMenu] Settings");
            ShowSettingsPanel();
        }

        private void OnQuitClicked()
        {
            _logger.Info("[MainMenu] Quit");

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void ShowMainPanel()
        {
            _mainPanel.SetActive(true);
            _settingsPanel.SetActive(false);
        }

        private void ShowSettingsPanel()
        {
            _mainPanel.SetActive(false);
            _settingsPanel.SetActive(true);
        }

        private void SetButtonsInteractable(bool interactable)
        {
            _newGameButton.interactable = interactable;
            _continueButton.interactable = interactable;
            _settingsButton.interactable = interactable;
            _quitButton.interactable = interactable;
        }

        private void OnDestroy()
        {
            _newGameButton.onClick.RemoveAllListeners();
            _continueButton.onClick.RemoveAllListeners();
            _settingsButton.onClick.RemoveAllListeners();
            _quitButton.onClick.RemoveAllListeners();
        }
    }
}
