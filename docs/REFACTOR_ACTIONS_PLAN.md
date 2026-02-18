# Refactor: Step-Based Turn System → Action-Based System

**Interfaces added under `Assets/Scripts/Gameplay/Turn/Actions/`:** `ICombatAction`, `IActionBuilder`, `IActionBuilderContext`, `ActionConfig`.

---

## 1. Goals

- **Composite actions as first-class**: One logical action (e.g. "move then attack" for Rook) with a single (from, to) contract, shared by UI, AI, and execution.
- **Single source of truth**: Validation (can we do this?), targeting (where can we do it?), and execution (do it) use the same `ICombatAction` so UI/AI and execution cannot desync.
- **Keep config-driven patterns**: TurnPatternFactory, ConditionRegistry, and config-based pattern descriptions stay; only the "step" is replaced by "action".

---

## 2. Refactor Plan (Phases)

| Phase | What | Risk |
|-------|------|------|
| **A** | Introduce action interfaces and `IActionBuilder`; add `ActionBuilderRegistry` and config for actions. | Low |
| **B** | Implement first actions: `MoveAction`, `AttackAction` (wrap existing step logic). | Low |
| **C** | Implement `MoveThenAttackAction` (Rook); single validation + execution. | Medium |
| **D** | TurnPatternDescription holds `ICombatAction` instead of `ITurnStep`; resolver returns action. | Medium |
| **E** | StageQueryService / AI use actions to get valid targets (one API). | Medium |
| **F** | TurnExecutor runs action.ExecuteAsync; remove step-based path. | Low |
| **G** | Deprecate TurnStepFactory, ITurnStep, CompositeTurnStep. | Low |

**Order**: A → B → C → D → E → F → G. You can keep steps and actions in parallel until E/F, then cut over.

---

## 3. Interfaces

### 3.1 Core: one action = one (from, to) contract

```csharp
namespace Project.Gameplay.Gameplay.Turn.Actions
{
    /// <summary>
    /// One logical thing a unit can do in a turn (move, attack, move+attack, etc.).
    /// Same instance used for: validation, UI targeting, AI, and execution.
    /// </summary>
    public interface ICombatAction
    {
        string Id { get; }

        /// <summary>
        /// Can this action be performed with the given context?
        /// Context.From = actor position, Context.To = chosen target (move dest or attack target).
        /// </summary>
        bool CanExecute(ActionContext context);

        /// <summary>
        /// All positions that are valid "To" for this action from the given actor position.
        /// Used by UI (highlight) and AI (candidate targets). Same logic as CanExecute.
        /// </summary>
        IReadOnlyCollection<GridPosition> GetValidTargets(Figure actor, GridPosition from, BoardGrid grid);

        /// <summary>
        /// Execute the action. Call only when CanExecute(context) is true.
        /// Sets context.ActionExecuted and updates context.From as needed.
        /// </summary>
        UniTask ExecuteAsync(ActionContext context);
    }
}
```

### 3.2 Builder: config → action (keeps config-driven patterns)

```csharp
namespace Project.Gameplay.Gameplay.Turn.Actions
{
    /// <summary>
    /// Builds an ICombatAction from config. Registered by action type (e.g. "move", "attack", "move_then_attack").
    /// </summary>
    public interface IActionBuilder
    {
        string ActionType { get; }

        ICombatAction Build(ActionConfig config, IActionBuilderContext builderContext);
    }

    /// <summary>
    /// Services and factories the builder needs to construct actions (MovementService, AttackFactory, etc.).
    /// </summary>
    public interface IActionBuilderContext
    {
        MovementService MovementService { get; }
        AttackStrategyFactory AttackFactory { get; }
        IAttackResolver AttackResolver { get; }
        CombatResolver CombatResolver { get; }
        // ... other services currently used by TurnStepFactory
    }

    /// <summary>
    /// Config for one action (replaces StepConfig for the action layer).
    /// </summary>
    public sealed class ActionConfig
    {
        public string Type { get; set; }   // "move", "attack", "move_then_attack"
        public string Strategy { get; set; }
        public ActionConfig[] SubActions { get; set; }  // for composite actions
    }
}
```

### 3.3 Pattern description: condition + action (not step)

```csharp
// TurnPatternDescription (evolved)
public sealed class TurnPatternDescription
{
    public string Id { get; }
    public int Priority { get; }
    public ITurnCondition Condition { get; }
    public ConditionParams ConditionParams { get; }
    public ICombatAction Action { get; }  // was: ITurnStep Step

    public bool Evaluate(ActionContext context) => Condition.Evaluate(context, ConditionParams);
}
```

### 3.4 Resolver and executor (same flow, different type)

```csharp
// TurnPatternResolver returns ICombatAction instead of ITurnStep
public interface ITurnPatternResolver
{
    ICombatAction Resolve(Figure actor, TurnPattern pattern, ActionContext context);
}

// TurnExecutor
// 1. Build context (actor, from, to, grid).
// 2. action = _patternResolver.Resolve(actor, actor.TurnPattern, context).
// 3. if (action == null || !action.CanExecute(context)) return Failed.
// 4. await action.ExecuteAsync(context).
// 5. return result from context.
```

**Desync prevention**: Executor always checks `action.CanExecute(context)` immediately before `ExecuteAsync`. UI and AI use the same `action.GetValidTargets(...)` / `action.CanExecute(context)` so valid targets and execution are identical.

---

## 4. Migration: Steps → Actions

### 4.1 Mapping

| Current step type   | New action type        | Notes |
|--------------------|------------------------|--------|
| `move`             | `move`                 | MoveAction wraps MovementService + VisualPipeline (same as MoveStep). |
| `attack`           | `attack`               | AttackAction wraps current AttackStep logic. |
| `move_to_killed`   | `move_to_killed`       | Same behaviour, built as single action. |
| `move_to_enemy`    | `move_to_enemy`        | Same behaviour, built as single action. |
| `composite` [move, attack] | `move_then_attack` | One action: GetValidTargets = “attack targets reachable after a move along line”; Execute = move then attack. |

### 4.2 Config change (minimal)

Keep existing **condition** config (ConditionRegistry, condition_id, condition_params). Only the “steps” part becomes “action”:

**Option A – Same file, new field (recommended)**  
- In `TurnPatternDescriptionConfig`, add `ActionConfig Action { get; set; }`.  
- If `Action != null`, use it and build one `ICombatAction`; else fall back to building from `Steps` (during migration).

**Option B – New config**  
- New repo e.g. `ActionConfigRepository`; pattern description references `action_id` and you load one ActionConfig by id.

Example (Option A) in JSON:

```json
{
  "id": "rook_move_then_attack",
  "priority": 10,
  "condition_id": "target_is_enemy",
  "condition_params": {},
  "action": {
    "type": "move_then_attack",
    "strategy": "rook"
  }
}
```

### 4.3 TurnPatternFactory (evolved)

- Inject `IActionBuilderRegistry` (or a factory that resolves builders by `ActionConfig.Type`).
- `CreatePatternFromConfig`:
  - If config has `Action`: get builder for `config.Action.Type`, call `builder.Build(config.Action, builderContext)` → `ICombatAction`.
  - Else (migration): build from `Steps` via existing TurnStepFactory and wrap in a **StepBridgeAction** (see below).
- `TurnPatternDescription` gets `ICombatAction` (from builder or from StepBridgeAction).

### 4.4 StepBridgeAction (temporary)

During migration, wrap an `ITurnStep` so it implements `ICombatAction`:

- `CanExecute(context)`: run condition only (or a minimal check); optionally run step in a “dry run” if needed (not ideal).
- `GetValidTargets`: use existing MovementService / AttackQueryService for “move” and “attack” steps; for composite, combine (see Rook below).
- `ExecuteAsync`: delegate to `_step.ExecuteAsync(context)`.

Once all patterns use actions, remove StepBridgeAction and TurnStepFactory from the hot path.

---

## 5. Example: Rook (move then attack in one action)

### 5.1 Contract

- **From**: current Rook position.  
- **To**: cell of the enemy we want to attack (same as today’s “attack target”).  
- **Semantics**: “Move along the line toward `To` (up to movement rules), then attack the enemy at `To`.”

### 5.2 GetValidTargets (single source of truth)

- For each enemy on the board, get its cell `E`.
- If the Rook can move along the line (row or column) toward `E` and then attack `E` from some intermediate cell (e.g. adjacent or in range), then `E` is a valid target.
- Implementation:
  - For each enemy position `E`, compute the straight line from `from` to `E`.
  - Walk from `from` toward `E` using movement rules (e.g. can’t pass through blockers); collect the set of reachable cells on that line.
  - For each reachable cell `mid`, check if from `mid` the unit can attack `E` (existing attack rules).
  - If any `mid` allows attack, add `E` to valid targets.

So “valid targets” = all enemy cells that are attackable after some legal move along the line. UI and AI both call `GetValidTargets(actor, from, grid)` and get the same set.

### 5.3 CanExecute(context)

- `context.To` must be in `GetValidTargets(context.Actor, context.From, context.Grid)` (or duplicate the same logic once in a private method and use it from both).

### 5.4 ExecuteAsync(context)

- Compute the move: from `context.From`, along the line to `context.To`, the first cell from which we can attack `context.To` (e.g. adjacent for melee, or in range for ranged). Call that `moveTo`.
- If `moveTo != context.From`: run move (MovementService + visual), then set `context.From = moveTo`.
- Run attack from `context.From` to `context.To` (same as AttackStep).
- Set `context.ActionExecuted = true`.

### 5.5 Code sketch (Rook)

```csharp
public sealed class MoveThenAttackAction : ICombatAction
{
    public string Id { get; }
    private readonly MovementService _movementService;
    private readonly IAttackQueryService _attackQuery;
    private readonly ICombatAction _moveAction;  // or inline move logic
    private readonly ICombatAction _attackAction;

    public MoveThenAttackAction(string id, MovementService movementService,
        IAttackQueryService attackQuery, ICombatAction moveAction, ICombatAction attackAction)
    {
        Id = id;
        _movementService = movementService;
        _attackQuery = attackQuery;
        _moveAction = moveAction;
        _attackAction = attackAction;
    }

    public bool CanExecute(ActionContext context)
    {
        return GetValidTargets(context.Actor, context.From, context.Grid).Contains(context.To);
    }

    public IReadOnlyCollection<GridPosition> GetValidTargets(Figure actor, GridPosition from, BoardGrid grid)
    {
        var targets = new HashSet<GridPosition>();
        Team enemyTeam = actor.Team == Team.Player ? Team.Enemy : Team.Player;

        foreach (Figure enemy in grid.GetFiguresByTeam(enemyTeam))
        {
            BoardCell? cell = grid.FindFigure(enemy);
            if (cell == null) continue;

            GridPosition enemyPos = cell.Position;
            if (from.Row != enemyPos.Row && from.Column != enemyPos.Column)
                continue; // not on same row/column

            // Reachable cells along the line (movement rules)
            IEnumerable<GridPosition> lineMoves = _movementService.GetAvailableMoves(actor, from)
                .Where(m => m.CanOccupy() && m.IsFree && IsOnLine(from, enemyPos, m.Position))
                .Select(m => m.Position);

            foreach (GridPosition mid in lineMoves)
            {
                if (_attackQuery.GetTargets(actor, mid, grid).Contains(enemyPos))
                {
                    targets.Add(enemyPos);
                    break;
                }
            }
        }
        return targets;
    }

    public async UniTask ExecuteAsync(ActionContext context)
    {
        GridPosition attackTarget = context.To;
        GridPosition moveTo = ComputeMoveTo(context.Actor, context.From, attackTarget, context.Grid);

        if (moveTo != context.From)
        {
            context.To = moveTo;
            await _moveAction.ExecuteAsync(context);
            context.From = moveTo;
        }

        context.To = attackTarget;
        await _attackAction.ExecuteAsync(context);
        context.ActionExecuted = true;
    }

    private static GridPosition ComputeMoveTo(Figure actor, GridPosition from, GridPosition attackTarget, BoardGrid grid)
    {
        // Walk toward attackTarget, find closest cell from which we can attack.
        // Implementation mirrors GetValidTargets (walk line, first valid attack cell).
    }

    private static bool IsOnLine(GridPosition from, GridPosition to, GridPosition p)
    {
        return (from.Row == to.Row && from.Row == p.Row) || (from.Column == to.Column && from.Column == p.Column);
    }
}
```

Builder for config `type: "move_then_attack"`:

```csharp
public sealed class MoveThenAttackActionBuilder : IActionBuilder
{
    public string ActionType => "move_then_attack";
    public ICombatAction Build(ActionConfig config, IActionBuilderContext ctx)
        => new MoveThenAttackAction(config.Id ?? "move_then_attack", ctx.MovementService, ctx.AttackQuery, ...);
}
```

---

## 6. Config-driven patterns (unchanged spirit)

- **ConditionRegistry**: no change. Conditions still answer “does this (actor, from, to) match this pattern?”.
- **TurnPatternFactory**:
  - Still loads pattern set by id; each pattern description still has condition + params.
  - Only change: instead of building `ITurnStep` from `StepConfig[]`, build one `ICombatAction` from `ActionConfig` via `IActionBuilder`.
- **TurnPatternResolver**: same as now: filter by condition, sort by priority, return the first match. Type changes from `ITurnStep` to `ICombatAction`.
- **Config**: pattern set id → list of pattern ids → each pattern id → condition_id + condition_params + **action** (single ActionConfig). No more “steps” array for the primary path.

So: **pattern selection** stays config-driven (condition + priority); **what runs** is one action (which can be composite internally like Rook).

---

## 7. Clean architecture and desync summary

| Concern | How it’s addressed |
|--------|---------------------|
| **Single source of truth** | Validation, targeting, and execution all go through the same `ICombatAction` instance. |
| **UI = AI = execution** | UI/AI call `GetValidTargets` and optionally `CanExecute`; executor calls `CanExecute` then `ExecuteAsync`. No separate “move service + attack service” for targeting vs execution. |
| **Composite as one action** | Rook is one `MoveThenAttackAction` with one (from, to) and one `GetValidTargets`/`ExecuteAsync`. |
| **Config-driven** | TurnPatternFactory + ConditionRegistry + ActionConfig; builders registered by type. |
| **Migration** | StepBridgeAction + optional “steps” in config until all patterns use actions; then remove steps. |

This gives you an action-based system where the same logic drives UI, AI, and execution, with config-driven patterns and a clear path from your current steps to actions, including a concrete Rook “move then attack” example.
