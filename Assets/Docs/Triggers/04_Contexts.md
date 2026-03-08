# Context Types

## Overview

Contexts carry data through the trigger pipeline. Each context type implements specific interfaces for type-safe access.

---

## Context Hierarchy

```
ITriggerContext (base)
    │
    ├─ IDamageContext      → OnBeforeHit, OnAfterHit, OnDamageReceived
    ├─ IMoveContext        → OnMove
    ├─ ITurnContext        → OnTurnStart, OnTurnEnd
    ├─ IKillContext        → OnUnitKill, OnUnitDeath, OnAllyDeath
    ├─ IBattleContext      → OnBattleStart, OnBattleEnd
    ├─ IRewardContext      → OnReward
    └─ IRunContext         → OnRunStart, OnStageEnter, OnStageLeave
```

---

## ITriggerContext (Base)

```csharp
public interface ITriggerContext
{
    TriggerType Type { get; }
    TriggerPhase Phase { get; }
    ITriggerEntity? Actor { get; }
    ITriggerEntity? Target { get; }
    object? Data { get; }
}
```

### Usage

```csharp
public bool Matches(TriggerContext context)
{
    if (context.Type != TriggerType.OnBeforeHit)
        return false;
    if (context.Actor == null)
        return false;
    return true;
}
```

---

## IDamageContext

Used for damage-related triggers.

```csharp
public interface IDamageContext : ITriggerContext
{
    float BaseValue { get; }         // Original damage
    float CurrentValue { get; set; } // Modified damage
    float DamageMultiplier { get; set; }
    float BonusDamage { get; set; }
    bool IsCritical { get; set; }
    bool IsDodged { get; set; }
    bool IsCancelled { get; set; }
}
```

### Usage Example

```csharp
public TriggerResult HandleBeforeHit(IDamageContext context)
{
    // Read base damage
    float baseDmg = context.BaseValue;

    // Apply multiplier
    context.DamageMultiplier *= 2f;

    // Add bonus damage
    context.BonusDamage += 5;

    // Mark as critical
    context.IsCritical = true;

    // Cancel hit (dodge)
    if (shouldDodge)
    {
        context.IsDodged = true;
        context.IsCancelled = true;
        return TriggerResult.Cancel;
    }

    return TriggerResult.Continue;
}
```

### Concrete Implementation

```csharp
// Gameplay layer
public sealed class BeforeHitContext : IDamageContext
{
    public Figure Attacker { get; set; }
    public Figure Target { get; set; }
    public BoardGrid Grid { get; set; }
    public float BaseDamage { get; set; }
    public float DamageMultiplier { get; set; } = 1f;
    public float BonusDamage { get; set; }
    public bool IsCritical { get; set; }
    public bool IsDodged { get; set; }
    public bool IsCancelled { get; set; }

    // Explicit interface implementation
    ITriggerEntity? ITriggerContext.Actor => Attacker;
    ITriggerEntity? ITriggerContext.Target => Target;
}
```

---

## IMoveContext

Used for movement triggers.

```csharp
public interface IMoveContext : ITriggerContext
{
    GridPosition From { get; }
    GridPosition To { get; }
    bool DidMove { get; }
}
```

### Usage Example

```csharp
public TriggerResult HandleMove(IMoveContext context)
{
    // Check if figure actually moved
    if (!context.DidMove)
        return TriggerResult.Continue;

    // Calculate distance
    int distance = GetDistance(context.From, context.To);

    // Apply momentum bonus
    if (distance > 0)
    {
        // Bonus damage based on distance
        // (handled via context.Data or direct figure access)
    }

    return TriggerResult.Continue;
}
```

---

## ITurnContext

Used for turn triggers.

```csharp
public interface ITurnContext : ITriggerContext
{
    int TurnNumber { get; }
    Team Team { get; }
}
```

### Usage Example

```csharp
public TriggerResult HandleTurnStart(ITurnContext context)
{
    // Check if it's this figure's turn
    if (context.Actor.Team != context.Team)
        return TriggerResult.Continue;

    // Apply start-of-turn effects
    ApplyBuff(context.Actor, "haste", duration: 1);

    return TriggerResult.Continue;
}
```

---

## IKillContext

Used for kill/death triggers.

```csharp
public interface IKillContext : ITriggerContext
{
    ITriggerEntity? Victim { get; }
    ITriggerEntity? Killer { get; }
}
```

### Usage Example

```csharp
public TriggerResult HandleUnitKill(IKillContext context)
{
    // Killer gets bonus
    if (context.Killer is Figure killer)
    {
        killer.AddBuff("victory", duration: 1);
    }

    return TriggerResult.Continue;
}
```

---

## Data Property

For additional context-specific data:

```csharp
// In TriggerContext
public object? Data { get; set; }

// Extension method
public static bool TryGetData<T>(this ITriggerContext ctx, out T? data)
{
    data = ctx.Data as T;
    return data != null;
}
```

### Usage

```csharp
public bool Matches(TriggerContext context)
{
    if (!context.TryGetData<BeforeHitContext>(out var beforeHit))
        return false;

    // Access gameplay-specific data
    return beforeHit.Grid != null;
}

public TriggerResult HandleBeforeHit(IDamageContext context, TriggerContext ctx)
{
    if (!ctx.TryGetData<BeforeHitContext>(out var beforeHit))
        return TriggerResult.Continue;

    // Use gameplay data
    beforeHit.Attacker.Stats.Attack.AddModifier(...);

    return TriggerResult.Continue;
}
```

---

## TriggerContextBuilder

Fluent builder for creating contexts:

```csharp
var context = new TriggerContextBuilder()
    .WithType(TriggerType.OnBeforeHit)
    .WithPhase(TriggerPhase.BeforeHit)
    .WithActor(attacker)
    .WithTarget(target)
    .WithValue(10f)
    .WithDamageMultiplier(1.5f)
    .WithBonusDamage(5)
    .WithCritical(true)
    .WithData(beforeHitContext)
    .Build();
```

### Specialized Builders

```csharp
// Damage context
public TriggerContextBuilder WithDamageMultiplier(float multiplier);
public TriggerContextBuilder WithBonusDamage(float bonus);
public TriggerContextBuilder WithCritical(bool isCritical);
public TriggerContextBuilder WithDodged(bool isDodged);
public TriggerContextBuilder WithCancelled(bool isCancelled);

// Move context
public TriggerContextBuilder WithMove(GridPosition from, GridPosition to, bool didMove = true);

// Turn context
public TriggerContextBuilder WithTurn(int turnNumber, Team team);

// Reward context
public TriggerContextBuilder WithReward(string? rewardId);

// Run context
public TriggerContextBuilder WithStage(string? stageId);
```

---

## Next Steps

- [Best Practices](05_BestPractices.md)
