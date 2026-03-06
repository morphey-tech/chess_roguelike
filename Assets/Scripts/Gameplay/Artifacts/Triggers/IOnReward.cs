namespace Project.Gameplay.Gameplay.Artifacts.Triggers
{
    /// <summary>
    /// Called at reward selection (loot choice).
    /// </summary>
    public interface IOnReward
    {
        void OnReward(ArtifactTriggerContext context);
    }
}
