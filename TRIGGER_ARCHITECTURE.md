# Trigger System Architecture

## Новая архитектура триггерной системы

### Основные принципы

1. **TriggerService не знает о владельцах** — только хранение и исполнение триггеров
2. **Владельцы сами регистрируют триггеры** — артефакты, пассивки, эффекты
3. **Единая точка исполнения** — все триггеры выполняются через `TriggerService`
4. **Контроль потока** — `TriggerResult` позволяет отменять события
5. **Система фаз** — `TriggerPhase` для детального контроля исполнения

---

## Core Architecture

```
Core
 └─ Triggers
     ├─ ITrigger
     │   ├─ Priority        (порядок внутри фазы)
     │   ├─ Phase           (фаза исполнения)
     │   ├─ Matches(context) (быстрый фильтр)
     │   └─ Execute(context) (возвращает TriggerResult)
     │
     ├─ TriggerContext
     │   ├─ Type            (тип события)
     │   ├─ Phase           (фаза внутри события)
     │   ├─ Actor           (источник)
     │   ├─ Target          (цель)
     │   ├─ Value           (числовое значение)
     │   ├─ StackCount      (для стакаемых эффектов)
     │   ├─ Data            (типизированные данные)
     │   └─ CustomData      (гибкие данные по ключам)
     │
     ├─ TriggerResult
     │   ├─ Continue        (продолжить)
     │   ├─ Stop            (остановить триггеры)
     │   ├─ Cancel          (отменить событие)
     │   └─ Replace         (заменить контекст)
     │
     ├─ TriggerPhase
     │   ├─ DamagePipeline  (BeforeCalculation → AfterApplication)
     │   ├─ AttackPipeline  (BeforeDeclare → AfterHit)
     │   ├─ DeathPipeline   (BeforeDeath → AfterDeath)
     │   └─ TurnPipeline    (BeforeTurn → AfterTurn)
     │
     └─ TriggerPriorities
         ├─ Critical (-100)
         ├─ High (-50)
         ├─ Normal (0)
         ├─ Low (50)
         └─ Cleanup (100)
```

### 1. TriggerService

```csharp
public sealed class TriggerService : IDisposable
{
    private readonly List<ITrigger> _triggers = new();
    
    public void Register(ITrigger trigger);
    public void Unregister(ITrigger trigger);
    public TriggerResult Execute(TriggerType type, TriggerContext context);
}
```

**Ответственность:**
- Хранение списка всех активных триггеров
- Сортировка по приоритету
- Исполнение с обработкой `TriggerResult`

**Не знает про:**
- `Figure`, `Artifact`, `StatusEffect`, `IPassive`

---

### 2. ITrigger

```csharp
public enum TriggerResult
{
    Continue = 0,  // Продолжить обработку
    Stop = 1,      // Остановить триггеры, применить событие
    Cancel = 2,    // Отменить событие полностью
    Replace = 3    // Заменить событие (модифицированный контекст)
}

public interface ITrigger
{
    int Priority { get; }
    bool Matches(TriggerContext context);
    TriggerResult Execute(TriggerContext context);
}
```

**Приоритеты:**
```csharp
public static class TriggerPriorities
{
    public const int Critical = -100;  // Отмена смерти, воскрешение
    public const int High = -50;       // Модификация урона, уклонение
    public const int Normal = 0;       // Большинство эффектов
    public const int Low = 50;         // Баффы, щиты
    public const int Cleanup = 100;    // Награды, очистка
}
```

---

### 3. Владельцы триггеров

#### Figure
```csharp
public class Figure
{
    private readonly TriggerService _triggerService;
    
    public void AddPassive(IPassive passive)
    {
        BasePassives.Add(passive);
        _triggerService?.Register(passive);  // Регистрация
    }
    
    public void RemovePassive(IPassive passive)
    {
        if (BasePassives.Remove(passive))
        {
            _triggerService?.Unregister(passive);  // Отмена регистрации
        }
    }
}
```

#### ArtifactService
```csharp
public sealed class ArtifactService : IDisposable
{
    public async UniTask<ArtifactInstance> Add(string configId, int stackCount = 1)
    {
        IArtifact artifact = await _factory.Create(configId);
        _artifacts.Add(instance);
        _triggerService.Register(artifact);  // Регистрация
        // ...
    }
    
    public bool Remove(string instanceId)
    {
        ArtifactInstance? instance = Find(instanceId);
        if (instance == null) return false;
        
        _triggerService.Unregister(instance.Artifact);  // Отмена регистрации
        // ...
    }
}
```

#### StatusEffectSystem
```csharp
public sealed class StatusEffectSystem
{
    public void AddOrStack(IStatusEffect effect)
    {
        _effects[effect.Id] = effect;
        effect.OnApply(_owner);
        // Эффекты исполняются локально, не через TriggerService
    }
}
```

---

## Использование

### Пример 1: Уклонение (DodgeEffect)

```csharp
public class DodgeEffect : StatusEffectBase, IOnBeforeHit
{
    public override TriggerResult Execute(TriggerContext context)
    {
        if (!context.TryGetData<BeforeHitContext>(out var ctx)) 
            return TriggerResult.Continue;

        if (!TryConsumeUse()) 
            return TriggerResult.Continue;
            
        if (_random.Chance(_chance))
        {
            ctx.IsDodged = true;
            ctx.IsCancelled = true;
            return TriggerResult.Cancel;  // Отмена атаки!
        }
        return TriggerResult.Continue;
    }
}
```

### Пример 2: Щит (ShieldArtifact)

```csharp
public class ShieldArtifact : ArtifactBase, IOnDamageReceived
{
    public override TriggerResult Execute(TriggerContext context)
    {
        if (_shield <= 0) 
            return TriggerResult.Continue;
        
        _shield -= context.Value;
        return TriggerResult.Cancel;  // Урон отменён!
    }
}
```

### Пример 3: Критический удар (CriticalPassive)

```csharp
public class CriticalPassive : IPassive, IOnBeforeHit
{
    public override TriggerResult Execute(TriggerContext context)
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
```

---

## Порядок исполнения

```
TriggerBeforeHit(attacker, target, context)
  ↓
[Priority: Critical (-100)]
  → ShieldArtifact.Cancel → отмена атаки (возврат false)
  ↓
[Priority: High (-50)]
  → DodgeEffect.Cancel → уклонение (возврат false)
  → CriticalPassive → модификация урона
  → ExecutePassive → бонус к урону по низким HP
  ↓
[Priority: Normal (0)]
  → PiercePassive → сквозной урон
  → SplashPassive → урон по области
  → ThornsPassive → отражение урона
  ↓
[Priority: Low (50)]
  → Buffs, shields
  ↓
[Priority: Cleanup (100)]
  → Rewards, cleanup
```

---

## Расширения для удобного использования

```csharp
// Extension methods
public static class TriggerServiceExtensions
{
    public static bool TriggerBeforeHit(this TriggerService service, 
        Figure attacker, Figure target, BeforeHitContext context);
    
    public static void TriggerAfterHit(this TriggerService service, 
        Figure attacker, Figure target, AfterHitContext context);
    
    public static void TriggerKill(this TriggerService service, 
        Figure killer, Figure victim);
    
    public static void TriggerDeath(this TriggerService service, 
        Figure victim, Figure killer);
}

// Builder
public static class TriggerContextBuilder
{
    public static TriggerContextBuilder For(TriggerType type);
    public TriggerContextBuilder WithActor(object actor);
    public TriggerContextBuilder WithTarget(object target);
    public TriggerContextBuilder WithData(object data);
    public TriggerContext Build();
}
```

---

## Диаграмма зависимостей

```
┌─────────────────────────────────────────────────────────┐
│                    TriggerService                       │
│  ┌──────────────────────────────────────────────────┐  │
│  │  List<ITrigger> _triggers                        │  │
│  │  Register(ITrigger)                              │  │
│  │  Unregister(ITrigger)                            │  │
│  │  Execute(TriggerType, TriggerContext)            │  │
│  └──────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────┘
           ▲                    ▲                    ▲
           │                    │                    │
    ┌──────┴──────┐      ┌─────┴──────┐     ┌──────┴──────┐
    │   Figure    │      │ Artifact   │     │ StatusEffect│
    │   AddPassive│      │  Service   │     │   System    │
    │   Register  │      │   Add      │     │  AddOrStack │
    └─────────────┘      └────────────┘     └─────────────┘
```

---

## Преимущества новой архитектуры

1. **Низкая связанность** — `TriggerService` не зависит от конкретных типов
2. **Автоматическая регистрация** — владельцы сами управляют жизненным циклом
3. **Контроль потока** — `TriggerResult` позволяет отменять события
4. **Приоритеты** — порядок исполнения контролируется через `Priority`
5. **Тестируемость** — легко моковать `TriggerService`
6. **Расширяемость** — новые триггеры добавляются без изменения ядра

---

## Миграция

### Было:
```csharp
// TriggerService знает про Figure
public void TriggerBeforeHit(Figure attacker, Figure target, BeforeHitContext context)
{
    var triggers = CollectTriggersForFigure(attacker)  // Высокая связанность
        .Concat(CollectTriggersForFigure(target));
    // ...
}
```

### Стало:
```csharp
// Владельцы регистрируют сами
public void AddPassive(IPassive passive)
{
    BasePassives.Add(passive);
    _triggerService.Register(passive);  // Низкая связанность
}

// Extension method для удобства
public static bool TriggerBeforeHit(this TriggerService service, 
    Figure attacker, Figure target, BeforeHitContext context)
{
    var triggerContext = TriggerContextBuilder.For(TriggerType.OnBeforeHit)
        .WithActor(attacker)
        .WithTarget(target)
        .WithData(context)
        .Build();
    
    return service.Execute(TriggerType.OnBeforeHit, triggerContext) 
        != TriggerResult.Cancel;
}
```
