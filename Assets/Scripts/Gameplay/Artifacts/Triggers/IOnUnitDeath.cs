namespace Project.Gameplay.Gameplay.Artifacts.Triggers
{
    /// <summary>
    /// Called when any unit dies.
    /// </summary>
    public interface IOnUnitDeath
    {
        void OnUnitDeath(ArtifactTriggerContext context);
    }
}
