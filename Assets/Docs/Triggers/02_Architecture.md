# Trigger Architecture

## Interface Hierarchy

### Core Interfaces

```
ITrigger (base)
    │
    ├─ IOnBattleStart     → HandleBattleStart(IBattleContext)
    ├─ IOnBattleEnd       → HandleBattleEnd(IBattleContext)
    ├─ IOnUnitKill        → HandleUnitKill(IKillContext)
    ├─ IOnUnitDeath       → HandleUnitDeath(IKillContext)
    ├─ IOnAllyDeath       → HandleAllyDeath(IKillContext)
    ├─ IOnDamageReceived  → HandleDamageReceived(IDamageContext)
    ├─ IOnDamageDealt     → HandleDamageDealt(IDamageContext)
    ├─ IOnAttack          → HandleAttack(IDamageContext)
    ├─ IOnBeforeHit       → HandleBeforeHit(IDamageContext)
    ├─ IOnAfterHit        → HandleAfterHit(IDamageContext)
    ├─ IOnTurnStart       → HandleTurnStart(ITurnContext)
    ├─ IOnTurnEnd         → HandleTurnEnd(ITurnContext)
    ├─ IOnMove            → HandleMove(IMoveContext)
    ├─ IOnReward          → HandleReward(IRewardContext)
    ├─ IOnRunStart        → HandleRunStart(IRunContext)
    ├─ IOnStageEnter      → HandleStageEnter(IRunContext)
    └─ IOnStageLeave      → HandleStageLeave(IRunContext)
```

### Implementation Pattern

```csharp
public sealed class CriticalPassive : IPassive, IOnBeforeHit
{
    // ITrigger properties
    public int Priority => TriggerPriorities.Normal;
    public TriggerGroup Group => TriggerGroup.Default;
    public TriggerPhase Phase => TriggerPhase.BeforeHit;

    // ITrigger.Execute — wrapper for compatibility
    public TriggerResult Execute(TriggerContext context)
    {
        if (context is not IDamageContext dc) return TriggerResult.Continue;
        return HandleBeforeHit(dc);
    }

    // IOnBeforeHit.HandleBeforeHit — main logic
    public TriggerResult HandleBeforeHit(IDamageContext context)
    {
        context.DamageMultiplier *= _critMultiplier;
        context.IsCritical = true;
        return TriggerResult.Continue;
    }
}
```

---

## Context Interfaces

### Layer Architecture

```
┌─────────────────────────────────────────────────────────┐
│  Core.Triggers (base interfaces)                        │
│  ─────────────────────────────────────────────────────  │
│  ITriggerContext (Type, Phase, Actor, Target, Data)     │
│  IDamageContext (BaseValue, CurrentValue, Multiplier)   │
│  IMoveContext (From, To, DidMove)                       │
│  ITurnContext (TurnNumber, Team)                        │
│  IKillContext (Victim, Killer)                          │
│  IBattleContext                                         │
│  IRewardContext (RewardId)                              │
│  IRunContext (StageId)                                  │
└─────────────────────────────────────────────────────────┘
                          ▲
                          │ implements
                          │
┌─────────────────────────────────────────────────────────┐
│  Gameplay.Combat.Contexts (concrete implementations)    │
│  ─────────────────────────────────────────────────────  │
│  BeforeHitContext : IDamageContext                      │
│  AfterHitContext                                        │
│  MoveContext : IMoveContext                             │
│  TurnContext : ITurnContext                             │
└─────────────────────────────────────────────────────────┘
```

### Extension Methods

```csharp
public static class TriggerContextExtensions
{
    public static bool TryGetData<T>(this ITriggerContext ctx, out T? data)
    {
        data = ctx.Data as T;
        return data != null;
    }
}
```

---

## TriggerExecutor

### Caching System

```csharp
// Cache key: (TriggerType, TriggerPhase)
private readonly Dictionary<(TriggerType, TriggerPhase), List<TriggerEntry>> _triggerMap;

// Cache is rebuilt when:
// 1. Trigger registered/unregistered
// 2. First Execute() call
// 3. InvalidateCache() called
```

### Execution Order

```csharp
foreach (TriggerEntry entry in list)
{
    // 1. Check recursion
    if (_executingTriggers.Contains(triggerId))
    {
        _logger.Warning($"Recursive trigger detected: {triggerId}");
        continue;
    }

    // 2. Check Matches()
    if (entry.Trigger.Matches(context))
    {
        // 3. Call typed Execute
        TriggerResult result = ExecuteTyped(entry.Trigger, context);
        
        // 4. Handle flow control
        switch (result)
        {
            case TriggerResult.Cancel:
                finalResult = TriggerResult.Cancel;
                goto end;
            case TriggerResult.Stop:
                finalResult = TriggerResult.Stop;
                goto end;
            // ...
        }
    }
}
```

### Type-Safe Dispatch

```csharp
private static TriggerResult ExecuteTyped(ITrigger trigger, TriggerContext context)
{
    return trigger switch
    {
        IOnBeforeHit t => t.HandleBeforeHit(context),
        IOnAfterHit t => t.HandleAfterHit(context),
        IOnMove t => t.HandleMove(context),
        IOnTurnStart t => t.HandleTurnStart(context),
        // ... all 17 interfaces
        _ => trigger.Execute(context)
    };
}
```

---

## TriggerService

### Registration

```csharp
public void Register(ITrigger trigger)
{
    lock (_lock)
    {
        if (!_triggers.Contains(trigger))
        {
            _triggers.Add(trigger);
            _isDirty = true;
            _matchesCache.Clear();  // Invalidate cache
        }
    }
}
```

### Execution

```csharp
public TriggerResult Execute(TriggerType type, TriggerPhase phase, TriggerContext context)
{
    List<ITrigger> triggers;
    var cacheKey = (type, phase);

    lock (_lock)
    {
        // Try cache first
        if (!_matchesCache.TryGetValue(cacheKey, out triggers))
        {
            // Cache miss — filter and cache
            triggers = _triggers.Where(t => t.Matches(context))
                               .OrderBy(t => t.Priority)
                               .ToList();
            _matchesCache[cacheKey] = triggers;
        }
    }

    // Execute via TriggerExecutor
    return _executor.Execute(type, phase, context);
}
```

---

## Figure Integration

### ITriggerEntity

```csharp
public interface ITriggerEntity
{
    string TriggerId { get; }  // String ID for triggers
    int EntityId { get; }      // Int ID for visual systems
}
```

### Figure Implementation

```csharp
public class Figure : Entity, ITriggerEntity
{
    // ITriggerEntity implementation
    string ITriggerEntity.Id => TriggerId;
    public string TriggerId => base.Id.ToString();
    public int EntityId => base.Id;

    // Passive management
    public List<IPassive> BasePassives { get; } = new();

    public void AddPassive(IPassive passive)
    {
        if (BasePassives.Any(p => p.GetType() == passive.GetType()))
            return;  // Prevent duplicates

        BasePassives.Add(passive);
        _triggerService?.Register(passive);
    }

    public void RemovePassive(IPassive passive)
    {
        if (BasePassives.Remove(passive))
        {
            _triggerService?.Unregister(passive);
        }
    }
}
```

---

## Status Effects Integration

### Registration Flow

```csharp
public sealed class StatusEffectSystem
{
    public void AddOrStack(IStatusEffect effect)
    {
        _effects[effect.Id] = effect;
        effect.OnApply(_owner);
        _triggerService.Register(effect);  // Register as trigger
    }

    public void Remove(string id)
    {
        if (_effects.TryGetValue(id, out var effect))
        {
            _triggerService.Unregister(effect);  // Unregister
            effect.OnRemove(_owner);
            _effects.Remove(id);
        }
    }
}
```

### Auto-Cleanup

```csharp
public class StatusEffectBase : IStatusEffect
{
    public virtual bool Matches(TriggerContext context)
    {
        // Auto-remove expired effects
        return !IsExpired;
    }
}
```

---

## Next Steps

- [Pipeline and Phases](03_Pipeline.md)
- [Context Types](04_Contexts.md)
- [Best Practices](05_BestPractices.md)
