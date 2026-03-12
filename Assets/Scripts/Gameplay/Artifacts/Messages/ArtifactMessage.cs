namespace Project.Gameplay.Gameplay.Artifacts.Messages
{
    /// <summary>
    /// Сообщения об изменениях артефактов (добавление, удаление, очистка).
    /// </summary>
    public readonly struct ArtifactMessage
    {
        public const string ADDED = "artifactAdded";
        public const string REMOVED = "artifactRemoved";
        public const string CLEARED = "artifactsCleared";

        public readonly string Type;
        public readonly ArtifactInstance? Instance;
        public readonly string ConfigId;

        private ArtifactMessage(string type, ArtifactInstance? instance, string configId)
        {
            Type = type;
            Instance = instance;
            ConfigId = configId;
        }

        public static ArtifactMessage Added(ArtifactInstance instance)
        {
            return new ArtifactMessage(ADDED, instance, string.Empty);
        }

        public static ArtifactMessage Removed(string configId)
        {
            return new ArtifactMessage(REMOVED, null, configId);
        }

        public static ArtifactMessage Cleared()
        {
            return new ArtifactMessage(CLEARED, null, string.Empty);
        }
    }
}
