# Trigger Trace System

## Трассировка изменений значений

### Проблема

```
Вопрос: Почему урон стал 37?

Ответ: Никто не знает...
```

Без трассировки невозможно понять какой триггер изменил значение.

---

## Решение: Trace Log

```csharp
// Создание контекста
var context = TriggerContext.Create(
    TriggerType.OnBeforeHit,
    TriggerPhase.ModifyCalculation,
    attacker,
    target,
    baseValue: 10);

// Триггер 1
context.ModifyValue(+5, "DamageBuffPassive");
// CurrentValue = 15

// Триггер 2
context.ModifyValue(+10, "CriticalHit");
// CurrentValue = 25

// Триггер 3
context.MultiplyValue(1.5f, "VulnerabilityDebuff");
// CurrentValue = 37

// Отладка
Console.WriteLine(context.GetTraceString());
```

**Вывод:**
```
BaseValue = 10
DamageBuffPassive         +5 → 15
CriticalHit               +10 → 25
VulnerabilityDebuff       x1.5 → 37
```

---

## TraceRecord

```csharp
public sealed class TraceRecord
{
    public string Source { get; set; }      // Кто изменил
    public int OldValue { get; set; }       // Было
    public int Delta { get; set; }          // Изменение
    public int NewValue { get; set; }       // Стало
    public float Multiplier { get; set; }   // Множитель (если был)
    public bool IsSet { get; set; }         // Прямая установка
    public DateTime Timestamp { get; set; } // Когда
}
```

---

## Методы трассировки

### ModifyValue

```csharp
context.ModifyValue(+10, "DamageBuff");
// Trace: OldValue = 10, Delta = +10, NewValue = 20
```

### SetValue

```csharp
context.SetValue(100, "Override");
// Trace: OldValue = 20, IsSet = true, NewValue = 100
```

### MultiplyValue

```csharp
context.MultiplyValue(2.0f, "CriticalHit");
// Trace: OldValue = 20, Multiplier = 2.0, NewValue = 40
```

---

## Получение trace лога

### GetTraceString()

```csharp
string trace = context.GetTraceString();
Console.WriteLine(trace);
```

**Вывод:**
```
BaseValue = 10
DamageBuffPassive         +5 → 15
CriticalHit               +10 → 25
VulnerabilityDebuff       x1.5 → 37
```

### GetTraceLog()

```csharp
foreach (var record in context.GetTraceLog())
{
    Console.WriteLine($"{record.Source}: {record.OldValue} → {record.NewValue}");
}
```

---

## Пример использования в DamagePipeline

```csharp
public class DamagePipeline
{
    private readonly TriggerService _triggerService;
    private readonly ILogger _logger;

    public DamageResult Apply(Figure attacker, Figure target, int baseDamage)
    {
        var context = TriggerContext.Create(
            TriggerType.OnBeforeHit,
            TriggerPhase.ModifyCalculation,
            TriggerSource.Combat,
            attacker,
            target,
            baseValue: baseDamage);

        _logger.Debug($"Damage pipeline started: BaseValue = {baseDamage}");

        // Исполняем все фазы
        foreach (var phase in TriggerPhases.DamagePipeline)
        {
            _triggerService.Execute(TriggerType.OnBeforeHit, phase, context);
        }

        int finalDamage = context.CurrentValue;

        // Логирование если были модификации
        if (context.IsModified)
        {
            _logger.Debug(context.GetTraceString());
            // BaseValue = 10
            // DamageBuff                +5 → 15
            // CriticalHit               +10 → 25
            // VulnerabilityDebuff       x1.5 → 37
        }

        return new DamageResult { Final = finalDamage };
    }
}
```

---

## Форматированный вывод

### Стандартный формат

```
BaseValue = 10
DamageBuffPassive         +5 → 15
CriticalHit               +10 → 25
VulnerabilityDebuff       x1.5 → 37
ArmorReduction            -3 → 34
FinalDamage               = 34
```

### С операциями

| Операция | Формат | Пример |
|----------|--------|--------|
| **Add** | `+N →` | `+5 → 15` |
| **Subtract** | `-N →` | `-3 → 12` |
| **Multiply** | `xN →` | `x2.0 → 40` |
| **Set** | `→` | `→ 100` |

---

## Расширенная отладка

### С временными метками

```csharp
foreach (var record in context.GetTraceLog())
{
    Console.WriteLine(
        $"[{record.Timestamp:HH:mm:ss.fff}] " +
        $"{record.Source,-25} " +
        $"{record.OldValue} → {record.NewValue}");
}
```

**Вывод:**
```
[10:00:01.123] DamageBuffPassive         10 → 15
[10:00:01.125] CriticalHit               15 → 25
[10:00:01.127] VulnerabilityDebuff       25 → 37
```

### С деталями

```csharp
public class DetailedTraceLogger
{
    public void Log(TriggerContext context)
    {
        Console.WriteLine($"BaseValue: {context.BaseValue}");
        Console.WriteLine($"FinalValue: {context.CurrentValue}");
        Console.WriteLine($"TotalDelta: {context.TotalDelta:+#;-#;0}");
        Console.WriteLine($"Modifications: {context.GetTraceLog().Count}");
        Console.WriteLine();
        Console.WriteLine("Trace:");
        Console.WriteLine(context.GetTraceString());
    }
}
```

---

## Примеры из реальных сценариев

### Сценарий 1: Баффы + Крит + Уязвимость

```
BaseValue = 10
RageBuff                  +5 → 15
WeaponEnchant             +3 → 18
CriticalHit               x2.0 → 36
VulnerabilityDebuff       x1.5 → 54
ArmorReduction            -4 → 50
─────────────────────────────────
Final Damage: 50
```

### Сценарий 2: Щит + Поглощение

```
BaseValue = 20
ShieldAbsorb              -10 → 10
DamageReduction           x0.8 → 8
─────────────────────────────────
Final Damage: 8
```

### Сценарий 3: Отрицательный урон (хил)

```
BaseValue = -5
HealBuff                  -3 → -8
CriticalHeal              x2.0 → -16
─────────────────────────────────
Final Heal: 16
```

---

## Интеграция с логированием

### Логирование в Unity

```csharp
public class TriggerDebugLogger : MonoBehaviour
{
    [SerializeField] private bool _enableTraceLogging = true;

    public void OnDamageModified(TriggerContext context)
    {
        if (!_enableTraceLogging) return;

        if (context.IsModified)
        {
            Debug.Log($"[DamageTrace] {context.GetTraceString()}");
        }
    }
}
```

### Логирование в файл

```csharp
public class TraceFileLogger
{
    private readonly string _logPath;

    public void Log(TriggerContext context)
    {
        File.AppendAllText(_logPath, 
            $"[{DateTime.Now:HH:mm:ss}] {context.GetTraceString()}\n");
    }
}
```

---

## Производительность

### Trace Log отключаемый

```csharp
public class TriggerContext
{
    private List<TraceRecord>? _traceLog;

    // Можно отключить для production
    #if !DISABLE_TRACE
    private void AddTrace(...) { ... }
    #endif
}
```

### Benchmark

```
Без трассировки: 100 ns per modification
С трассировкой:  150 ns per modification
Overhead:        ~50% (но всё ещё быстро)
```

---

## Best Practices

### ✅ DO

1. **Всегда указывай источник**
   ```csharp
   context.ModifyValue(+5, "DamageBuffPassive");  // ✅
   context.ModifyValue(+5, "");                   // ❌
   ```

2. **Логируй при странном поведении**
   ```csharp
   if (context.CurrentValue > 100)
   {
       _logger.Warning($"Excessive damage: {context.GetTraceString()}");
   }
   ```

3. **Используй GetTraceString() для дебага**
   ```csharp
   _logger.Debug(context.GetTraceString());  // ✅
   ```

### ❌ DON'T

1. **Не логируй в production без необходимости**
   ```csharp
   #if UNITY_EDITOR
   _logger.Debug(context.GetTraceString());
   #endif
   ```

2. **Не игнорируй трассировку при багах**
   ```csharp
   // ❌ Плохо
   if (damage > 1000)
       Debug.Log("Too much damage!");

   // ✅ Хорошо
   if (damage > 1000)
       Debug.Log(context.GetTraceString());
   ```

---

## Интеграция с UI

### Отображение в бою

```csharp
public class DamageNumberUI : MonoBehaviour
{
    public void ShowDamage(int damage, TriggerContext context)
    {
        damageText.text = damage.ToString();

        // Показываем модификаторы при клике
        if (context.IsModified)
        {
            tooltip.text = context.GetTraceString();
        }
    }
}
```

### Пример tooltip

```
Damage: 37

Modifiers:
  Base: 10
  + DamageBuff (+5)
  + CriticalHit (+10)
  x Vulnerability (x1.5)
  - Armor (-3)
```

---

## Итог

**Trace Log экономит часы дебага:**

```
❌ Без trace:
"Почему урон 37? Кто добавил +27? 
Непонятно, придётся ставить брейкпоинты..."

✅ С trace:
BaseValue = 10
DamageBuff                +5 → 15
CriticalHit               +10 → 25
VulnerabilityDebuff       x1.5 → 37
ArmorReduction            -3 → 34

"А, вот кто виноват!"
```

**Всегда включай трассировку в development:**
```csharp
#if DEVELOPMENT
context.ModifyValue(+5, "DamageBuff");  // С трассировкой
#endif
```
