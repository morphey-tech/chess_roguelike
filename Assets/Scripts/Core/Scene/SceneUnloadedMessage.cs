namespace Project.Core.Core.Scene
{
    public readonly struct SceneUnloadedMessage
    {
        public string SceneName { get; }

        public SceneUnloadedMessage(string sceneName)
        {
            SceneName = sceneName;
        }
    }
}