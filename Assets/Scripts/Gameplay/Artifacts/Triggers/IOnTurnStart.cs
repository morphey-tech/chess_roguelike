namespace Project.Gameplay.Gameplay.Artifacts.Triggers
{
    /// <summary>
    /// Called at turn start.
    /// </summary>
    public interface IOnTurnStart
    {
        void OnTurnStart(ArtifactTriggerContext context);
    }
}
