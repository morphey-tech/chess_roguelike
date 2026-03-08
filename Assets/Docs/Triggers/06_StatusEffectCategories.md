# Status Effect Categories

## Overview

Status effects are categorized by their impact on the figure. This enables:
- Mass removal (Cleanse, Dispel)
- Immunity checks
- AI targeting priorities
- Visual filtering

---

## Effect Categories

| Category | Description | Examples |
|----------|-------------|----------|
| `Buff` | Positive effects that benefit the figure | Fury, Inspiration, Royal Presence, Dodge |
| `Debuff` | Negative effects that harm the figure | Poison, Burn, Vulnerability, Weakness |
| `CrowdControl` | Effects that limit figure actions | Stun, Silence, Root, Disarm |
| `Neutral` | Effects without clear positive/negative impact | Marks, Triggers, Auras |

---

## Implementation

### Creating a Categorized Effect

```csharp
// Buff example
public sealed class FuryEffect : StackableStatusEffect
{
    public override string Id => "fury";
    public override EffectCategory Category => EffectCategory.Buff;
    
    // ...
}

// Debuff example (to be added)
public sealed class PoisonEffect : StatusEffectBase
{
    public override string Id => "poison";
    public override EffectCategory Category => EffectCategory.Debuff;
    
    // ...
}

// Crowd Control example (to be added)
public sealed class StunEffect : StatusEffectBase
{
    public override string Id => "stun";
    public override EffectCategory Category => EffectCategory.CrowdControl;
    
    // ...
}
```

---

## Extension Methods

### Filtering

```csharp
// Get all buffs
var buffs = figure.Effects.GetBuffs();

// Get all debuffs
var debuffs = figure.Effects.GetDebuffs();

// Get crowd control
var cc = figure.Effects.GetCrowdControl();

// Check for specific category
bool hasBuff = figure.Effects.HasEffectOfType(EffectCategory.Buff);
bool hasCC = figure.Effects.HasCrowdControl();

// Count by category
int buffCount = figure.Effects.CountByCategory(EffectCategory.Buff);
```

### Mass Removal

```csharp
// Remove all buffs (e.g., for "Purge")
figure.Effects.RemoveAllBuffs();

// Remove all debuffs (e.g., for "Cleanse")
figure.Effects.RemoveAllDebuffs();

// Remove all crowd control (e.g., for "Freedom")
figure.Effects.RemoveAllCrowdControl();
```

---

## Use Cases

### 1. Cleanse Ability

```csharp
public sealed class CleansePassive : IPassive, IOnTurnStart
{
    public TriggerResult HandleTurnStart(ITurnContext context)
    {
        if (context.Actor is Figure figure)
        {
            // Remove all debuffs at start of turn
            figure.Effects.RemoveAllDebuffs();
        }
        return TriggerResult.Continue;
    }
}
```

### 2. Immunity Check

```csharp
public sealed class CrowdControlImmunityEffect : StatusEffectBase
{
    public override string Id => "cc_immunity";
    public override EffectCategory Category => EffectCategory.Buff;

    public override void OnApply(Figure owner)
    {
        // Mark as immune to crowd control
        owner.SetFlag(FigureFlags.CCImmune, true);
    }

    public override void OnRemove(Figure owner)
    {
        owner.SetFlag(FigureFlags.CCImmune, false);
    }
}

// In CC application logic
public bool CanApplyCrowdControl(Figure target)
{
    if (target.HasFlag(FigureFlags.CCImmune))
        return false;
    
    return !target.Effects.HasCrowdControl(); // Optional: no double CC
}
```

### 3. AI Priority Targeting

```csharp
public Figure SelectTarget(IEnumerable<Figure> enemies)
{
    // Priority 1: Enemies with crowd control (kill them first)
    var ccTargets = enemies.Where(e => e.Effects.HasCrowdControl());
    if (ccTargets.Any())
        return ccTargets.First();

    // Priority 2: Enemies with buffs (dispel them)
    var buffTargets = enemies.Where(e => e.Effects.CountByCategory(EffectCategory.Buff) > 0);
    if (buffTargets.Any())
        return buffTargets.First();

    // Default: lowest HP
    return enemies.OrderBy(e => e.Stats.CurrentHp.Value).First();
}
```

### 4. Visual Filtering

```csharp
// In UI: Show only debuffs on enemy
var enemyDebuffs = enemy.Effects.GetDebuffs();
foreach (var debuff in enemyDebuffs)
{
    ShowDebuffIcon(debuff.Id);
}

// In UI: Show only buffs on ally
var allyBuffs = ally.Effects.GetBuffs();
foreach (var buff in allyBuffs)
{
    ShowBuffIcon(buff.Id);
}
```

---

## Current Effects by Category

### Buffs
| Effect | Description |
|--------|-------------|
| `FuryEffect` | +Attack damage per stack |
| `DodgeEffect` | Chance to dodge attacks |
| `InspirationBuffEffect` | +Attack/Defence/Evasion |
| `RoyalPresenceBuffEffect` | +Attack damage (aura) |

### Debuffs
| Effect | Description | Status |
|--------|-------------|--------|
| *To be added* | Poison, Burn, etc. | TODO |

### Crowd Control
| Effect | Description | Status |
|--------|-------------|--------|
| *To be added* | Stun, Silence, etc. | TODO |

### Neutral
| Effect | Description | Status |
|--------|-------------|--------|
| *To be added* | Marks, triggers | TODO |

---

## Future Enhancements

1. **Effect Categories as Flags**
   - Allow effects to have multiple categories (e.g., Buff + CrowdControl for "Berserk")
   - Change `EffectCategory` from enum to `[Flags]` enum

2. **Category-Based Interactions**
   - "Double effect duration for buffs"
   - "Immune to all debuffs"
   - "Convert debuffs to buffs"

3. **Synergy System**
   - "For each buff on this figure, +1 Attack"
   - "For each debuff on target, +5% crit chance"

---

## Related Documentation

- [Status Effect System](../Architecture/01_Overview.md#status-effect-system)
- [Trigger System](../Triggers/01_Overview.md)
