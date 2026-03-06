namespace Project.Gameplay.Gameplay.Artifacts.Triggers
{
    /// <summary>
    /// Called at turn end.
    /// </summary>
    public interface IOnTurnEnd
    {
        void OnTurnEnd(ArtifactTriggerContext context);
    }
}
