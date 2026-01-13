using Cysharp.Threading.Tasks;
using Project.Core.Core.Logging;
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

        private ILogger<MainMenuController> _logger = null!;
        private ISceneService _sceneService = null!;
        
        [Inject]
        private void Construct(ILogService logService, ISceneService sceneService)
        {
            _logger = logService.CreateLogger<MainMenuController>();
            _sceneService = sceneService;
        }

        private void Start()
        {
            SetupButtons();
            ShowMainPanel();
            _continueButton.interactable = false;
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
            StartGame().Forget();
        }

        private void OnContinueClicked()
        {
            _logger.Info("[MainMenu] Continue");
            StartGame().Forget();
        }

        private async UniTaskVoid StartGame()
        {
            SetButtonsInteractable(false);
            await _sceneService.LoadAsync(
                _gameSceneName,
                SceneLoadParams.Default,
                new SceneTransitionData());
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
