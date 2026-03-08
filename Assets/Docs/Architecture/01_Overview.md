# Architecture Overview

## System Layers

```
┌─────────────────────────────────────────────────────────┐
│  Unity Layer                                            │
│  ─────────────────────────────────────────────────────  │
│  - MonoBehaviours                                       │
│  - Presenters (FigurePresenter, BoardPresenter)         │
│  - Views (FigureView, CellView)                         │
│  - UI (Windows, Tooltips)                               │
└────────────────────┬────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────┐
│  Gameplay Layer (Project.Gameplay)                      │
│  ─────────────────────────────────────────────────────  │
│  - Figures, Combat, Movement                            │
│  - Status Effects, Artifacts, Passives                  │
│  - Turn System, AI                                      │
│  - Grid, Pathfinding                                    │
└────────────────────┬────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────┐
│  Core Layer (Project.Core)                              │
│  ─────────────────────────────────────────────────────  │
│  - Trigger System                                       │
│  - Configs, Save System                                 │
│  - Logging, Random                                      │
│  - Grid (basic), Assets                                 │
└─────────────────────────────────────────────────────────┘
```

---

## Dependency Rules

```
Unity → Gameplay → Core
   ✗        ✗       ✗
   │        │       │
   └────────┴───────┘
   (no upward dependencies)
```

### Examples

```csharp
// ✅ Core doesn't know about Figure
public interface ITriggerEntity
{
    string TriggerId { get; }  // string, not Figure
}

// ✅ Gameplay implements Core interfaces
public class Figure : Entity, ITriggerEntity
{
    string ITriggerEntity.Id => TriggerId;
    public string TriggerId => base.Id.ToString();
    public int EntityId => base.Id;  // For visual systems
}

// ✅ Unity references Gameplay
public class FigurePresenter : MonoBehaviour
{
    private Figure _figure;  // Gameplay type
}
```

---

## Key Systems

### Trigger System

See [Trigger Documentation](../Triggers/01_Overview.md)

```
TriggerService
    │
    ├─ Register(ITrigger)
    ├─ Unregister(ITrigger)
    └─ Execute(TriggerType, TriggerContext)
            │
            ▼
    TriggerExecutor
            │
            ├─ Cache triggers by (Type, Phase)
            ├─ Sort by Priority → Group → Order
            └─ ExecuteTyped() for type-safe dispatch
```

### Combat System

```
CombatResolver
    │
    ├─ AttackRuleService (target selection)
    ├─ DamageApplier (damage calculation)
    └─ TriggerService (passive triggers)
            │
            ├─ BeforeHit pipeline
            ├─ AfterHit pipeline
            └─ Kill/Death triggers
```

### Status Effect System

```
StatusEffectSystem (per Figure)
    │
    ├─ AddOrStack(IStatusEffect)
    │   └─ Register in TriggerService
    ├─ Remove(string id)
    │   └─ Unregister from TriggerService
    └─ GetEffects()
```

### Artifact System

```
ArtifactService
    │
    ├─ Acquire(string configId)
    │   └─ Create ArtifactInstance
    │       └─ Register artifact trigger
    ├─ Remove(string instanceId)
    │   └─ Unregister artifact trigger
    └─ GetArtifacts()
```

---

## Data Flow

### Attack Flow

```
1. Player clicks "Attack"
         │
         ▼
2. InputDispatcher → InteractionController
         │
         ▼
3. AttackActionBuilder builds action
         │
         ▼
4. CombatResolver resolves attack
         │
         ├─ Select target (TauntRule, etc.)
         ├─ Calculate damage (TriggerService.BeforeHit)
         ├─ Apply damage (DamageApplier)
         └─ Trigger after-hit (TriggerService.AfterHit)
         │
         ▼
5. VisualCommandExecutor plays visuals
         │
         ▼
6. Update HP, check death
         │
         ▼
7. TriggerService.TriggerKill/Death
```

### Turn Flow

```
1. TurnService.EndTurn()
         │
         ▼
2. TriggerService.TriggerTurnEnd()
         │
         ▼
3. Switch active team
         │
         ▼
4. Reset MovedThisTurn flags
         │
         ▼
5. TriggerService.TriggerTurnStart()
         │
         ├─ InspirationPassive triggers
         └─ Other turn-start effects
```

---

## Configuration

### Config Repositories

```csharp
// Core.Configs
public class FigureConfigRepository
{
    public IReadOnlyList<FigureConfig> Figures { get; }
    public FigureConfig Get(string id);
}

public class ArtifactConfigRepository
{
    public IReadOnlyList<ArtifactConfig> Artifacts { get; }
    public ArtifactConfig Get(string id);
}
```

### Config Structure

```json
// figures_conf.json
{
  "figures": [
    {
      "id": "knight",
      "stats_id": "knight_stats",
      "description_id": "knight_desc",
      "asset_key": "Prefabs/Figures/Knight",
      "passives": ["critical", "swarm"]
    }
  ]
}
```

---

## Save System

```
SaveService
    │
    ├─ SaveGame()
    │   └─ Serialize RunState
    │       ├─ Figures
    │       ├─ Artifacts
    │       └─ Economy
    │
    └─ LoadGame()
        └─ Deserialize RunState
```

---

## Next Steps

- [Context Architecture](02_Contexts.md)
