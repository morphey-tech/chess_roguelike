# Best Practices

## Creating a Passive

### Step 1: Choose the Right Interface

| When to Use | Interface |
|-------------|-----------|
| Before damage calculation | `IOnBeforeHit` + `Phase.BeforeCalculation` |
| Critical hit, multiplier | `IOnBeforeHit` + `Phase.BeforeHit` |
| After damage dealt | `IOnBeforeHit` + `Phase.AfterHit` |
| Lifesteal, thorns | `IOnAfterHit` |
| Start of turn | `IOnTurnStart` |
| After movement | `IOnMove` |

### Step 2: Implement the Pattern

```csharp
using Project.Core.Core.Triggers;
using Project.Gameplay.Gameplay.Combat.Contexts;

namespace Project.Gameplay.Gameplay.Combat.Passives
{
    public sealed class MyPassive : IPassive, IOnBeforeHit
    {
        // === Identity ===
        public string Id { get; }
        public int Priority => TriggerPriorities.Normal;
        public TriggerGroup Group => TriggerGroup.Default;
        public TriggerPhase Phase => TriggerPhase.BeforeHit;

        // === Constructor ===
        public MyPassive(string id)
        {
            Id = id;
        }

        // === Filter ===
        public bool Matches(TriggerContext context)
        {
            if (context.Type != TriggerType.OnBeforeHit)
                return false;
            if (context.Actor == null)
                return false;
            return true;
        }

        // === Wrapper (from ITrigger) ===
        public TriggerResult Execute(TriggerContext context)
        {
            if (context is not IDamageContext dc)
                return TriggerResult.Continue;
            return HandleBeforeHit(dc);
        }

        // === Main Logic (from IOnBeforeHit) ===
        public TriggerResult HandleBeforeHit(IDamageContext context)
        {
            context.DamageMultiplier *= 1.5f;
            return TriggerResult.Continue;
        }
    }
}
```

---

## Code Style

### Braces Always

```csharp
// ✅ Good
if (condition)
{
    return false;
}

// ❌ Bad
if (condition) return false;
```

### Explicit Types (No `var`)

```csharp
// ✅ Good
Figure attacker = context.Attacker as Figure;
List<ITrigger> triggers = _triggers.ToList();

// ❌ Bad
var attacker = context.Attacker as Figure;
var triggers = _triggers.ToList();
```

### Handle* Naming

```csharp
// ✅ Good
public TriggerResult HandleBeforeHit(IDamageContext context);
public TriggerResult HandleMove(IMoveContext context);

// ❌ Bad
public TriggerResult Execute(IDamageContext context);
```

---

## Common Patterns

### Damage Multiplier

```csharp
public TriggerResult HandleBeforeHit(IDamageContext context)
{
    // Check condition
    if (target.HPPercent <= 0.3f)
    {
        context.DamageMultiplier *= 1.5f;
    }
    return TriggerResult.Continue;
}
```

### Bonus Damage

```csharp
public TriggerResult HandleBeforeHit(IDamageContext context)
{
    // Flat bonus
    context.BonusDamage += 5;
    return TriggerResult.Continue;
}
```

### Critical Hit

```csharp
public TriggerResult HandleBeforeHit(IDamageContext context)
{
    if (_random.Chance(_critChance))
    {
        context.DamageMultiplier *= _critMultiplier;
        context.IsCritical = true;
    }
    return TriggerResult.Continue;
}
```

### Cancel Event (Dodge)

```csharp
public TriggerResult HandleBeforeHit(IDamageContext context)
{
    if (_random.Chance(_dodgeChance))
    {
        context.IsDodged = true;
        context.IsCancelled = true;
        return TriggerResult.Cancel;
    }
    return TriggerResult.Continue;
}
```

### After Hit Effect

```csharp
public TriggerResult HandleAfterHit(IDamageContext context)
{
    // Lifesteal
    int heal = (int)(context.CurrentValue * 0.3f);
    context.Actor.Stats.Heal(heal);
    return TriggerResult.Continue;
}
```

### Move Effect

```csharp
public TriggerResult HandleMove(IMoveContext context, TriggerContext ctx)
{
    if (!ctx.TryGetData<MoveContext>(out var move))
        return TriggerResult.Continue;

    // Apply movement-based buff
    move.Actor.AddBuff("momentum", damageBonus: move.Distance);
    return TriggerResult.Continue;
}
```

---

## Priority Guidelines

| Priority | When to Use | Examples |
|----------|-------------|----------|
| `High` (-10) | Must execute before others | Desperation (sets base damage) |
| `Normal` (0) | Default | Most passives |
| `Low` (10) | Should execute after others | Final modifiers |

### Group Guidelines

| Group | When to Use | Examples |
|-------|-------------|----------|
| `Additive` | Flat bonuses | Swarm (+2 per ally) |
| `Multiplicative` | Percent bonuses | Critical (2x), Execute (1.5x) |
| `Default` | No specific order | Most passives |

---

## Testing

### Unit Test Pattern

```csharp
[Test]
public void Critical_LowHpTarget_MultiplierApplied()
{
    // Arrange
    var attacker = CreateFigure(attack: 10);
    var target = CreateFigure(hp: 30, maxHp: 100);
    attacker.AddPassive(new CriticalPassive("crit", 1f, 2f));

    // Act
    var context = CreateBeforeHitContext(attacker, target, baseDamage: 10);
    ExecuteBeforeHit(context);

    // Assert
    Assert.That(context.DamageMultiplier, Is.EqualTo(2.0f));
    Assert.That(context.IsCritical, Is.True);
}
```

### Test Helpers

```csharp
// In PassiveFixture.cs
protected void ExecuteBeforeHit(BeforeHitContext context)
{
    var triggerContext = new TriggerContextBuilder()
        .WithType(TriggerType.OnBeforeHit)
        .WithPhase(TriggerPhase.BeforeHit)
        .WithActor(context.Attacker)
        .WithTarget(context.Target)
        .WithValue(context.BaseDamage)
        .WithData(context)
        .Build();

    TriggerService.Execute(TriggerType.OnBeforeHit, TriggerPhase.BeforeHit, triggerContext);

    // Copy values back for assertions
    context.DamageMultiplier = triggerContext.DamageMultiplier;
    context.IsCritical = triggerContext.IsCritical;
    context.BonusDamage = triggerContext.BonusDamage;
}
```

---

## Debugging

### Enable Logging

```csharp
// In TriggerExecutor
_logger.Debug($"Trigger {entry.Source} executed, result: {result}");
```

### Trace Modifications

```csharp
// In TriggerContext
public string GetTraceString()
{
    // Shows full pipeline of modifications
    // "BaseValue = 10\nCriticalPassive x2 → 20\nExecutePassive x1.5 → 30"
}
```

### Common Issues

| Issue | Solution |
|-------|----------|
| Passive not firing | Check `Matches()` returns `true` |
| Wrong order | Adjust `Priority` or `Group` |
| Values not modified | Use `context.DamageMultiplier`, not local variable |
| Duplicate passives | Check `AddPassive` prevents duplicates |

---

## Performance

### Caching

The system automatically caches trigger lists:

```csharp
// Cache is rebuilt when:
// 1. Trigger registered/unregistered
// 2. First Execute() call
// 3. InvalidateCache() called
```

### Avoid in Matches()

```csharp
// ✅ Good — fast checks
public bool Matches(TriggerContext context)
{
    if (context.Type != TriggerType.OnBeforeHit) return false;
    if (context.Actor == null) return false;
    return true;
}

// ❌ Bad — expensive operations
public bool Matches(TriggerContext context)
{
    // Don't do heavy computation here
    var allAllies = context.Grid.GetAllFigures().ToList();
    // ...
}
```

---

## Next Steps

- [Architecture Overview](../Architecture/01_Overview.md)
- [Context Architecture](../Architecture/02_Contexts.md)
