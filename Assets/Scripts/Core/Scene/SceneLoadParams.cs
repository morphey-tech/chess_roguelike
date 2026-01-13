namespace Project.Core.Core.Scene
{
    public readonly struct SceneLoadParams
    {
        public bool ShowLoadingScreen { get; }
        public bool UnloadPrevious { get; }
        public float MinLoadTime { get; }
        public string TransitionId { get; }
        public bool CanDoHeavyCleanup { get; }

        public SceneLoadParams(
            bool showLoadingScreen = true,
            bool unloadPrevious = true,
            float minLoadTime = 0.5f,
            string? transitionId = null,
            bool canDoHeavyCleanup = false)
        {
            ShowLoadingScreen = showLoadingScreen;
            UnloadPrevious = unloadPrevious;
            MinLoadTime = minLoadTime;
            TransitionId = transitionId ?? string.Empty;
            CanDoHeavyCleanup = canDoHeavyCleanup;
        }

        public static SceneLoadParams Default => new(
            showLoadingScreen: true,
            unloadPrevious: true,
            minLoadTime: 0.5f,
            canDoHeavyCleanup: true);
        
        public static SceneLoadParams Instant => new(
            showLoadingScreen: false,
            unloadPrevious: true,
            minLoadTime: 0f);
        
        public static SceneLoadParams Additive => new(
            showLoadingScreen: false,
            unloadPrevious: false,
            minLoadTime: 0f,
            canDoHeavyCleanup: false);
    }

}