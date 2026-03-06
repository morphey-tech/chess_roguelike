namespace Project.Gameplay.Gameplay.Artifacts.Triggers
{
    /// <summary>
    /// Called when a battle starts.
    /// </summary>
    public interface IOnBattleStart
    {
        void OnBattleStart(ArtifactTriggerContext context);
    }
}
