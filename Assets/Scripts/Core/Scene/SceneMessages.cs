namespace Project.Core.Scene
{
    public readonly struct SceneLoadStartedMessage
    {
        public string SceneName { get; }
        public SceneLoadParams Params { get; }

        public SceneLoadStartedMessage(string sceneName, SceneLoadParams loadParams)
        {
            SceneName = sceneName;
            Params = loadParams;
        }
    }

    public readonly struct SceneLoadCompletedMessage
    {
        public string SceneName { get; }
        public float LoadTime { get; }

        public SceneLoadCompletedMessage(string sceneName, float loadTime)
        {
            SceneName = sceneName;
            LoadTime = loadTime;
        }
    }

    public readonly struct SceneUnloadedMessage
    {
        public string SceneName { get; }

        public SceneUnloadedMessage(string sceneName)
        {
            SceneName = sceneName;
        }
    }
}

