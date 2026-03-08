# Trigger Execution Order

## Детерминированный порядок исполнения

### Проблема

```csharp
// Два триггера с одинаковыми Priority и Phase
public class DamageBuffA : IPassive
{
    public int Priority => 0;
    public TriggerPhase Phase => TriggerPhase.ModifyCalculation;
}

public class DamageBuffB : IPassive
{
    public int Priority => 0;
    public TriggerPhase Phase => TriggerPhase.ModifyCalculation;
}

// Какой выполнится первым?
```

**Без RegistrationOrder:** Недетерминировано (зависит от порядка в списке)  
**С RegistrationOrder:** Детерминировано (кто раньше зарегистрирован)

---

## Алгоритм сортировки

```csharp
kvp.Value.Sort((a, b) =>
{
    // 1. Priority (lower = first)
    int priorityCompare = a.Trigger.Priority.CompareTo(b.Trigger.Priority);
    if (priorityCompare != 0)
        return priorityCompare;

    // 2. RegistrationOrder (earlier = first)
    return a.RegistrationOrder.CompareTo(b.RegistrationOrder);
});
```

### Порядок

```
┌──────────────────────────────────────────────────────────────┐
│ Сортировка триггеров в одной фазе                            │
├──────────────────────────────────────────────────────────────┤
│ 1. Priority (ascending)                                      │
│    ├─ Critical (-100)                                        │
│    ├─ High (-50)                                             │
│    ├─ Normal (0)                                             │
│    ├─ Low (50)                                               │
│    └─ Cleanup (100)                                          │
│                                                              │
│ 2. RegistrationOrder (ascending)                             │
│    ├─ RegistrationOrder = 1 (первый)                         │
│    ├─ RegistrationOrder = 2 (второй)                         │
│    └─ RegistrationOrder = N (последний)                      │
└──────────────────────────────────────────────────────────────┘
```

---

## Пример

### Регистрация

```csharp
// В начале игры
triggerService.Register(new CriticalPassive());      // RegistrationOrder = 1
triggerService.Register(new DamageBuffPassive());    // RegistrationOrder = 2
triggerService.Register(new VulnerabilityDebuff());  // RegistrationOrder = 3

// Позже (например, после получения артефакта)
triggerService.Register(new ArtifactDamageBuff());   // RegistrationOrder = 4
```

### Исполнение (ModifyCalculation фаза)

```
ModifyCalculation Phase
│
├─ Priority: High (-50)
│   └─ 1. CriticalPassive (RegistrationOrder = 1)
│
├─ Priority: Normal (0)
│   ├─ 2. DamageBuffPassive (RegistrationOrder = 2)
│   ├─ 3. VulnerabilityDebuff (RegistrationOrder = 3)
│   └─ 4. ArtifactDamageBuff (RegistrationOrder = 4)
│
└─ Priority: Low (50)
    └─ (нет триггеров)
```

---

## Global Registration Counter

```csharp
public sealed class TriggerExecutor<T>
{
    private static int _globalRegistrationCounter;
    
    private void CacheTrigger(T source, ITrigger trigger)
    {
        // Атомарное увеличение глобального счётчика
        int registrationOrder = Interlocked.Increment(ref _globalRegistrationCounter);
        _triggerMap[key].Add(new TriggerEntry(source, trigger, registrationOrder));
    }
}
```

### Почему `static`?

- **Глобальный порядок** для всех `TriggerExecutor<T>`
- **Детерминированный** даже при создании новых экземпляров
- **Thread-safe** через `Interlocked.Increment`

---

## Сценарии

### Сценарий 1: Одинаковый Priority

```csharp
// Регистрация
triggerService.Register(new DamageBuffA());  // Order = 1, Priority = 0
triggerService.Register(new DamageBuffB());  // Order = 2, Priority = 0

// Порядок исполнения
1. DamageBuffA (Priority = 0, Order = 1) ← Первый
2. DamageBuffB (Priority = 0, Order = 2)
```

### Сценарий 2: Разный Priority

```csharp
// Регистрация
triggerService.Register(new DamageBuff());     // Order = 1, Priority = 0
triggerService.Register(new CriticalHit());    // Order = 2, Priority = -50

// Порядок исполнения (сортировка!)
1. CriticalHit (Priority = -50, Order = 2) ← Первый (ниже приоритет)
2. DamageBuff (Priority = 0, Order = 1)
```

### Сценарий 3: Поздняя регистрация

```csharp
// Начало игры
triggerService.Register(new Passive1());  // Order = 1

// Игрок получает артефакт
triggerService.Register(new ArtifactPassive());  // Order = 2

// Порядок исполнения
1. Passive1 (Order = 1) ← Первый
2. ArtifactPassive (Order = 2)
```

---

## Важные замечания

### ⚠️ RegistrationOrder присваивается при кэшировании

```csharp
// Регистрация (Order ещё не присвоен)
triggerService.Register(new MyPassive());

// Первое исполнение → RebuildCache → присваивается Order
triggerService.Execute(...);  // ← Здесь MyPassive получает Order

// Последующие исполнения используют закэшированный Order
triggerService.Execute(...);  // ← Использует кэш
```

### ⚠️ InvalidateCache сбрасывает кэш

```csharp
// Если триггеры изменились
_executor.InvalidateCache();

// Следующее исполнение → новый RebuildCache → новые Order
// Но Order будут продолжены от глобального счётчика!
triggerService.Execute(...);  // ← Новые Order, но глобально уникальные
```

---

## Логирование

### Debug Log

```
[TriggerExecutor] Trigger cache rebuilt: 3 trigger type/phase combinations
[TriggerExecutor] OnBeforeHit/ModifyCalculation:
  - CriticalPassive (Priority=-50, Order=1)
  - DamageBuffPassive (Priority=0, Order=2)
  - VulnerabilityDebuff (Priority=0, Order=3)
```

### Отладка порядка

```csharp
public class DebugPassive : IPassive
{
    public int Priority => 0;
    public TriggerPhase Phase => TriggerPhase.ModifyCalculation;
    
    public TriggerResult Execute(TriggerContext context)
    {
        _logger.Debug($"DebugPassive executed at depth={context.Depth}");
        return TriggerResult.Continue;
    }
}

// Лог:
// [Debug] DebugPassive executed at depth=2
```

---

## Best Practices

### ✅ DO

1. **Регистрируй триггеры в предсказуемом порядке**
   ```csharp
   // В начале игры
   foreach (var passive in defaultPassives)
       triggerService.Register(passive);
   ```

2. **Используй Priority для критичного порядка**
   ```csharp
   // Крит всегда раньше баффов
   public class CriticalPassive : IPassive
   {
       public int Priority => TriggerPriorities.High; // -50
   }
   ```

3. **Документируй зависимость от порядка**
   ```csharp
   /// <summary>
   /// Must execute after CriticalPassive.
   /// Priority = 0, expects to be second in ModifyCalculation.
   /// </summary>
   public class DamageMultiplierPassive : IPassive { }
   ```

### ❌ DON'T

1. **Не полагайся на порядок регистрации в runtime**
   ```csharp
   // ❌ Плохо: порядок зависит от когда игрок получил артефакт
   if (player.HasArtifact)
       triggerService.Register(new ArtifactPassive());
   ```

2. **Не используй одинаковый Priority для зависимых триггеров**
   ```csharp
   // ❌ Плохо: порядок зависит от RegistrationOrder
   public class BuffA : IPassive { public int Priority => 0; }
   public class BuffB : IPassive { public int Priority => 0; }
   
   // ✅ Хорошо: явный приоритет
   public class BuffA : IPassive { public int Priority => -10; }
   public class BuffB : IPassive { public int Priority => 0; }
   ```

---

## Тестирование

### Unit Test

```csharp
[Test]
public void ExecutionOrder_ShouldBeDeterministic()
{
    var executor = new TriggerExecutor<ITrigger>(...);
    var results = new List<string>();
    
    // Регистрация в известном порядке
    executor.Register(new TriggerA(() => results.Add("A")));
    executor.Register(new TriggerB(() => results.Add("B")));
    executor.Register(new TriggerC(() => results.Add("C")));
    
    // Исполнение
    executor.Execute(TriggerType.OnBeforeHit, context);
    
    // Проверка детерминированного порядка
    Assert.AreEqual(new[] { "A", "B", "C" }, results);
}

[Test]
public void Priority_ShouldOverrideRegistrationOrder()
{
    var executor = new TriggerExecutor<ITrigger>(...);
    var results = new List<string>();
    
    // Регистрация: сначала низкий приоритет
    executor.Register(new LowPriority(() => results.Add("Low")));
    // Потом высокий приоритет
    executor.Register(new HighPriority(() => results.Add("High")));
    
    executor.Execute(TriggerType.OnBeforeHit, context);
    
    // Высокий приоритет выполняется первым, несмотря на RegistrationOrder
    Assert.AreEqual(new[] { "High", "Low" }, results);
}
```

---

## Таблица порядка

| Триггер 1 | Триггер 2 | Порядок исполнения |
|-----------|-----------|-------------------|
| Priority = -50 | Priority = 0 | Триггер 1 → Триггер 2 |
| Priority = 0, Order = 1 | Priority = 0, Order = 2 | Триггер 1 → Триггер 2 |
| Priority = 0, Order = 5 | Priority = 0, Order = 3 | Триггер 2 → Триггер 1 |
| Priority = 100 | Priority = -100 | Триггер 2 → Триггер 1 |

---

## Итог

**Детерминированный порядок = Priority + RegistrationOrder**

```
Сортировка:
1. Priority (ascending)    // Явный контроль разработчика
2. RegistrationOrder       // Гарантированный tie-breaker
```

**Результат:**
- ✅ Предсказуемое исполнение
- ✅ Воспроизводимые баги
- ✅ Тестируемый порядок
- ✅ Нет зависимости от порядка в списке
