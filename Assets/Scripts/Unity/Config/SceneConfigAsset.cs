using System.Collections.Generic;
using UnityEngine;

namespace Project.Unity.Config
{
    [CreateAssetMenu(fileName = "SceneConfig", menuName = "Game/Scene Config")]
    public class SceneConfigAsset : ScriptableObject
    {
        [Header("Сцены")]
        public string BootScene = "Boot";
        public string MainMenuScene = "MainMenu";
        public string GameScene = "Game";

        [Header("Загрузка")]
        public float MinLoadTime = 0.5f;
        public bool ShowLoadingScreen = true;

        [Header("Дополнительные сцены")]
        public List<SceneInfo> AdditionalScenes = new();
    }

    [System.Serializable]
    public class SceneInfo
    {
        public string SceneName;
        public string DisplayName;
        public bool LoadAdditive;
        public bool PreloadOnBoot;
    }
}

