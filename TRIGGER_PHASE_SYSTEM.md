# Trigger Phase System

## Система фаз для триггеров

### Зачем нужны фазы

В сложных боевых системах (Slay the Spire, Monster Train, Across the Obelisk) одно событие может иметь несколько точек исполнения:

```
Урон по фигуре:
1. BeforeCalculation → Проверка условий (сон, стун, заморозка)
2. ModifyCalculation → Модификация урона (крит, уязвимость, сопротивление)
3. BeforeApplication → Поглощение урона (щит, перенаправление)
4. AfterApplication → Последствия (кровотечение, вампиризм, шипы)
```

---

## TriggerPhase

```csharp
public enum TriggerPhase
{
    // Damage Pipeline
    BeforeCalculation = 1,
    ModifyCalculation = 2,
    BeforeApplication = 3,
    AfterApplication = 4,

    // Attack Pipeline
    BeforeDeclare = 10,
    OnDeclare = 11,
    AfterDeclare = 12,
    BeforeHit = 13,
    AfterHit = 14,

    // Death Pipeline
    BeforeDeath = 20,
    OnDeath = 21,
    AfterDeath = 22,

    // Turn Pipeline
    BeforeTurn = 30,
    OnTurnStart = 31,
    DuringTurn = 32,
    OnTurnEnd = 33,
    AfterTurn = 34,

    // Movement Pipeline
    BeforeMove = 40,
    DuringMove = 41,
    AfterMove = 42,
}
```

---

## Обновлённый ITrigger

```csharp
public interface ITrigger
{
    int Priority { get; }           // Порядок внутри фазы
    TriggerPhase Phase { get; }     // Фаза исполнения
    bool Matches(TriggerContext context);
    TriggerResult Execute(TriggerContext context);
}
```

---

## Пример: Damage Pipeline

### Шаг 1: Создание контекста с фазами

```csharp
public class DamagePipeline
{
    private readonly TriggerService _triggerService;
    
    public DamageResult ApplyDamage(Figure attacker, Figure target, float baseDamage)
    {
        var context = new BeforeHitContext
        {
            Attacker = attacker,
            Target = target,
            BaseDamage = baseDamage
        };
        
        // Фаза 1: BeforeCalculation
        var beforeCalcContext = TriggerContextBuilder.For(TriggerType.OnBeforeHit)
            .WithPhase(TriggerPhase.BeforeCalculation)
            .WithActor(attacker)
            .WithTarget(target)
            .WithData(context)
            .Build();
        
        if (_triggerService.Execute(TriggerType.OnBeforeHit, TriggerPhase.BeforeCalculation, beforeCalcContext) 
            == TriggerResult.Cancel)
        {
            return DamageResult.Cancelled; // Атака отменена (стун, сон)
        }
        
        // Фаза 2: ModifyCalculation
        var modifyContext = TriggerContextBuilder.For(TriggerType.OnBeforeHit)
            .WithPhase(TriggerPhase.ModifyCalculation)
            .WithActor(attacker)
            .WithTarget(target)
            .WithData(context)
            .Build();
        
        _triggerService.Execute(TriggerType.OnBeforeHit, TriggerPhase.ModifyCalculation, modifyContext);
        
        // Рассчитываем финальный урон после модификаторов
        float finalDamage = CalculateFinalDamage(context);
        
        // Фаза 3: BeforeApplication
        var beforeAppContext = TriggerContextBuilder.For(TriggerType.OnBeforeHit)
            .WithPhase(TriggerPhase.BeforeApplication)
            .WithActor(attacker)
            .WithTarget(target)
            .WithValue((int)finalDamage)
            .WithData(context)
            .Build();
        
        if (_triggerService.Execute(TriggerType.OnBeforeHit, TriggerPhase.BeforeApplication, beforeAppContext) 
            == TriggerResult.Cancel)
        {
            return DamageResult.Absorbed; // Щит поглотил урон
        }
        
        // Применяем урон
        target.Stats.TakeDamage(finalDamage);
        
        // Фаза 4: AfterApplication
        var afterAppContext = TriggerContextBuilder.For(TriggerType.OnBeforeHit)
            .WithPhase(TriggerPhase.AfterApplication)
            .WithActor(attacker)
            .WithTarget(target)
            .WithValue((int)finalDamage)
            .WithData(context)
            .Build();
        
        _triggerService.Execute(TriggerType.OnBeforeHit, TriggerPhase.AfterApplication, afterAppContext);
        
        return new DamageResult { Final = finalDamage };
    }
}
```

### Шаг 2: Триггеры для разных фаз

```csharp
// Фаза: BeforeCalculation - проверка на стун
public class StunCheckPassive : IPassive, IOnBeforeHit
{
    public int Priority => TriggerPriorities.Critical;
    public TriggerPhase Phase => TriggerPhase.BeforeCalculation;
    
    public bool Matches(TriggerContext context)
    {
        return context.Type == TriggerType.OnBeforeHit;
    }
    
    public TriggerResult Execute(TriggerContext context)
    {
        if (context.Actor is Figure attacker && attacker.Effects.HasEffect("stun"))
        {
            return TriggerResult.Cancel; // Атака отменена
        }
        return TriggerResult.Continue;
    }
}

// Фаза: ModifyCalculation - критический удар
public class CriticalPassive : IPassive, IOnBeforeHit
{
    public int Priority => TriggerPriorities.High;
    public TriggerPhase Phase => TriggerPhase.ModifyCalculation;
    
    public TriggerResult Execute(TriggerContext context)
    {
        if (!context.TryGetData<BeforeHitContext>(out var beforeHit)) 
            return TriggerResult.Continue;

        if (_random.Chance(_critChance))
        {
            beforeHit.DamageMultiplier *= _critMultiplier;
            beforeHit.IsCritical = true;
        }
        return TriggerResult.Continue;
    }
}

// Фаза: BeforeApplication - щит
public class ShieldArtifact : ArtifactBase, IOnBeforeHit
{
    public int Priority => TriggerPriorities.Low;
    public TriggerPhase Phase => TriggerPhase.BeforeApplication;
    
    public TriggerResult Execute(TriggerContext context)
    {
        if (_shield <= 0) return TriggerResult.Continue;
        
        _shield -= context.Value;
        if (_shield >= 0)
        {
            return TriggerResult.Cancel; // Урон полностью поглощён
        }
        return TriggerResult.Continue;
    }
}

// Фаза: AfterApplication - вампиризм
public class LifestealPassive : IPassive, IOnAfterHit
{
    public int Priority => TriggerPriorities.Normal;
    public TriggerPhase Phase => TriggerPhase.AfterApplication;
    
    public TriggerResult Execute(TriggerContext context)
    {
        if (!context.TryGetData<AfterHitContext>(out var afterHit)) 
            return TriggerResult.Continue;

        int heal = (int)(afterHit.DamageDealt * _percent);
        afterHit.Attacker.Stats.Heal(heal);
        return TriggerResult.Continue;
    }
}
```

---

## Pipeline Helpers

```csharp
public static class TriggerPhases
{
    public static readonly TriggerPhase[] DamagePipeline =
    {
        TriggerPhase.BeforeCalculation,
        TriggerPhase.ModifyCalculation,
        TriggerPhase.BeforeApplication,
        TriggerPhase.AfterApplication
    };
    
    public static readonly TriggerPhase[] AttackPipeline =
    {
        TriggerPhase.BeforeDeclare,
        TriggerPhase.OnDeclare,
        TriggerPhase.AfterDeclare,
        TriggerPhase.BeforeHit,
        TriggerPhase.AfterHit
    };
}

// Использование
public void ExecuteDamagePipeline(Figure attacker, Figure target, BeforeHitContext context)
{
    foreach (var phase in TriggerPhases.DamagePipeline)
    {
        var triggerContext = TriggerContextBuilder.For(TriggerType.OnBeforeHit)
            .WithPhase(phase)
            .WithActor(attacker)
            .WithTarget(target)
            .WithData(context)
            .Build();
        
        TriggerResult result = _triggerService.Execute(TriggerType.OnBeforeHit, phase, triggerContext);
        
        if (result == TriggerResult.Cancel)
        {
            return; // Прерываем pipeline
        }
    }
}
```

---

## Порядок исполнения

```
OnBeforeHit Event
  │
  ├─ Phase: BeforeCalculation (1)
  │   ├─ Priority: Critical (-100) → StunCheck.Cancel
  │   ├─ Priority: High (-50)      → FreezeCheck
  │   └─ Priority: Normal (0)      → AttackValidator
  │
  ├─ Phase: ModifyCalculation (2)
  │   ├─ Priority: High (-50)      → CriticalHit
  │   ├─ Priority: Normal (0)      → DamageBuff
  │   └─ Priority: Low (50)        → VulnerabilityDebuff
  │
  ├─ Phase: BeforeApplication (3)
  │   ├─ Priority: Low (50)        → ShieldAbsorb.Cancel
  │   └─ Priority: Normal (0)      → DamageRedirection
  │
  └─ Phase: AfterApplication (4)
      ├─ Priority: High (-50)      → ThornsReflect
      ├─ Priority: Normal (0)      → Lifesteal
      └─ Priority: Low (50)        → BleedApply
```

---

## Преимущества системы фаз

1. **Детальный контроль** — каждый аспект события обрабатывается в своей фазе
2. **Предсказуемость** — порядок исполнения явно задан
3. **Гибкость** — триггеры могут влиять на разные этапы события
4. **Отмена в любой момент** — `TriggerResult.Cancel` работает на любой фазе
5. **Модульность** — новые фазы добавляются без изменения существующего кода

---

## Сравнение с играми

| Игра | Реализация | Аналог в нашей системе |
|------|-----------|----------------------|
| **Slay the Spire** | Power-триггеры по фазам | TriggerPhase + Priority |
| **Monster Train** | Слои эффектов (Layer) | TriggerPhase |
| **Across the Obelisk** | Steps боя | TriggerPipeline |
| **Darkest Dungeon** | Combat queue | TriggerType + Phase |

---

## Best Practices

### ✅ Правильно

```csharp
// Явное указание фазы
public class CriticalPassive : IPassive
{
    public TriggerPhase Phase => TriggerPhase.ModifyCalculation;
    public int Priority => TriggerPriorities.High;
}
```

### ❌ Неправильно

```csharp
// Проверка фазы в Execute (медленнее)
public TriggerResult Execute(TriggerContext context)
{
    if (context.Phase != TriggerPhase.ModifyCalculation)
        return TriggerResult.Continue;
    // ...
}
```

### ✅ Правильно

```csharp
// Использование pipeline helper
foreach (var phase in TriggerPhases.DamagePipeline)
{
    _triggerService.Execute(TriggerType.OnBeforeHit, phase, context);
}
```

### ❌ Неправильно

```csharp
// Ручное перечисление фаз
_triggerService.Execute(TriggerType.OnBeforeHit, TriggerPhase.BeforeCalculation, context);
_triggerService.Execute(TriggerType.OnBeforeHit, TriggerPhase.ModifyCalculation, context);
_triggerService.Execute(TriggerType.OnBeforeHit, TriggerPhase.BeforeApplication, context);
// ...
```
