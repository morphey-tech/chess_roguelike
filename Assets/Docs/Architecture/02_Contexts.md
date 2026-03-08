# Context Architecture

## Three-Layer Design

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
│                                                         │
│  TriggerContext : implements all interfaces             │
└────────────────────┬────────────────────────────────────┘
                     │ implements
                     ▼
┌─────────────────────────────────────────────────────────┐
│  Core.Combat.Contexts (combat-specific)                 │
│  ─────────────────────────────────────────────────────  │
│  IDamageContext (Attacker, Target as object)            │
│  IKillContext, IDeathContext                            │
└────────────────────┬────────────────────────────────────┘
                     │ implements
                     ▼
┌─────────────────────────────────────────────────────────┐
│  Gameplay.Combat.Contexts (concrete)                    │
│  ─────────────────────────────────────────────────────  │
│  BeforeHitContext : IDamageContext                      │
│  AfterHitContext                                        │
│  MoveContext : IMoveContext                             │
│  TurnContext : ITurnContext                             │
└─────────────────────────────────────────────────────────┘
```

---

## Why Three Layers?

### Layer 1: Core.Triggers

**Purpose:** Base interfaces for trigger system.

**Characteristics:**
- No dependency on Gameplay layer
- Uses `ITriggerEntity` instead of `Figure`
- Can be used in pure Core systems

```csharp
public interface IDamageContext : ITriggerContext
{
    float BaseValue { get; }
    float CurrentValue { get; set; }
    float DamageMultiplier { get; set; }
    // ...
}
```

### Layer 2: Core.Combat.Contexts

**Purpose:** Combat-specific abstractions.

**Characteristics:**
- Uses `object` for Attacker/Target (avoids Figure dependency)
- Can be used in Core.Combat systems

```csharp
public interface IDamageContext : ITriggerContext
{
    object Attacker { get; }
    object Target { get; }
    float BaseDamage { get; }
    // ...
}
```

### Layer 3: Gameplay.Combat.Contexts

**Purpose:** Concrete implementations with full type safety.

**Characteristics:**
- Uses `Figure`, `BoardGrid`, `Team` types
- Full access to game state
- Implements Core interfaces for compatibility

```csharp
public sealed class BeforeHitContext : IDamageContext
{
    public Figure Attacker { get; set; }
    public Figure Target { get; set; }
    public BoardGrid Grid { get; set; }
    // ...
}
```

---

## Conversion Between Layers

### Gameplay → Core (Automatic)

```csharp
// BeforeHitContext implements IDamageContext
BeforeHitContext gameplay = new();
IDamageContext core = gameplay;  // ✅ Implicit cast
```

### Core → Gameplay (Safe Cast)

```csharp
// Extension method
public static BeforeHitContext? AsBeforeHit(this IDamageContext ctx)
{
    return ctx as BeforeHitContext;
}

// Usage
IDamageContext core = GetContext();
if (core.AsBeforeHit() is BeforeHitContext gameplay)
{
    // Now has full Figure access
    var attacker = gameplay.Attacker;
}
```

---

## TriggerContext Implementation

```csharp
public sealed class TriggerContext : 
    ITriggerContext, IDamageContext, IMoveContext, 
    ITurnContext, IKillContext, IBattleContext, 
    IRewardContext, IRunContext
{
    // ITriggerContext
    public TriggerType Type { get; internal set; }
    public TriggerPhase Phase { get; internal set; }
    public ITriggerEntity? Actor { get; internal set; }
    public ITriggerEntity? Target { get; internal set; }
    public object? Data { get; set; }

    // IDamageContext
    public float BaseValue { get; internal set; }
    public float CurrentValue { get; set; }
    public float DamageMultiplier { get; set; } = 1f;
    public float BonusDamage { get; set; }
    public bool IsCritical { get; set; }
    public bool IsDodged { get; set; }
    public bool IsCancelled { get; set; }

    // IMoveContext
    public GridPosition From { get; internal set; }
    public GridPosition To { get; internal set; }
    public bool DidMove { get; internal set; }

    // ITurnContext
    public int TurnNumber { get; internal set; }
    public Team Team { get; internal set; }

    // IKillContext (explicit)
    ITriggerEntity? IKillContext.Victim => Target;
    ITriggerEntity? IKillContext.Killer => Actor;

    // IRewardContext
    public string? RewardId { get; internal set; }

    // IRunContext
    public string? StageId { get; internal set; }
}
```

---

## Extension Methods

```csharp
public static class TriggerContextExtensions
{
    public static bool TryGetData<T>(this ITriggerContext ctx, out T? data)
    {
        data = ctx.Data as T;
        return data != null;
    }

    public static BeforeHitContext? AsBeforeHit(this IDamageContext ctx)
    {
        return ctx as BeforeHitContext;
    }

    public static MoveContext? AsMove(this IMoveContext ctx)
    {
        return ctx as MoveContext;
    }

    public static TurnContext? AsTurn(this ITurnContext ctx)
    {
        return ctx as TurnContext;
    }
}
```

---

## Usage Examples

### In Core (Trigger System)

```csharp
public TriggerResult Execute(TriggerContext context)
{
    // Use base interface only
    if (context.Type != TriggerType.OnBeforeHit)
        return TriggerResult.Continue;

    if (context is IDamageContext dc)
    {
        dc.DamageMultiplier *= 2f;
    }

    return TriggerResult.Continue;
}
```

### In Gameplay (Passives)

```csharp
public TriggerResult HandleBeforeHit(IDamageContext context, TriggerContext ctx)
{
    // Use Gameplay-specific data
    if (!ctx.TryGetData<BeforeHitContext>(out var beforeHit))
        return TriggerResult.Continue;

    // Full Figure access
    beforeHit.Attacker.Stats.Attack.AddModifier(...);

    return TriggerResult.Continue;
}
```

---

## Benefits

| Benefit | Description |
|---------|-------------|
| **Separation** | Core doesn't depend on Gameplay |
| **Flexibility** | Can use Core contexts in isolation |
| **Type Safety** | Gameplay layer has full types |
| **Testability** | Core can be tested without Gameplay |
| **Maintainability** | Clear layer boundaries |

---

## Next Steps

- [Trigger Best Practices](../Triggers/05_BestPractices.md)
