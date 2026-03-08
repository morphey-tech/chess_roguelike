# Chess Roguelike Documentation

## 📚 Documentation Index

### Trigger System
- [01 Overview](Triggers/01_Overview.md) — Introduction to trigger system
- [02 Architecture](Triggers/02_Architecture.md) — Interfaces and class hierarchy
- [03 Pipeline](Triggers/03_Pipeline.md) — Execution order and phases
- [04 Contexts](Triggers/04_Contexts.md) — Context types and usage
- [05 Best Practices](Triggers/05_BestPractices.md) — Examples and guidelines
- [06 Status Effect Categories](Triggers/06_StatusEffectCategories.md) — EffectCategory system

### Gameplay
- [01 Artifact Synergy](Gameplay/01_ArtifactSynergy.md) — Artifact synergy system with MessagePipe

### Architecture
- [01 Overview](Architecture/01_Overview.md) — System architecture overview
- [02 Context Architecture](Architecture/02_Contexts.md) — Context layer design

---

## 🎯 Quick Start

### Creating a Simple Passive

```csharp
using Project.Core.Core.Triggers;
using Project.Gameplay.Gameplay.Combat.Contexts;

namespace Project.Gameplay.Gameplay.Combat.Passives
{
    public sealed class MyPassive : IPassive, IOnBeforeHit
    {
        public string Id { get; }
        public int Priority => TriggerPriorities.Normal;
        public TriggerGroup Group => TriggerGroup.Default;
        public TriggerPhase Phase => TriggerPhase.BeforeHit;

        public bool Matches(TriggerContext context)
        {
            return context.Type == TriggerType.OnBeforeHit;
        }

        public TriggerResult Execute(TriggerContext context)
        {
            if (context is not IDamageContext dc) return TriggerResult.Continue;
            return HandleBeforeHit(dc);
        }

        public TriggerResult HandleBeforeHit(IDamageContext context)
        {
            context.DamageMultiplier *= 1.5f;
            return TriggerResult.Continue;
        }
    }
}
```

### Registering a Passive

```csharp
// In Figure constructor or initialization
figure.AddPassive(new MyPassive("my_passive"));
```

---

## 📋 Status

| System | Status | Tests |
|--------|--------|-------|
| Triggers | ✅ Complete | 16 tests |
| Passives | ✅ Complete | Tested |
| Artifacts | ✅ Complete | Tested |
| Status Effects | ✅ Complete | Tested |

---

## 🔧 Development

### Running Tests

```bash
# In Unity Test Runner
Window → General → Test Runner
```

### Code Style

- All `if` statements use braces `{}`
- No `var` — use explicit types
- `Handle*Event()` for typed interface methods
- `Execute(TriggerContext)` as wrapper only

---

## 📞 Support

For questions or issues, refer to:
- [Trigger Architecture](Triggers/02_Architecture.md)
- [Pipeline Documentation](Triggers/03_Pipeline.md)
