# Trigger System Overview

## What is a Trigger?

A **Trigger** is an event-driven system that allows passive abilities, artifacts, and status effects to react to game events without modifying core game logic.

## Core Concepts

### 1. Trigger Types

| Type | When It Fires |
|------|---------------|
| `OnBeforeHit` | Before damage is calculated |
| `OnAfterHit` | After damage is dealt |
| `OnTurnStart` | When a turn begins |
| `OnTurnEnd` | When a turn ends |
| `OnMove` | After a figure moves |
| `OnBattleStart` | When combat begins |
| `OnBattleEnd` | When combat ends |
| `OnUnitKill` | When a unit is killed |
| `OnUnitDeath` | When a unit dies |

### 2. Trigger Phases

Phases control **when** within an event type a trigger fires:

```
OnBeforeHit Pipeline:
┌─────────────────┐
│ BeforeCalculation │ ← Damage calculation (Desperation, Swarm)
├─────────────────┤
│ BeforeHit         │ ← Modifiers (Critical, Execute)
├─────────────────┤
│ AfterApplication  │ ← Final modifications
└─────────────────┘
```

### 3. Trigger Groups

Groups control **order** within a phase:

```
Phase: BeforeCalculation
┌──────────────────────────┐
│ Additive    (+5 damage)  │ ← First
├──────────────────────────┤
│ Multiplicative (x2 dmg)  │ ← Second
├──────────────────────────┤
│ Reduction   (-3 damage)  │ ← Third
├──────────────────────────┤
│ Final       (cap at 999) │ ← Last
└──────────────────────────┘
```

### 4. Trigger Results

| Result | Effect |
|--------|--------|
| `Continue` | Continue processing other triggers |
| `Stop` | Stop processing, apply event normally |
| `Cancel` | Cancel the event entirely |
| `Replace` | Continue with modified context |

---

## Architecture

```
┌─────────────────────────────────────────────────────────┐
│                    TriggerService                       │
│  - Register(ITrigger)                                   │
│  - Unregister(ITrigger)                                 │
│  - Execute(TriggerType, TriggerContext)                 │
└────────────────────┬────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────┐
│                   TriggerExecutor                       │
│  - Caches triggers by (Type, Phase)                     │
│  - Sorts by Priority → Group → RegistrationOrder        │
│  - Calls ExecuteTyped() for type-safe dispatch          │
└────────────────────┬────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────┐
│              ITrigger Implementations                   │
│  - Passives (CriticalPassive, ExecutePassive, etc.)     │
│  - Artifacts (WornCrownArtifact, etc.)                  │
│  - Status Effects (DodgeEffect, FuryEffect, etc.)       │
└─────────────────────────────────────────────────────────┘
```

---

## Key Components

### ITrigger

Base interface for all triggers:

```csharp
public interface ITrigger
{
    int Priority { get; }
    TriggerGroup Group { get; }
    TriggerPhase Phase { get; }
    bool Matches(TriggerContext context);
    TriggerResult Execute(TriggerContext context);
}
```

### IOn* Interfaces

Type-safe interfaces for each trigger type:

```csharp
public interface IOnBeforeHit : ITrigger
{
    TriggerResult HandleBeforeHit(IDamageContext context);
}

public interface IOnMove : ITrigger
{
    TriggerResult HandleMove(IMoveContext context);
}
```

### TriggerContext

Carries data through the trigger pipeline:

```csharp
public sealed class TriggerContext : 
    ITriggerContext, IDamageContext, IMoveContext, 
    ITurnContext, IKillContext, IBattleContext, 
    IRewardContext, IRunContext
{
    // Core properties
    public TriggerType Type { get; internal set; }
    public TriggerPhase Phase { get; internal set; }
    public ITriggerEntity? Actor { get; internal set; }
    public ITriggerEntity? Target { get; internal set; }
    
    // Damage properties (IDamageContext)
    public float BaseValue { get; internal set; }
    public float CurrentValue { get; set; }
    public float DamageMultiplier { get; set; } = 1f;
    public float BonusDamage { get; set; }
    public bool IsCritical { get; set; }
    public bool IsDodged { get; set; }
    public bool IsCancelled { get; set; }
    
    // Move properties (IMoveContext)
    public GridPosition From { get; internal set; }
    public GridPosition To { get; internal set; }
    public bool DidMove { get; internal set; }
    
    // Turn properties (ITurnContext)
    public int TurnNumber { get; internal set; }
    public Team Team { get; internal set; }
}
```

---

## Execution Flow

```
1. Game Event Occurs (e.g., figure attacks)
         │
         ▼
2. TriggerService.Execute() called
         │
         ▼
3. TriggerExecutor rebuilds cache (if dirty)
         │
         ▼
4. Filters triggers by Matches()
         │
         ▼
5. Sorts by Priority → Group → RegistrationOrder
         │
         ▼
6. Calls ExecuteTyped() for each trigger
         │
         ▼
7. Handle*Event() method invoked
         │
         ▼
8. TriggerResult determines flow
         │
         ├─ Continue → Next trigger
         ├─ Stop → Stop processing, apply event
         ├─ Cancel → Cancel event entirely
         └─ Replace → Continue with modified context
```

---

## Next Steps

- [Architecture Details](02_Architecture.md)
- [Pipeline and Phases](03_Pipeline.md)
- [Context Types](04_Contexts.md)
- [Best Practices](05_BestPractices.md)
