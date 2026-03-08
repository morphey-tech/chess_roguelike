# TriggerContext: ReadOnly vs Mutable

## Разделение на immutable и mutable данные

### Проблема

```csharp
// ❌ ПЛОХО: Один контекст, все мутируют
var context = new TriggerContext { Value = 10 };

// Artifact A
context.Value += 5;  // 15

// Artifact B (ожидает оригинальное значение)
if (context.Value == 10)  // ❌ false! Баг!
{
    // ...
}

// Artifact C
context.Value *= 2;  // 30

// Итог: кто что изменил? непонятно
```

### Решение

```csharp
// ✅ ХОРОШО: BaseValue (immutable) + CurrentValue (mutable)
var context = TriggerContext.Create(
    TriggerType.OnBeforeHit,
    TriggerPhase.ModifyCalculation,
    attacker,
    target,
    baseValue: 10);

// Artifact A
context.ModifyValue(+5, "DamageBuff");
// BaseValue = 10, CurrentValue = 15

// Artifact B (читает оригинальное)
if (context.BaseValue == 10)  // ✅ true!
{
    // Работает с оригинальным значением
}

// Artifact C
context.ModifyValue(+10, "CriticalHit");
// BaseValue = 10, CurrentValue = 25

// Debug log:
// DamageBuff: +5 → 15
// CriticalHit: +10 → 25
// TotalDelta: +15
```

---

## Структура TriggerContext

```
┌─────────────────────────────────────────────────────────────┐
│                    TriggerContext                           │
├─────────────────────────────────────────────────────────────┤
│ Immutable Core (Read-Only)                                  │
│ ├─ Type : TriggerType         // Что происходит            │
│ ├─ Phase : TriggerPhase       // Когда в событии           │
│ ├─ Actor : object?            // Кто инициировал           │
│ ├─ Target : object?           // Цель                      │
│ └─ BaseValue : int            // Оригинальное значение     │
├─────────────────────────────────────────────────────────────┤
│ Mutable Data                                                  │
│ ├─ CurrentValue : int         // Текущее (модифицированное) │
│ ├─ Data : object?             // Дополнительные данные      │
│ └─ _mutationLog : List        // Лог изменений             │
├─────────────────────────────────────────────────────────────┤
│ Properties                                                    │
│ ├─ IsModified : bool          // Были ли изменения          │
│ └─ TotalDelta : int           // Сумма всех изменений       │
└─────────────────────────────────────────────────────────────┘
```

---

## Пример использования

### Damage Pipeline

```csharp
public class DamagePipeline
{
    public DamageResult Apply(Figure attacker, Figure target, int baseDamage)
    {
        var context = TriggerContext.Create(
            TriggerType.OnBeforeHit,
            TriggerPhase.ModifyCalculation,
            attacker,
            target,
            baseValue: baseDamage);

        // Исполняем триггеры
        foreach (var phase in TriggerPhases.DamagePipeline)
        {
            _triggerService.Execute(TriggerType.OnBeforeHit, phase, context);
        }

        // Финальный урон
        int finalDamage = context.CurrentValue;
        
        // Debug info
        if (context.IsModified)
        {
            _logger.Debug($"Damage modified: {context.BaseValue} → {finalDamage}");
            foreach (var mutation in context.GetMutationLog())
            {
                _logger.Debug($"  {mutation}");
            }
        }
        
        return new DamageResult { Final = finalDamage };
    }
}
```

### Триггеры

```csharp
// Бафф урона
public class DamageBuffPassive : IPassive
{
    public TriggerPhase Phase => TriggerPhase.ModifyCalculation;
    
    public TriggerResult Execute(TriggerContext context)
    {
        // ✅ Модифицируем с логированием
        context.ModifyValue(+10, "DamageBuffPassive");
        return TriggerResult.Continue;
    }
}

// Критический удар
public class CriticalPassive : IPassive
{
    public TriggerPhase Phase => TriggerPhase.ModifyCalculation;
    
    public TriggerResult Execute(TriggerContext context)
    {
        if (!_random.Chance(_critChance))
            return TriggerResult.Continue;

        // ✅ Умножаем текущее значение
        context.SetValue(context.CurrentValue * 2, "CriticalHit");
        return TriggerResult.Continue;
    }
}

// Проверка оригинального урона
public class LowDamageBonus : IPassive
{
    public TriggerPhase Phase => TriggerPhase.ModifyCalculation;
    
    public TriggerResult Execute(TriggerContext context)
    {
        // ✅ Читаем BaseValue (оригинальное, без модификаторов)
        if (context.BaseValue <= 5)
        {
            context.ModifyValue(+5, "LowDamageBonus");
        }
        return TriggerResult.Continue;
    }
}
```

---

## Mutation Log

### Формат записи

```csharp
public sealed class MutationRecord
{
    public string Source { get; set; }      // Кто изменил
    public int Delta { get; set; }          // На сколько
    public int NewValue { get; set; }       // Новое значение
    public DateTime Timestamp { get; set; } // Когда
}

// ToString()
"DamageBuff: +10 → 20"
"CriticalHit: +20 → 40"
"Vulnerability: +5 → 45"
```

### Пример лога

```
Damage Pipeline Debug:
  BaseValue: 10
  CurrentValue: 45
  TotalDelta: +35
  
  Mutations:
    [10:00:01] DamageBuffPassive: +10 → 20
    [10:00:01] CriticalHit: +20 → 40
    [10:00:01] VulnerabilityDebuff: +5 → 45
```

---

## Свойства для отладки

### IsModified

```csharp
if (context.IsModified)
{
    _logger.Debug($"Value was modified from {context.BaseValue}");
}
else
{
    _logger.Debug($"Value unchanged: {context.BaseValue}");
}
```

### TotalDelta

```csharp
int original = context.BaseValue;
int modified = context.CurrentValue;
int delta = context.TotalDelta;  // modified - original

_logger.Debug($"Damage: {original} + {delta} = {modified}");
```

---

## Методы модификации

### ModifyValue (рекомендуется)

```csharp
// Добавляет к текущему значению
context.ModifyValue(+10, "DamageBuff");
// CurrentValue += 10
```

### SetValue (осторожно)

```csharp
// Переписывает текущее значение
context.SetValue(100, "OverridePassive");
// CurrentValue = 100
```

### ResetValue

```csharp
// Сбрасывает к оригинальному
context.ResetValue();
// CurrentValue = BaseValue
// (лог не очищается!)
```

---

## Best Practices

### ✅ DO

1. **Используй BaseValue для проверок**
   ```csharp
   if (context.BaseValue <= 5)  // Оригинальное значение
   {
       context.ModifyValue(+5, "LowDamageBonus");
   }
   ```

2. **Всегда указывай источник модификации**
   ```csharp
   context.ModifyValue(+10, "MyPassiveName");  // ✅
   context.ModifyValue(+10, "");               // ❌
   ```

3. **Логируй изменения для отладки**
   ```csharp
   if (context.IsModified)
   {
       foreach (var record in context.GetMutationLog())
       {
           _logger.Debug(record.ToString());
       }
   }
   ```

### ❌ DON'T

1. **Не меняй CurrentValue напрямую**
   ```csharp
   // ❌ Плохо: нет логирования
   context.CurrentValue += 10;
   
   // ✅ Хорошо: с логированием
   context.ModifyValue(+10, "MyPassive");
   ```

2. **Не используй SetValue без необходимости**
   ```csharp
   // ❌ Переписывает все предыдущие модификации
   context.SetValue(100, "Override");
   
   // ✅ Добавляет к существующему
   context.ModifyValue(+100, "Bonus");
   ```

3. **Не игнорируй TotalDelta**
   ```csharp
   // Проверяй итоговое изменение
   if (context.TotalDelta > 100)
   {
       _logger.Warning($"Excessive damage modification: {context.TotalDelta}");
   }
   ```

---

## Отладка

### Визуализация цепочки модификаций

```csharp
public void DebugContext(TriggerContext context)
{
    Console.WriteLine($"BaseValue: {context.BaseValue}");
    Console.WriteLine($"CurrentValue: {context.CurrentValue}");
    Console.WriteLine($"TotalDelta: {context.TotalDelta:+#;-#;0}");
    Console.WriteLine($"IsModified: {context.IsModified}");
    
    if (context.IsModified)
    {
        Console.WriteLine("Mutations:");
        foreach (var record in context.GetMutationLog())
        {
            Console.WriteLine($"  {record.Timestamp:HH:mm:ss} {record}");
        }
    }
}
```

### Пример вывода

```
BaseValue: 10
CurrentValue: 45
TotalDelta: +35
IsModified: True

Mutations:
  10:00:01 DamageBuffPassive: +10 → 20
  10:00:02 CriticalHit: +20 → 40
  10:00:03 VulnerabilityDebuff: +5 → 45
```

---

## Сравнение: До и После

### До (без разделения)

```csharp
var context = new TriggerContext { Value = 10 };

trigger1.Execute(context);  // Value = 15
trigger2.Execute(context);  // Value = 30
trigger3.Execute(context);  // Value = 25 (???)

// Кто изменил? Когда? Почему?
// Непонятно!
```

### После (с разделением)

```csharp
var context = TriggerContext.Create(..., baseValue: 10);

trigger1.Execute(context);  // CurrentValue = 15
trigger2.Execute(context);  // CurrentValue = 30
trigger3.Execute(context);  // CurrentValue = 25

// BaseValue = 10 (всегда!)
// TotalDelta = +15
// MutationLog:
//   trigger1: +5 → 15
//   trigger2: +15 → 30
//   trigger3: -5 → 25

// Всё прозрачно!
```

---

## Итог

| Поле | Изменяемость | Назначение |
|------|-------------|------------|
| `Type` | ❌ Immutable | Что происходит |
| `Phase` | ❌ Immutable | Когда в событии |
| `Actor` | ❌ Immutable | Кто инициировал |
| `Target` | ❌ Immutable | Цель |
| `BaseValue` | ❌ Immutable | Оригинальное значение |
| `CurrentValue` | ✅ Mutable | Текущее (модифицированное) |
| `Data` | ✅ Mutable | Дополнительные данные |

**Преимущества:**
- ✅ Всегда известно оригинальное значение
- ✅ Полная история модификаций
- ✅ Легкая отладка
- ✅ Предсказуемое поведение
- ✅ Нет неожиданных сайд-эффектов
