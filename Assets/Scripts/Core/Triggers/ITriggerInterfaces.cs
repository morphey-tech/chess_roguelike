namespace Project.Core.Core.Triggers
{
    /// <summary>
    /// Trigger for OnBattleStart events.
    /// </summary>
    public interface IOnBattleStart : ITrigger { }

    /// <summary>
    /// Trigger for OnBattleEnd events.
    /// </summary>
    public interface IOnBattleEnd : ITrigger { }

    /// <summary>
    /// Trigger for OnUnitKill events.
    /// </summary>
    public interface IOnUnitKill : ITrigger { }

    /// <summary>
    /// Trigger for OnUnitDeath events.
    /// </summary>
    public interface IOnUnitDeath : ITrigger { }

    /// <summary>
    /// Trigger for OnAllyDeath events.
    /// </summary>
    public interface IOnAllyDeath : ITrigger { }

    /// <summary>
    /// Trigger for OnDamageReceived events.
    /// </summary>
    public interface IOnDamageReceived : ITrigger { }

    /// <summary>
    /// Trigger for OnDamageDealt events.
    /// </summary>
    public interface IOnDamageDealt : ITrigger { }

    /// <summary>
    /// Trigger for OnAttack events.
    /// </summary>
    public interface IOnAttack : ITrigger { }

    /// <summary>
    /// Trigger for OnBeforeHit events.
    /// </summary>
    public interface IOnBeforeHit : ITrigger { }

    /// <summary>
    /// Trigger for OnAfterHit events.
    /// </summary>
    public interface IOnAfterHit : ITrigger { }

    /// <summary>
    /// Trigger for OnTurnStart events.
    /// </summary>
    public interface IOnTurnStart : ITrigger { }

    /// <summary>
    /// Trigger for OnTurnEnd events.
    /// </summary>
    public interface IOnTurnEnd : ITrigger { }

    /// <summary>
    /// Trigger for OnMove events.
    /// </summary>
    public interface IOnMove : ITrigger { }

    /// <summary>
    /// Trigger for OnReward events.
    /// </summary>
    public interface IOnReward : ITrigger { }

    /// <summary>
    /// Trigger for OnRunStart events.
    /// </summary>
    public interface IOnRunStart : ITrigger { }

    /// <summary>
    /// Trigger for OnStageEnter events.
    /// </summary>
    public interface IOnStageEnter : ITrigger { }

    /// <summary>
    /// Trigger for OnStageLeave events.
    /// </summary>
    public interface IOnStageLeave : ITrigger { }
}
