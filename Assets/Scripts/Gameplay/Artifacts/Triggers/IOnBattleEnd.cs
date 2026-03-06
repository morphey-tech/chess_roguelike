namespace Project.Gameplay.Gameplay.Artifacts.Triggers
{
    /// <summary>
    /// Called when a battle ends (victory).
    /// </summary>
    public interface IOnBattleEnd
    {
        void OnBattleEnd(ArtifactTriggerContext context);
    }
}
