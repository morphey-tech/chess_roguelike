namespace Project.Core.Core.Scene
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
}

