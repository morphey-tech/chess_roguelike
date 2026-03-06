namespace Project.Gameplay.Gameplay.Artifacts.Triggers
{
    /// <summary>
    /// Called when this figure kills an enemy.
    /// </summary>
    public interface IOnUnitKill
    {
        void OnUnitKill(ArtifactTriggerContext context);
    }
}
