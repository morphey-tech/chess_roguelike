using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Project.Editor
{
    /// <summary>
    /// Автоматический запуск игры с Init сцены.
    /// Tools → CGS → Init Run
    /// </summary>
    [InitializeOnLoad]
    public static class InitSceneLoader
    {
        private const string MENU_PATH = "Tools/CGS/Init Run";
        private const string PREF_KEY = "CGS_InitRunEnabled";
        private const string INIT_SCENE_PATH = "Assets/Scenes/InitScene.unity";
        
        private static string _previousScene;

        static InitSceneLoader()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        /// <summary>
        /// Включить/выключить запуск с Init сцены
        /// </summary>
        [MenuItem(MENU_PATH, priority = 100)]
        private static void ToggleInitRun()
        {
            bool current = IsEnabled;
            IsEnabled = !current;
            
            Debug.Log($"[CGS] Init Run: {(IsEnabled ? "ON" : "OFF")}");
        }

        /// <summary>
        /// Галочка в меню
        /// </summary>
        [MenuItem(MENU_PATH, true)]
        private static bool ToggleInitRunValidate()
        {
            Menu.SetChecked(MENU_PATH, IsEnabled);
            return true;
        }

        private static bool IsEnabled
        {
            get => EditorPrefs.GetBool(PREF_KEY, false);
            set => EditorPrefs.SetBool(PREF_KEY, value);
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (!IsEnabled) return;

            switch (state)
            {
                case PlayModeStateChange.ExitingEditMode:
                    // Перед запуском — запоминаем текущую сцену и переключаемся на Init
                    HandleExitingEditMode();
                    break;
                    
                case PlayModeStateChange.EnteredEditMode:
                    // После остановки — возвращаемся к предыдущей сцене
                    HandleEnteredEditMode();
                    break;
            }
        }

        private static void HandleExitingEditMode()
        {
            // Проверяем существование Init сцены
            if (!System.IO.File.Exists(INIT_SCENE_PATH))
            {
                Debug.LogError($"[CGS] Init scene not found at: {INIT_SCENE_PATH}");
                return;
            }

            // Запоминаем текущую сцену
            var currentScene = EditorSceneManager.GetActiveScene();
            _previousScene = currentScene.path;

            // Если уже на Init сцене — ничего не делаем
            if (currentScene.path == INIT_SCENE_PATH)
            {
                return;
            }

            // Сохраняем текущую сцену если есть изменения
            if (currentScene.isDirty)
            {
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    // Пользователь сохранил или отказался
                }
                else
                {
                    // Пользователь отменил — не запускаем
                    EditorApplication.isPlaying = false;
                    return;
                }
            }

            // Переключаемся на Init сцену
            EditorSceneManager.OpenScene(INIT_SCENE_PATH);
            Debug.Log($"[CGS] Switched to Init scene for play mode");
        }

        private static void HandleEnteredEditMode()
        {
            // Возвращаемся к предыдущей сцене
            if (!string.IsNullOrEmpty(_previousScene) && 
                _previousScene != INIT_SCENE_PATH &&
                System.IO.File.Exists(_previousScene))
            {
                EditorSceneManager.OpenScene(_previousScene);
                Debug.Log($"[CGS] Returned to: {_previousScene}");
            }
            
            _previousScene = null;
        }
    }
}

