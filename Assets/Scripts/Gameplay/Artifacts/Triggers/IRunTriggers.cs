namespace Project.Gameplay.Gameplay.Artifacts.Triggers
{
    /// <summary>
    /// Called at run start (new game).
    /// </summary>
    public interface IOnRunStart
    {
        void OnRunStart(RunContext context);
    }

    /// <summary>
    /// Called when reward selection is presented.
    /// Can modify number of choices.
    /// </summary>
    public interface IOnRewardSelect
    {
        void OnRewardSelect(RunContext context);
    }

    /// <summary>
    /// Called when entering a new stage.
    /// </summary>
    public interface IOnStageEnter
    {
        void OnStageEnter(RunContext context);
    }

    /// <summary>
    /// Called when leaving a stage (victory).
    /// </summary>
    public interface IOnStageLeave
    {
        void OnStageLeave(RunContext context);
    }
}
