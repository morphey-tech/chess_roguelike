namespace Project.Gameplay.Gameplay.Artifacts.Triggers
{
    /// <summary>
    /// Called when an ally dies.
    /// </summary>
    public interface IOnAllyDeath
    {
        void OnAllyDeath(ArtifactTriggerContext context);
    }
}
