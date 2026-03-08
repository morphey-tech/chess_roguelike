# Trigger Pipeline

## Execution Order

Triggers execute in a specific order based on three factors:

```
1. Phase (TriggerPhase)
   └─ 2. Group (TriggerGroup)
      └─ 3. Priority (int)
         └─ 4. Registration Order
```

---

## Phase System

### Damage Pipeline (OnBeforeHit)

```
┌─────────────────────────────────────────────────────────┐
│ BeforeCalculation (Phase = 1)                           │
│ ─────────────────────────────────────────────────────── │
│ Purpose: Base damage calculation                        │
│ Examples: Desperation (set ATK to 1), Swarm (+X per ally)│
│ Group: Additive → Multiplicative → Reduction → Final    │
└─────────────────────────────────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────────┐
│ BeforeHit (Phase = 13)                                  │
│ ─────────────────────────────────────────────────────── │
│ Purpose: Hit modifiers                                  │
│ Examples: Critical (2x dmg), Execute (1.5x vs low HP)   │
│ Group: Default, Multiplicative                          │
└─────────────────────────────────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────────┐
│ AfterApplication (Phase = 4)                            │
│ ─────────────────────────────────────────────────────── │
│ Purpose: Post-damage effects                            │
│ Examples: Lifesteal, Thorns                             │
│ Group: Default                                          │
└─────────────────────────────────────────────────────────┘
```

### Turn Pipeline (OnTurnStart)

```
┌─────────────────────────────────────────────────────────┐
│ BeforeTurn (Phase = 30)                                 │
│ OnTurnStart (Phase = 31)  ← Inspiration triggers here   │
│ DuringTurn (Phase = 32)                                 │
│ OnTurnEnd (Phase = 33)                                  │
│ AfterTurn (Phase = 34)                                  │
└─────────────────────────────────────────────────────────┘
```

### Movement Pipeline (OnMove)

```
┌─────────────────────────────────────────────────────────┐
│ BeforeMove (Phase = 40)                                 │
│ DuringMove (Phase = 41)                                 │
│ AfterMove (Phase = 42)  ← Momentum, Royal Presence      │
└─────────────────────────────────────────────────────────┘
```

---

## Group System

Groups control order **within** a phase:

```
Phase: BeforeCalculation
┌──────────────────────────────────────────────────────────┐
│ Additive (Group = 1)                                     │
│ ──────────────────────────────────────────────────────── │
│ Flat bonuses: +5 damage, +10 armor                       │
│ Examples: Swarm (+2 per ally), FirstShot (+5 damage)     │
└──────────────────────────────────────────────────────────┘
                          │
                          ▼
┌──────────────────────────────────────────────────────────┐
│ Multiplicative (Group = 2)                               │
│ ──────────────────────────────────────────────────────── │
│ Percent bonuses: x2 damage, x1.5 armor                   │
│ Examples: Critical (2x), Execute (1.5x vs low HP)        │
└──────────────────────────────────────────────────────────┘
                          │
                          ▼
┌──────────────────────────────────────────────────────────┐
│ Reduction (Group = 3)                                    │
│ ──────────────────────────────────────────────────────── │
│ Flat/percent reduction: -3 damage, -50% damage taken     │
│ Examples: Armour reduction, resistance                   │
└──────────────────────────────────────────────────────────┘
                          │
                          ▼
┌──────────────────────────────────────────────────────────┐
│ Final (Group = 4)                                        │
│ ──────────────────────────────────────────────────────── │
│ Cap/clamp: min 1 damage, max 999 damage                  │
│ Examples: Damage floor/ceiling                           │
└──────────────────────────────────────────────────────────┘
```

---

## Priority System

Lower priority = executes first:

```csharp
public static class TriggerPriorities
{
    public const int VeryHigh = -100;
    public const int High = -10;
    public const int Normal = 0;
    public const int Low = 10;
    public const int VeryLow = 100;
}
```

### Example: Desperation vs Swarm

```csharp
// Desperation executes FIRST (sets ATK to 1)
public sealed class DesperationPassive : IPassive, IOnBeforeHit
{
    public int Priority => TriggerPriorities.High;  // -10
    public TriggerGroup Group => TriggerGroup.First;
    public TriggerPhase Phase => TriggerPhase.BeforeCalculation;
}

// Swarm executes SECOND (adds +X per ally)
public sealed class SwarmPassive : IPassive, IOnBeforeHit
{
    public int Priority => TriggerPriorities.Normal;  // 0
    public TriggerGroup Group => TriggerGroup.Additive;
    public TriggerPhase Phase => TriggerPhase.BeforeCalculation;
}
```

**Result:** If no allies nearby, Desperation sets ATK=1, Swarm adds +0.

---

## Complete Execution Example

### Scenario: Critical + Execute vs Low HP Target

```
Event: Figure attacks (10 base damage)
Target: 20% HP (triggers Execute)
Attacker: Has Critical (100% crit chance, 2x) and Execute (1.5x vs <30% HP)

Execution Order:
┌─────────────────────────────────────────────────────────┐
│ 1. BeforeCalculation Phase                              │
│    - No triggers match                                  │
└─────────────────────────────────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────────┐
│ 2. BeforeHit Phase                                      │
│    a. CriticalPassive.HandleBeforeHit()                 │
│       → DamageMultiplier = 1.0 * 2.0 = 2.0              │
│       → IsCritical = true                               │
│    b. ExecutePassive.HandleBeforeHit()                  │
│       → DamageMultiplier = 2.0 * 1.5 = 3.0              │
└─────────────────────────────────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────────┐
│ 3. AfterHit Phase                                       │
│    - No triggers match                                  │
└─────────────────────────────────────────────────────────┘

Final Damage: 10 * 3.0 = 30 damage
```

---

## Flow Control

### TriggerResult Options

| Result | Effect | Example |
|--------|--------|---------|
| `Continue` | Continue processing | Most passives |
| `Stop` | Stop processing, apply event | "Last stand" abilities |
| `Cancel` | Cancel event entirely | Dodge, shield |
| `Replace` | Continue with modified context | Damage redirection |

### Example: Dodge Cancels Hit

```csharp
public class DodgeEffect : StatusEffectBase, IOnBeforeHit
{
    public TriggerResult HandleBeforeHit(IDamageContext context)
    {
        if (!_random.Chance(_dodgeChance))
            return TriggerResult.Continue;

        context.IsDodged = true;
        context.IsCancelled = true;
        return TriggerResult.Cancel;  // Cancel the hit
    }
}
```

---

## Debugging

### Enable Trace Logging

```csharp
// In TriggerContext
public string GetTraceString()
{
    if (_traceLog == null || _traceLog.Count == 0)
        return $"BaseValue = {BaseValue} (no modifications)";

    StringBuilder sb = new();
    sb.AppendLine($"BaseValue = {BaseValue}");

    foreach (var record in _traceLog)
    {
        string operation = record.IsSet ? "→" :
                          !Mathf.Approximately(record.Multiplier, 1f) ? $"x{record.Multiplier} →" :
                          record.Delta >= 0 ? $"+{record.Delta} →" : $"{record.Delta} →";

        sb.AppendLine($"{record.Source,-25} {operation} {record.NewValue}");
    }

    return sb.ToString().TrimEnd();
}
```

### Example Output

```
BaseValue = 10
CriticalPassive             x2 → 20
ExecutePassive              x1.5 → 30
SwarmPassive                +4 → 34
```

---

## Next Steps

- [Context Types](04_Contexts.md)
- [Best Practices](05_BestPractices.md)
