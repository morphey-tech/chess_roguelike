namespace Project.Core.Scene
{
    public readonly struct SceneLoadProgress
    {
        public string SceneName { get; }
        public float Progress { get; }
        public SceneLoadPhase Phase { get; }

        public SceneLoadProgress(string sceneName, float progress, SceneLoadPhase phase)
        {
            SceneName = sceneName;
            Progress = progress;
            Phase = phase;
        }
    }
}