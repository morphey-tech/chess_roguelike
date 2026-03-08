# Artifact Synergy System

## Overview

The Artifact Synergy System allows artifacts to interact with each other, creating powerful combinations when multiple specific artifacts are equipped.

---

## How It Works

### 1. Artifact Changed Message

When an artifact is acquired or removed, `ArtifactService` publishes an `ArtifactChangedMessage`:

```csharp
public readonly struct ArtifactChangedMessage
{
    public int OwnerId { get; }
    public string? ArtifactId { get; }
    public ArtifactChangeType ChangeType { get; }
}
```

### 2. UI Subscription

`ArtifactsWindow` subscribes to the message via MessagePipe:

```csharp
_subscription = _artifactChangedSubscriber.Subscribe(OnArtifactChanged);

private void OnArtifactChanged(ArtifactChangedMessage message)
{
    Refresh();  // Rebuild UI
}
```

### 3. Synergy Check

When an artifact changes, `ArtifactService.CheckSynergies()` is called:

```csharp
private void CheckSynergies()
{
    var activeSynergies = _synergyRegistry.GetActiveSynergies(this);
    foreach (var synergyId in activeSynergies)
    {
        _logger.Info($"Synergy activated: {synergyId}");
        ApplySynergyEffect(synergyId);
    }
}
```

---

## Configuration

### Synergy Config

```json
// artifact_synergies_conf.json
{
  "synergies": [
    {
      "id": "fire_mastery",
      "name": "Fire Mastery",
      "description": "+20% fire damage",
      "required_artifacts": ["fire_amulet", "fire_ring"],
      "required_count": 2,
      "effect_id": "fire_damage_bonus"
    },
    {
      "id": "royal_presence",
      "name": "Royal Presence",
      "description": "Allies gain +1 ATK",
      "required_artifacts": ["worn_crown", "royal_scepter"],
      "required_count": 2,
      "effect_id": "royal_aura"
    }
  ]
}
```

### Artifact Config with Tags

```json
// artifacts_conf.json
{
  "artifacts": [
    {
      "id": "fire_amulet",
      "name": "Fire Amulet",
      "max_stack": 1,
      "tags": ["Fire", "Magical"]
    },
    {
      "id": "worn_crown",
      "name": "Worn Crown",
      "max_stack": 1,
      "tags": ["Royal", "Light"]
    }
  ]
}
```

---

## Implementation

### Creating a Synergy

1. **Create synergy config:**

```csharp
var config = new ArtifactSynergyConfig
{
    Id = "fire_mastery",
    Name = "Fire Mastery",
    Description = "+20% fire damage",
    RequiredArtifactIds = new List<string> { "fire_amulet", "fire_ring" },
    RequiredCount = 2,
    EffectId = "fire_damage_bonus"
};

_synergyRegistry.Register(config);
```

2. **Implement synergy effect:**

```csharp
private void ApplySynergyEffect(string synergyId)
{
    var config = _synergyRegistry.Get(synergyId);
    
    // Option 1: Register a passive
    var passive = _passiveFactory.Create(config.EffectId);
    _triggerService.Register(passive);
    
    // Option 2: Apply a buff
    var buff = new SynergyBuff(config.EffectId);
    _figure.Effects.AddOrStack(buff);
    
    // Option 3: Direct stat modification
    _figure.Stats.Attack.AddModifier(...);
}
```

---

## UI Integration

### ArtifactsWindow Refresh

```csharp
public class ArtifactsWindow : ParameterlessWindow
{
    private ISubscriber<ArtifactChangedMessage> _artifactChangedSubscriber = null!;
    private IDisposable? _subscription;

    [Inject]
    private void Construct(
        ArtifactService artifactService,
        ISubscriber<ArtifactChangedMessage> artifactChangedSubscriber)
    {
        _artifactService = artifactService;
        _artifactChangedSubscriber = artifactChangedSubscriber;
    }

    protected override void OnInit()
    {
        _subscription = _artifactChangedSubscriber.Subscribe(OnArtifactChanged);
    }

    private void OnArtifactChanged(ArtifactChangedMessage message)
    {
        Refresh();  // Rebuild artifact list UI
    }

    private void OnDestroy()
    {
        _subscription?.Dispose();
    }
}
```

---

## Future Enhancements

1. **Tag-Based Synergies**
   ```csharp
   // Check if player has 2+ Fire artifacts
   if (_artifactService.CountByTag(ArtifactTag.Fire) >= 2)
   {
       ApplyFireSynergy();
   }
   ```

2. **Tiered Synergies**
   ```json
   {
     "id": "fire_mastery",
     "tiers": [
       { "count": 2, "effect": "+10% fire damage" },
       { "count": 3, "effect": "+20% fire damage" },
       { "count": 4, "effect": "+30% fire damage, fire aura" }
     ]
   }
   ```

3. **Visual Indicators**
   - Show active synergies in UI
   - Highlight artifacts that contribute to synergies
   - Show progress toward incomplete synergies

4. **Synergy Notifications**
   ```csharp
   _publisher.Publish(new SynergyActivatedMessage(synergyId));
   ```

---

## Related Documentation

- [Artifact System](../Triggers/01_Overview.md)
- [MessagePipe Usage](../Architecture/03_Messaging.md)
- [UI Windows](../UI/01_Windows.md)
