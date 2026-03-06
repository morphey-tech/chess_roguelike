namespace Project.Gameplay.Gameplay.Artifacts.Triggers
{
    /// <summary>
    /// Called when figure moves.
    /// </summary>
    public interface IOnMove
    {
        void OnMove(ArtifactTriggerContext context);
    }
}
