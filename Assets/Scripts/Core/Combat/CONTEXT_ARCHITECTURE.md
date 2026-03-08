# Context Architecture

## Overview

The context system is split into two layers:

### Core Layer (`Project.Core.Core.Combat.Contexts`)

**Purpose:** Abstraction layer for trigger system and core logic.

**Interfaces:**
- `IDamageContext` - Damage-related data (attacker, target, damage values)
- `IKillContext` - Kill event data
- `IDeathContext` - Death event data  
- `IMoveContext` - Movement data
- `ITurnContext` - Turn data

**Characteristics:**
- Uses `object` references instead of `Figure`
- No dependency on Gameplay layer
- Can be used in pure Core systems (triggers, configs)

### Gameplay Layer (`Project.Gameplay.Gameplay.Combat.Contexts`)

**Purpose:** Concrete implementations with full type safety.

**Classes:**
- `BeforeHitContext : IDamageContext` - Before hit resolution
- `AfterHitContext` - After hit resolution
- `MoveContext : IMoveContext` - Movement events
- `TurnContext : ITurnContext` - Turn events

**Characteristics:**
- Uses `Figure`, `BoardGrid`, `Team` types
- Full access to game state
- Implements Core interfaces for compatibility

---

## Usage Patterns

### In Core Systems (Triggers)

```csharp
public void ProcessDamage(IDamageContext context)
{
    // Works with any damage context
    var multiplier = context.DamageMultiplier;
    var attacker = context.GetAttacker(); // Returns Figure?
}
```

### In Gameplay Systems

```csharp
public void ProcessBeforeHit(BeforeHitContext context)
{
    // Full type safety
    Figure attacker = context.Attacker;
    BoardGrid grid = context.Grid;
    
    // Can be passed to Core systems
    _triggerService.Execute(TriggerType.OnBeforeHit, context);
}
```

### Converting Between Layers

```csharp
// Gameplay → Core (automatic via interface)
IDamageContext core = gameplayContext;

// Core → Gameplay (safe cast, in Gameplay layer)
using Project.Gameplay.Gameplay.Combat.Contexts;

var gameplay = core.AsBeforeHit();
if (gameplay != null)
{
    // Now has full Figure access
}
```

---

## Creating Contexts

### In Gameplay Code

```csharp
var context = new BeforeHitContext
{
    Attacker = attacker,
    Target = target,
    Grid = grid,
    BaseDamage = 10f
};
```

### In Core Code

```csharp
var context = ContextExtensions.CreateDamageContext(
    attacker: someObject,
    target: otherObject,
    baseDamage: 10f,
    isCritical: true
);
```

---

## Best Practices

1. **Use interfaces in Core** - Accept `IDamageContext`, not `BeforeHitContext`
2. **Use concrete types in Gameplay** - Accept `BeforeHitContext` for full access
3. **Safe casting** - Use `AsBeforeHit()`, `AsMove()`, etc. for Core→Gameplay
4. **No circular dependencies** - Core cannot reference Gameplay types directly

---

## Migration Guide

### Old Code (Before Refactoring)

```csharp
// Core layer - using concrete Gameplay types ❌
public void Process(BeforeHitContext context)
{
    // This creates Core→Gameplay dependency
}
```

### New Code (After Refactoring)

```csharp
// Core layer - using interfaces ✅
public void Process(IDamageContext context)
{
    // No dependency on Gameplay layer
    var attacker = context.GetAttacker(); // Returns Figure?
}

// Gameplay layer - implements interface ✅
public sealed class BeforeHitContext : IDamageContext
{
    public Figure Attacker { get; set; }
    object IDamageContext.Attacker => Attacker;
}
```
