# Trigger System — Полная Документация

**Версия:** 2.0  
**Уровень:** Commercial-grade (Slay the Spire / Monster Train)

---

## Содержание

1. [Обзор](#обзор)
2. [Архитектура](#архитектура)
3. [Основные компоненты](#основные-компоненты)
4. [Как добавить новый триггер](#как-добавить-новый-триггер)
5. [Как создать новую пассивку](#как-создать-новую-пассивку)
6. [Как создать артефакт с триггером](#как-создать-артефакт-с-триггером)
7. [Как создать статус-эффект с триггером](#как-создать-статус-эффект-с-триггером)
8. [Порядок исполнения](#порядок-исполнения)
9. [Защита от рекурсии](#защита-от-рекурсии)
10. [Отладка и трассировка](#отладка-и-трассировка)
11. [Лучшие практики](#лучшие-практики)
12. [Частые ошибки](#частые-ошибки)
13. [API Reference](#api-reference)

---

## Обзор

Trigger System — это система событий и реакций, которая управляет всеми эффектами в игре:
- Пассивные способности
- Артефакты
- Статус-эффекты (баффы/дебаффы)
- Эффекты окружения

**Ключевые возможности:**
- Детерминированный порядок исполнения
- Защита от бесконечных циклов
- Полная трассировка изменений
- Типобезопасные данные
- Производительность O(1)

---

## Архитектура

```
┌─────────────────────────────────────────────────────────────┐
│                    Trigger System                           │
├─────────────────────────────────────────────────────────────┤
│ Core (Project.Core.Core.Triggers)                           │
│ ├─ ITrigger              // Интерфейс всех триггеров        │
│ ├─ TriggerContext        // Контекст события                │
│ ├─ TriggerResult         // Результат исполнения            │
│ ├─ TriggerType           // Типы событий                    │
│ ├─ TriggerPhase          // Фазы внутри события             │
│ ├─ TriggerGroup          // Группы внутри фазы              │
│ ├─ TriggerPriorities     // Константы приоритетов           │
│ ├─ TriggerSource         // Источники событий               │
│ ├─ TriggerExecutor       // Исполнение с кэшированием       │
│ └─ TriggerContextBuilder // Fluent builder                  │
├─────────────────────────────────────────────────────────────┤
│ Service (Project.Gameplay.Gameplay.Combat)                  │
│ ├─ TriggerService        // Центральная регистрация         │
│ └─ TriggerServiceExtensions // Extension методы             │
├─────────────────────────────────────────────────────────────┤
│ Owners                                                        │
│ ├─ Figure                // Пассивки                        │
│ ├─ ArtifactService       // Артефакты                       │
│ └─ StatusEffectSystem  // Статус-эффекты                    │
└─────────────────────────────────────────────────────────────┘
```

---

## Основные компоненты

### ITrigger

Базовый интерфейс для всех триггеров:

```csharp
public interface ITrigger
{
    int Priority { get; }                    // Порядок исполнения
    TriggerGroup Group { get; }              // Группа внутри фазы
    TriggerPhase Phase { get; }              // Фаза события
    bool Matches(TriggerContext context);    // Быстрый фильтр
    TriggerResult Execute(TriggerContext context);
}
```

### TriggerContext

Контекст события с данными:

```csharp
public sealed class TriggerContext
{
    // Immutable (нельзя менять)
    public TriggerType Type { get; }
    public TriggerPhase Phase { get; }
    public TriggerSource SourceType { get; }
    public object? SourceObject { get; }
    public ITriggerEntity? Actor { get; }    // Типобезопасно!
    public ITriggerEntity? Target { get; }   // Типобезопасно!
    public int BaseValue { get; }

    // Mutable (можно менять)
    public int CurrentValue { get; set; }
    public object? Data { get; set; }

    // Trace
    public bool IsModified { get; }
    public int TotalDelta { get; }
    public string GetTraceString();
}

// ITriggerEntity реализуется Figure и другими сущностями
public interface ITriggerEntity
{
    string Id { get; }
}

// Пример использования без кастов:
public TriggerResult Execute(TriggerContext context)
{
    // ✅ Раньше: var attacker = context.Actor as Figure;
    // ✅ Теперь: прямой доступ!
    var attacker = context.Actor;  // Уже ITriggerEntity
    var target = context.Target;   // Уже ITriggerEntity
    
    // Если нужен Figure:
    if (context.Actor is Figure figure)
    {
        // Работа с Figure
    }
}
```

### TriggerResult

Результат исполнения триггера:

```csharp
public enum TriggerResult
{
    Continue = 0,  // Продолжить обработку
    Stop = 1,      // Остановить триггеры
    Cancel = 2,    // Отменить событие
    Replace = 3    // Заменить контекст
}
```

### TriggerService

Центральная регистрация и исполнение:

```csharp
public sealed class TriggerService : IDisposable
{
    public void Register(ITrigger trigger);
    public void Unregister(ITrigger trigger);
    public TriggerResult Execute(TriggerType type, TriggerPhase phase, TriggerContext context);
}
```

---

## Как добавить новый триггер

### Шаг 1: Создайте класс триггера

```csharp
using Project.Core.Core.Triggers;
using Project.Gameplay.Gameplay.Combat;
using Project.Gameplay.Gameplay.Combat.Contexts;

namespace Project.Gameplay.Gameplay.YourNamespace
{
    public class YourNewTrigger : ITrigger
    {
        public int Priority => TriggerPriorities.Normal;
        public TriggerGroup Group => TriggerGroup.Additive;
        public TriggerPhase Phase => TriggerPhase.ModifyCalculation;

        public bool Matches(TriggerContext context)
        {
            return context.Type == TriggerType.OnBeforeHit;
        }

        public TriggerResult Execute(TriggerContext context)
        {
            // Ваша логика здесь
            if (!context.TryGetData<BeforeHitContext>(out var hit))
                return TriggerResult.Continue;

            // Модификация урона
            context.ModifyValue(+10, "YourNewTrigger");

            return TriggerResult.Continue;
        }
    }
}
```

### Шаг 2: Зарегистрируйте триггер

```csharp
// В конструкторе владельца
public class YourOwner
{
    private readonly TriggerService _triggerService;

    public YourOwner(TriggerService triggerService)
    {
        _triggerService = triggerService;
        
        // Регистрация
        _triggerService.Register(new YourNewTrigger());
    }
}
```

### Шаг 3: Отпишите от регистрации (если нужно)

```csharp
public class YourOwner : IDisposable
{
    private readonly ITrigger _trigger;

    public YourOwner(TriggerService triggerService)
    {
        _trigger = new YourNewTrigger();
        triggerService.Register(_trigger);
    }

    public void Dispose()
    {
        _triggerService.Unregister(_trigger);
    }
}
```

---

## Как создать новую пассивку

### Пример: Пассивка с бонусным уроном

```csharp
using Project.Core.Core.Triggers;
using Project.Gameplay.Gameplay.Combat;
using Project.Gameplay.Gameplay.Combat.Contexts;
using Project.Gameplay.Gameplay.Figures;

namespace Project.Gameplay.Gameplay.Combat.Passives
{
    /// <summary>
    /// Пассивка: +10 урона когда HP ниже 50%
    /// </summary>
    public class LowHpDamagePassive : IPassive, IOnBeforeHit
    {
        public string Id { get; }
        public int Priority => TriggerPriorities.Normal;
        public TriggerGroup Group => TriggerGroup.Additive;
        public TriggerPhase Phase => TriggerPhase.ModifyCalculation;

        public LowHpDamagePassive(string id)
        {
            Id = id;
        }

        public bool Matches(TriggerContext context)
        {
            return context.Type == TriggerType.OnBeforeHit;
        }

        public TriggerResult Execute(TriggerContext context)
        {
            if (!context.TryGetData<BeforeHitContext>(out var hit))
                return TriggerResult.Continue;

            // Проверяем HP атакующего
            if (hit.Attacker.Stats.CurrentHp.Value / hit.Attacker.Stats.MaxHp <= 0.5f)
            {
                context.ModifyValue(+10, "LowHpDamagePassive");
            }

            return TriggerResult.Continue;
        }
    }
}
```

### Регистрация пассивки в Figure

```csharp
public class Figure
{
    private readonly TriggerService _triggerService;
    public List<IPassive> BasePassives { get; } = new();

    public void AddPassive(IPassive passive)
    {
        if (passive == null) return;
        
        BasePassives.Add(passive);
        _triggerService?.Register(passive);  // Авто-регистрация!
    }

    public void RemovePassive(IPassive passive)
    {
        if (BasePassives.Remove(passive))
        {
            _triggerService?.Unregister(passive);  // Авто-отписка!
        }
    }
}
```

---

## Как создать артефакт с триггером

### Пример: Артефакт с отражением урона

```csharp
using Project.Core.Core.Triggers;
using Project.Gameplay.Gameplay.Artifacts;
using Project.Gameplay.Gameplay.Combat.Contexts;

namespace Project.Gameplay.Gameplay.Artifacts.Effects
{
    /// <summary>
    /// Артефакт: Отражает 50% полученного урона обратно атакующему
    /// </summary>
    public class ThornArmorArtifact : ArtifactBase, IOnDamageReceived
    {
        private readonly float _reflectPercent;

        public ThornArmorArtifact(ArtifactConfig config) : base(config)
        {
            _reflectPercent = config.Effect.Value;
        }

        public override int Priority => TriggerPriorities.Normal;
        public override TriggerGroup Group => TriggerGroup.Additive;
        public override TriggerPhase Phase => TriggerPhase.AfterApplication;

        public override bool Matches(TriggerContext context)
        {
            // Игнорируем урон от других артефактов (защита от рекурсии!)
            if (context.SourceType == TriggerSource.Artifact)
                return false;
            
            return context.Type == TriggerType.OnDamageReceived;
        }

        public override TriggerResult Execute(TriggerContext context)
        {
            if (!context.TryGetData<DamageContext>(out var damage))
                return TriggerResult.Continue;

            // Отражаем урон
            int reflectDamage = (int)(context.Value * _reflectPercent);
            
            // Наносим урон атакующему с указанием источника!
            damage.Attacker.TakeDamage(
                reflectDamage,
                source: TriggerSource.Artifact,
                sourceObject: this);

            return TriggerResult.Continue;
        }
    }
}
```

### Регистрация артефакта

```csharp
public class ArtifactService : IDisposable
{
    private readonly TriggerService _triggerService;
    private readonly List<ArtifactInstance> _artifacts = new();

    public async UniTask<ArtifactInstance> Add(string configId, int stackCount = 1)
    {
        IArtifact artifact = await _factory.Create(configId);
        
        var instance = new ArtifactInstance(artifact, Guid.NewGuid().ToString());
        _artifacts.Add(instance);
        
        // Авто-регистрация триггера!
        _triggerService.Register(artifact);
        
        return instance;
    }

    public bool Remove(string instanceId)
    {
        var instance = Find(instanceId);
        if (instance == null) return false;

        // Авто-отписка триггера!
        _triggerService.Unregister(instance.Artifact);
        _artifacts.Remove(instance);
        
        return true;
    }

    public void Dispose()
    {
        Clear();
        _triggerService.Dispose();
    }
}
```

---

## Как создать статус-эффект с триггером

### Пример: Яд (DoT)

```csharp
using Project.Core.Core.Triggers;
using Project.Gameplay.Gameplay.Figures.StatusEffects;
using Project.Gameplay.Gameplay.Combat.Contexts;

namespace Project.Gameplay.Gameplay.Figures.StatusEffects
{
    /// <summary>
    /// Яд: Наносит 3 урона в начале хода
    /// </summary>
    public class PoisonEffect : StatusEffectBase, IOnTurnStart
    {
        private readonly int _damagePerTurn;

        public PoisonEffect(int damage, int turns = -1, int uses = -1) 
            : base(turns, uses)
        {
            _damagePerTurn = damage;
        }

        public override string Id => "poison";
        public override int Priority => TriggerPriorities.Normal;
        public override TriggerGroup Group => TriggerGroup.Additive;
        public override TriggerPhase Phase => TriggerPhase.OnTurnStart;

        public override bool Matches(TriggerContext context)
        {
            return context.Type == TriggerType.OnTurnStart;
        }

        public override TriggerResult Execute(TriggerContext context)
        {
            // Наносим урон владельцу эффекта
            // С указанием источника (важно для защиты от рекурсии!)
            _owner.TakeDamage(
                _damagePerTurn,
                source: TriggerSource.StatusEffect,
                sourceObject: this);

            return TriggerResult.Continue;
        }
    }
}
```

---

## Порядок исполнения

### Сортировка триггеров

```
1. Priority (ascending)     // -100 → -50 → 0 → 50 → 100
2. Group (ascending)        // Additive → Multiplicative → Reduction → Final
3. RegistrationOrder        // Кто раньше зарегистрирован
```

### Damage Pipeline

```
OnBeforeHit Event
│
├─ Phase: BeforeCalculation (1)
│   └─ Priority: Critical (-100)
│       └─ StunCheck.Cancel (отмена атаки)
│
├─ Phase: ModifyCalculation (2)
│   ├─ Priority: High (-50), Group: Multiplicative
│   │   └─ CriticalHit (x2)
│   ├─ Priority: Normal (0), Group: Additive
│   │   ├─ DamageBuff (+5)
│   │   └─ WeaponEnchant (+3)
│   ├─ Priority: Normal (0), Group: Multiplicative
│   │   └─ Vulnerability (x1.5)
│   └─ Priority: Normal (0), Group: Reduction
│       └─ ArmorReduction (-4)
│
├─ Phase: BeforeApplication (3)
│   └─ Priority: Low (50)
│       └─ ShieldAbsorb.Cancel (поглощение урона)
│
└─ Phase: AfterApplication (4)
    └─ Priority: Normal (0)
        ├─ Lifesteal (вампиризм)
        └─ Thorns (шипы)
```

### Пример расчёта

```
BaseValue = 10

ModifyCalculation Phase:
├─ DamageBuff (Additive):     10 + 5 = 15
├─ WeaponEnchant (Additive):  15 + 3 = 18
├─ CriticalHit (Multiplicative): 18 x 2 = 36
├─ Vulnerability (Multiplicative): 36 x 1.5 = 54
└─ ArmorReduction (Reduction): 54 - 4 = 50

Final Damage: 50
```

---

## Защита от рекурсии

### Проблема

```csharp
// ❌ БЕСКОНЕЧНЫЙ ЦИКЛ
public class ThornArmorArtifact : ArtifactBase
{
    public TriggerResult Execute(TriggerContext context)
    {
        target.TakeDamage(2);  // → OnDamage → этот же триггер → ...
        return TriggerResult.Continue;
    }
}
```

### Решение 1: Проверка SourceType

```csharp
// ✅ ПРАВИЛЬНО
public override bool Matches(TriggerContext context)
{
    // Игнорируем урон от артефактов
    if (context.SourceType == TriggerSource.Artifact)
        return false;
    
    return context.Type == TriggerType.OnDamageReceived;
}
```

### Решение 2: Проверка SourceObject

```csharp
// ✅ ПРАВИЛЬНО
public override TriggerResult Execute(TriggerContext context)
{
    // Игнорируем если это мы вызвали
    if (context.SourceObject == this)
        return TriggerResult.Continue;
    
    target.TakeDamage(2, source: TriggerSource.Artifact, sourceObject: this);
    return TriggerResult.Continue;
}
```

### Решение 3: Depth Limit

```csharp
// Автоматическая защита в TriggerExecutor
MaxTriggerDepth = 10;  // Превышение → Cancel

// Лог при превышении:
// "Trigger execution depth exceeded 10. Possible infinite loop. Aborting."
```

---

## Отладка и трассировка

### GetTraceString()

```csharp
var context = TriggerContext.Create(..., baseValue: 10);

context.ModifyValue(+5, "DamageBuff");
context.MultiplyValue(2.0f, "CriticalHit");
context.ModifyValue(-3, "ArmorReduction");

// Вывод для отладки
Console.WriteLine(context.GetTraceString());
```

**Вывод:**
```
BaseValue = 10
DamageBuff                +5 → 15
CriticalHit               x2.0 → 30
ArmorReduction            -3 → 27
```

### GetMutationLog()

```csharp
foreach (var record in context.GetMutationLog())
{
    Console.WriteLine($"[{record.Timestamp:HH:mm:ss}] {record.Source}: {record.Delta}");
}
```

### IsModified и TotalDelta

```csharp
if (context.IsModified)
{
    Console.WriteLine($"Value changed by {context.TotalDelta}");
    // Value changed by +17
}
```

---

## Лучшие практики

### ✅ DO

1. **Всегда указывай источник**
   ```csharp
   target.TakeDamage(5, source: TriggerSource.Artifact, sourceObject: this);
   ```

2. **Используй правильную группу**
   ```csharp
   public TriggerGroup Group => TriggerGroup.Additive;  // Для +N
   public TriggerGroup Group => TriggerGroup.Multiplicative;  // Для xN
   ```

3. **Проверяй SourceType для защиты от рекурсии**
   ```csharp
   if (context.SourceType == TriggerSource.Artifact)
       return TriggerResult.Continue;
   ```

4. **Логируй модификации**
   ```csharp
   context.ModifyValue(+5, "YourTriggerName");  // С именем!
   ```

5. **Используй Matches() для быстрой фильтрации**
   ```csharp
   public bool Matches(TriggerContext context) =>
       context.Type == TriggerType.OnBeforeHit;
   ```

### ❌ DON'T

1. **Не вызывай события без источника**
   ```csharp
   target.TakeDamage(5);  // ❌ Unknown source!
   ```

2. **Не игнорируй порядок групп**
   ```csharp
   public TriggerGroup Group => TriggerGroup.Default;  // ❌ Недетерминировано!
   ```

3. **Не меняй immutable поля**
   ```csharp
   context.Type = TriggerType.OnAfterHit;  // ❌ Не скомпилируется!
   ```

4. **Не логируй в production без необходимости**
   ```csharp
   #if UNITY_EDITOR
   Debug.Log(context.GetTraceString());
   #endif
   ```

---

## Частые ошибки

### Ошибка 1: Бесконечный цикл

```csharp
// ❌ ПЛОХО
public TriggerResult Execute(TriggerContext context)
{
    target.TakeDamage(5);  // → OnDamage → этот же триггер → ...
}

// ✅ ХОРОШО
public TriggerResult Execute(TriggerContext context)
{
    if (context.SourceType == TriggerSource.Artifact)
        return TriggerResult.Continue;
    
    target.TakeDamage(5, source: TriggerSource.Artifact, sourceObject: this);
}
```

### Ошибка 2: Неправильный порядок модификаторов

```csharp
// ❌ ПЛОХО: Все в Default группе
public TriggerGroup Group => TriggerGroup.Default;

// ✅ ХОРОШО: Явная группа
public TriggerGroup Group => TriggerGroup.Additive;
```

### Ошибка 3: Отсутствие имени источника

```csharp
// ❌ ПЛОХО
context.ModifyValue(+5, "");

// ✅ ХОРОШО
context.ModifyValue(+5, "DamageBuffPassive");
```

### Ошибка 4: Прямое изменение CurrentValue

```csharp
// ❌ ПЛОХО: Нет логирования
context.CurrentValue += 10;

// ✅ ХОРОШО: С логированием
context.ModifyValue(+10, "MyPassive");
```

---

## API Reference

### TriggerContext Builder

```csharp
// Создание контекста
var context = TriggerContextBuilder
    .For(TriggerType.OnBeforeHit, TriggerPhase.ModifyCalculation, TriggerSource.Combat)
    .WithActor(attacker)
    .WithTarget(target)
    .WithValue(10)
    .WithCustomData(new DamageInfo("fire", 5))
    .Build();

// Исполнение
var result = context.Execute(triggerService);
```

### TriggerService Extensions

```csharp
// BeforeHit
bool hitProceeds = triggerService.TriggerBeforeHit(attacker, target, context);

// AfterHit
triggerService.TriggerAfterHit(attacker, target, context);

// Kill/Death
triggerService.TriggerKill(killer, victim);
triggerService.TriggerDeath(victim, killer);

// Move
triggerService.TriggerMove(figure, moveContext);

// Turn
triggerService.TriggerTurnStart(figureRegistry, turnContext);
triggerService.TriggerTurnEnd(figureRegistry, turnContext);

// Battle
triggerService.TriggerBattleStart(figure);
triggerService.TriggerBattleEnd(figure);

// Damage Received
triggerService.TriggerDamageReceived(target, amount, source);
```

### Trigger Phases

```csharp
// Damage Pipeline
TriggerPhase.BeforeCalculation
TriggerPhase.ModifyCalculation
TriggerPhase.BeforeApplication
TriggerPhase.AfterApplication

// Attack Pipeline
TriggerPhase.BeforeDeclare
TriggerPhase.OnDeclare
TriggerPhase.AfterDeclare
TriggerPhase.BeforeHit
TriggerPhase.AfterHit

// Death Pipeline
TriggerPhase.BeforeDeath
TriggerPhase.OnDeath
TriggerPhase.AfterDeath

// Turn Pipeline
TriggerPhase.BeforeTurn
TriggerPhase.OnTurnStart
TriggerPhase.DuringTurn
TriggerPhase.OnTurnEnd
TriggerPhase.AfterTurn

// Movement Pipeline
TriggerPhase.BeforeMove
TriggerPhase.DuringMove
TriggerPhase.AfterMove
```

### Trigger Groups

```csharp
// Damage Modification
TriggerGroup.Additive       // +5, +10
TriggerGroup.Multiplicative // x2, x1.5
TriggerGroup.Reduction      // -3, -50%
TriggerGroup.Final          // min 1, max 999

// Generic
TriggerGroup.First
TriggerGroup.Early
TriggerGroup.Normal
TriggerGroup.Late
TriggerGroup.Last
```

### Trigger Priorities

```csharp
TriggerPriorities.Critical = -100  // Отмена смерти, воскрешение
TriggerPriorities.High = -50       // Модификация урона, уклонение
TriggerPriorities.Normal = 0       // Большинство эффектов
TriggerPriorities.Low = 50         // Баффы, щиты
TriggerPriorities.Cleanup = 100    // Награды, очистка
```

### Trigger Sources

```csharp
TriggerSource.Combat        // Боевое действие
TriggerSource.Artifact      // Эффект артефакта
TriggerSource.Passive       // Пассивная способность
TriggerSource.StatusEffect  // Бафф/дебафф
TriggerSource.Environment   // Окружение
TriggerSource.DirectDamage  // Прямой урон (DoT, отражение)
TriggerSource.Heal          // Лечение
TriggerSource.Custom        // Кастомный источник
```

---

## Примеры

### Полный пример: Критический удар

```csharp
using Project.Core.Core.Triggers;
using Project.Core.Core.Random;
using Project.Gameplay.Gameplay.Combat;
using Project.Gameplay.Gameplay.Combat.Contexts;

namespace Project.Gameplay.Gameplay.Combat.Passives
{
    /// <summary>
    /// Критический удар: 20% шанс удвоить урон
    /// </summary>
    public class CriticalHitPassive : IPassive, IOnBeforeHit
    {
        public string Id { get; }
        public int Priority => TriggerPriorities.High;
        public TriggerGroup Group => TriggerGroup.Multiplicative;
        public TriggerPhase Phase => TriggerPhase.ModifyCalculation;

        private readonly float _critChance;
        private readonly float _critMultiplier;
        private readonly IRandomService _random;

        public CriticalHitPassive(string id, float critChance, float critMultiplier, IRandomService random)
        {
            Id = id;
            _critChance = critChance;
            _critMultiplier = critMultiplier;
            _random = random;
        }

        public bool Matches(TriggerContext context) =>
            context.Type == TriggerType.OnBeforeHit;

        public TriggerResult Execute(TriggerContext context)
        {
            if (!context.TryGetData<BeforeHitContext>(out var hit))
                return TriggerResult.Continue;

            if (!_random.Chance(_critChance))
                return TriggerResult.Continue;

            hit.DamageMultiplier *= _critMultiplier;
            hit.IsCritical = true;

            context.MultiplyValue(_critMultiplier, "CriticalHitPassive");
            return TriggerResult.Continue;
        }
    }
}
```

### Полный пример: Щит артефакт

```csharp
using Project.Core.Core.Triggers;
using Project.Gameplay.Gameplay.Artifacts;

namespace Project.Gameplay.Gameplay.Artifacts.Effects
{
    /// <summary>
    /// Щит: Поглощает до 10 урона
    /// </summary>
    public class ShieldArtifact : ArtifactBase, IOnDamageReceived
    {
        private int _shieldCharge;
        private readonly int _maxCharge;

        public ShieldArtifact(ArtifactConfig config) : base(config)
        {
            _maxCharge = (int)config.Effect.Value;
            _shieldCharge = _maxCharge;
        }

        public override int Priority => TriggerPriorities.Low;
        public override TriggerGroup Group => TriggerGroup.Reduction;
        public override TriggerPhase Phase => TriggerPhase.BeforeApplication;

        public override bool Matches(TriggerContext context)
        {
            // Игнорируем урон от окружения и DoT
            if (context.SourceType == TriggerSource.Environment ||
                context.SourceType == TriggerSource.DirectDamage)
                return false;
            
            return context.Type == TriggerType.OnDamageReceived && _shieldCharge > 0;
        }

        public override TriggerResult Execute(TriggerContext context)
        {
            int damage = context.Value;
            int absorbed = Mathf.Min(damage, _shieldCharge);
            
            _shieldCharge -= absorbed;
            context.ModifyValue(-absorbed, "ShieldArtifact");

            if (_shieldCharge <= 0)
            {
                // Щит разрушен
                return TriggerResult.Stop;
            }

            return TriggerResult.Continue;
        }
    }
}
```

---

## Поддержка

При возникновении проблем:

1. Проверьте `GetTraceString()` для отладки
2. Убедитесь что `SourceType` указан правильно
3. Проверьте `Priority` и `Group` для порядка исполнения
4. Используйте `Matches()` для быстрой фильтрации

**Документация:**
- `TRIGGER_SYSTEM_COMPLETE.md` — Полная архитектура
- `TRIGGER_PHASE_SYSTEM.md` — Система фаз
- `TRIGGER_GROUP_SYSTEM.md` — Группы
- `TRIGGER_SOURCE.md` — Источники
- `TRIGGER_TRACE_SYSTEM.md` — Трассировка
- `TRIGGER_RECURSION_PROTECTION.md` — Защита от рекурсии
