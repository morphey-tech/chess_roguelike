namespace Project.Core.Core.Scene
{
    /// <summary>
    /// Сообщения о загрузке/выгрузке сцен.
    /// </summary>
    public readonly struct SceneMessage
    {
        public const string UNLOADED = "sceneUnloaded";
        public const string LOAD_STARTED = "sceneLoadStarted";
        public const string LOAD_COMPLETED = "sceneLoadCompleted";

        public readonly string Type;
        public readonly string SceneName;
        public readonly SceneLoadParams Params;
        public readonly float LoadTime;

        private SceneMessage(
            string type,
            string sceneName,
            SceneLoadParams parms,
            float loadTime)
        {
            Type = type;
            SceneName = sceneName;
            Params = parms;
            LoadTime = loadTime;
        }

        public static SceneMessage Unloaded(string sceneName)
        {
            return new SceneMessage(UNLOADED, sceneName, default, 0);
        }

        public static SceneMessage LoadStarted(string sceneName, SceneLoadParams loadParams)
        {
            return new SceneMessage(LOAD_STARTED, sceneName, loadParams, 0);
        }

        public static SceneMessage LoadCompleted(string sceneName, float loadTime)
        {
            return new SceneMessage(LOAD_COMPLETED, sceneName, default, loadTime);
        }
    }
}
