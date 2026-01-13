namespace Project.Core.Core.Scene
{
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
}