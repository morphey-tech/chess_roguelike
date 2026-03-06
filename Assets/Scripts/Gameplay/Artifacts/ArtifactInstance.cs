namespace Project.Gameplay.Gameplay.Artifacts
{
    public sealed class ArtifactInstance
    {
        public string Id { get; }
        public IArtifact Artifact { get; }
        public string ConfigId => Artifact.ConfigId;

        public ArtifactInstance(IArtifact artifact, string id)
        {
            Artifact = artifact;
            Id = id;
        }
    }
}