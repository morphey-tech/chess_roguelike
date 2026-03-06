namespace Project.Gameplay.Gameplay.Artifacts
{
    public interface IArtifact
    {
        string ConfigId { get; }
        void OnAcquired(ArtifactContext context);
        void OnRemoved(ArtifactContext context);
    }
}
