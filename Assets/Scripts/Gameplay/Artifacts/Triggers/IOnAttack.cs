namespace Project.Gameplay.Gameplay.Artifacts.Triggers
{
    /// <summary>
    /// Called when figure attacks.
    /// </summary>
    public interface IOnAttack
    {
        void OnAttack(ArtifactTriggerContext context);
    }
}
