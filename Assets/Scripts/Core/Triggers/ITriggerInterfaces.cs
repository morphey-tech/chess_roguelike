namespace Project.Core.Core.Triggers
{
    /// <summary>
    /// Trigger for OnBattleStart events.
    /// </summary>
    public interface IOnBattleStart : ITrigger
    {
        TriggerResult HandleBattleStart(IBattleContext context);
    }

    /// <summary>
    /// Trigger for OnBattleEnd events.
    /// </summary>
    public interface IOnBattleEnd : ITrigger
    {
        TriggerResult HandleBattleEnd(IBattleContext context);
    }

    /// <summary>
    /// Trigger for OnUnitKill events.
    /// </summary>
    public interface IOnUnitKill : ITrigger
    {
        TriggerResult HandleUnitKill(IKillContext context);
    }

    /// <summary>
    /// Trigger for OnUnitDeath events.
    /// </summary>
    public interface IOnUnitDeath : ITrigger
    {
        TriggerResult HandleUnitDeath(IKillContext context);
    }

    /// <summary>
    /// Trigger for OnAllyDeath events.
    /// </summary>
    public interface IOnAllyDeath : ITrigger
    {
        TriggerResult HandleAllyDeath(IKillContext context);
    }

    /// <summary>
    /// Trigger for OnDamageReceived events.
    /// </summary>
    public interface IOnDamageReceived : ITrigger
    {
        TriggerResult HandleDamageReceived(IDamageContext context);
    }

    /// <summary>
    /// Trigger for OnDamageDealt events.
    /// </summary>
    public interface IOnDamageDealt : ITrigger
    {
        TriggerResult HandleDamageDealt(IDamageContext context);
    }

    /// <summary>
    /// Trigger for OnAttack events.
    /// </summary>
    public interface IOnAttack : ITrigger
    {
        TriggerResult HandleAttack(IDamageContext context);
    }

    /// <summary>
    /// Trigger for OnBeforeHit events.
    /// </summary>
    public interface IOnBeforeHit : ITrigger
    {
        TriggerResult HandleBeforeHit(IDamageContext context);
    }

    /// <summary>
    /// Trigger for OnAfterHit events.
    /// </summary>
    public interface IOnAfterHit : ITrigger
    {
        TriggerResult HandleAfterHit(IDamageContext context);
    }

    /// <summary>
    /// Trigger for OnTurnStart events.
    /// </summary>
    public interface IOnTurnStart : ITrigger
    {
        TriggerResult HandleTurnStart(ITurnContext context);
    }

    /// <summary>
    /// Trigger for OnTurnEnd events.
    /// </summary>
    public interface IOnTurnEnd : ITrigger
    {
        TriggerResult HandleTurnEnd(ITurnContext context);
    }

    /// <summary>
    /// Trigger for OnMove events.
    /// </summary>
    public interface IOnMove : ITrigger
    {
        TriggerResult HandleMove(IMoveContext context);
    }

    /// <summary>
    /// Trigger for OnReward events.
    /// </summary>
    public interface IOnReward : ITrigger
    {
        TriggerResult HandleReward(IRewardContext context);
    }

    /// <summary>
    /// Trigger for OnRunStart events.
    /// </summary>
    public interface IOnRunStart : ITrigger
    {
        TriggerResult HandleRunStart(IRunContext context);
    }

    /// <summary>
    /// Trigger for OnStageEnter events.
    /// </summary>
    public interface IOnStageEnter : ITrigger
    {
        TriggerResult HandleStageEnter(IRunContext context);
    }

    /// <summary>
    /// Trigger for OnStageLeave events.
    /// </summary>
    public interface IOnStageLeave : ITrigger
    {
        TriggerResult HandleStageLeave(IRunContext context);
    }
}
