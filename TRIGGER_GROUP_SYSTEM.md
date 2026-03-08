# Trigger Group System

## Группы внутри фаз

### Проблема порядка модификаторов

```
Базовый урон: 10

Вариант 1 (неправильный порядок):
+5 (Additive)     → 15
-3 (Reduction)    → 12
x2 (Multiplicative) → 24  ❌

Вариант 2 (правильный порядок):
+5 (Additive)      → 15
x2 (Multiplicative) → 30
-3 (Reduction)     → 27  ✅
```

**Результат разный!** Поэтому нужен контроль порядка.

---

## TriggerGroup

```csharp
public enum TriggerGroup
{
    // Damage Modification Pipeline
    Additive = 1,      // +5 damage, +10 armor
    Multiplicative = 2, // x2 damage, x1.5 armor
    Reduction = 3,     // -3 damage, -50% damage
    Final = 4,         // min 1, max 999

    // Generic Pipeline
    First = 10,
    Early = 20,
    Normal = 30,
    Late = 40,
    Last = 50
}
```

---

## Порядок сортировки

```
Сортировка триггеров:
1. Priority (ascending)     // -100, -50, 0, 50, 100
2. Group (ascending)        // Additive → Multiplicative → Reduction → Final
3. RegistrationOrder        // Кто раньше зарегистрирован
```

### Пример

```
ModifyCalculation Phase, Priority = 0:
├─ Group: Additive (1)
│   ├─ DamageBuff (+5)         [Order = 1]
│   └─ WeaponEnchant (+3)      [Order = 2]
├─ Group: Multiplicative (2)
│   ├─ CriticalHit (x2)        [Order = 3]
│   └─ Vulnerability (x1.5)    [Order = 4]
├─ Group: Reduction (3)
│   └─ ArmorReduction (-3)     [Order = 5]
└─ Group: Final (4)
    └─ DamageClamp (min 1)     [Order = 6]
```

---

## Примеры использования

### Additive Group

```csharp
public class DamageBuffPassive : IPassive
{
    public int Priority => TriggerPriorities.Normal;
    public TriggerGroup Group => TriggerGroup.Additive;
    public TriggerPhase Phase => TriggerPhase.ModifyCalculation;

    public TriggerResult Execute(TriggerContext context)
    {
        // ✅ Добавляем плоский бонус
        context.ModifyValue(+10, "DamageBuffPassive");
        return TriggerResult.Continue;
    }
}
```

### Multiplicative Group

```csharp
public class CriticalHitPassive : IPassive
{
    public int Priority => TriggerPriorities.High;
    public TriggerGroup Group => TriggerGroup.Multiplicative;
    public TriggerPhase Phase => TriggerPhase.ModifyCalculation;

    public TriggerResult Execute(TriggerContext context)
    {
        if (!_random.Chance(_critChance))
            return TriggerResult.Continue;

        // ✅ Умножаем текущее значение
        context.MultiplyValue(2.0f, "CriticalHit");
        return TriggerResult.Continue;
    }
}
```

### Reduction Group

```csharp
public class ArmorReductionPassive : IPassive
{
    public int Priority => TriggerPriorities.Normal;
    public TriggerGroup Group => TriggerGroup.Reduction;
    public TriggerPhase Phase => TriggerPhase.ModifyCalculation;

    public TriggerResult Execute(TriggerContext context)
    {
        // ✅ Вычитаем из текущего значения
        context.ModifyValue(-context.GetCustomData<ArmorValue>()?.Value ?? 0, "ArmorReduction");
        return TriggerResult.Continue;
    }
}
```

### Final Group

```csharp
public class DamageClampPassive : IPassive
{
    public int Priority => TriggerPriorities.Low;
    public TriggerGroup Group => TriggerGroup.Final;
    public TriggerPhase Phase => TriggerPhase.ModifyCalculation;

    public TriggerResult Execute(TriggerContext context)
    {
        // ✅ Финальная обработка (min/max)
        context.SetValue(Math.Clamp(context.CurrentValue, 1, 999), "DamageClamp");
        return TriggerResult.Continue;
    }
}
```

---

## Damage Modification Pipeline

```csharp
public class DamagePipeline
{
    public DamageResult Apply(Figure attacker, Figure target, int baseDamage)
    {
        var context = TriggerContext.Create(
            TriggerType.OnBeforeHit,
            TriggerPhase.ModifyCalculation,
            TriggerSource.Combat,
            attacker,
            target,
            baseValue: baseDamage);

        // Исполняем триггеры по группам
        foreach (var group in TriggerGroups.DamageModificationPipeline)
        {
            _triggerService.Execute(
                TriggerType.OnBeforeHit,
                TriggerPhase.ModifyCalculation,
                context,
                group);  // ← Фильтр по группе
        }

        // Trace лог
        _logger.Debug(context.GetTraceString());
        // BaseValue = 10
        // DamageBuff                +5 → 15
        // WeaponEnchant             +3 → 18
        // CriticalHit               x2.0 → 36
        // VulnerabilityDebuff       x1.5 → 54
        // ArmorReduction            -4 → 50
        // DamageClamp               = 50

        return new DamageResult { Final = context.CurrentValue };
    }
}
```

---

## Сортировка: Детальный пример

### Триггеры

```csharp
// Триггер A
public class TriggerA : IPassive
{
    public int Priority => 0;
    public TriggerGroup Group => TriggerGroup.Additive;
    // RegistrationOrder = 1
}

// Триггер B
public class TriggerB : IPassive
{
    public int Priority => -50;  // Higher priority!
    public TriggerGroup Group => TriggerGroup.Multiplicative;
    // RegistrationOrder = 2
}

// Триггер C
public class TriggerC : IPassive
{
    public int Priority => 0;
    public TriggerGroup Group => TriggerGroup.Additive;
    // RegistrationOrder = 3
}
```

### Порядок исполнения

```
1. TriggerB (Priority = -50, Group = Multiplicative) ← Первый (приоритет)
2. TriggerA (Priority = 0, Group = Additive, Order = 1)
3. TriggerC (Priority = 0, Group = Additive, Order = 3)
```

---

## Generic Pipeline

Для недетализированных сценариев:

```csharp
public class GenericTrigger : IPassive
{
    public int Priority => TriggerPriorities.Normal;
    public TriggerGroup Group => TriggerGroup.Normal;  // ✅ По умолчанию
    public TriggerPhase Phase => TriggerPhase.Default;
}
```

### Порядок

```
Phase: Default
├─ Group: First (10)
│   └─ (триггеры первой очереди)
├─ Group: Early (20)
│   └─ (ранние триггеры)
├─ Group: Normal (30)
│   ├─ TriggerA [Order = 1]
│   └─ TriggerB [Order = 2]
├─ Group: Late (40)
│   └─ (поздние триггеры)
└─ Group: Last (50)
    └─ (финальные триггеры)
```

---

## Best Practices

### ✅ DO

1. **Используй Additive для плоских бонусов**
   ```csharp
   public TriggerGroup Group => TriggerGroup.Additive;
   context.ModifyValue(+10, "FlatBonus");
   ```

2. **Используй Multiplicative для множителей**
   ```csharp
   public TriggerGroup Group => TriggerGroup.Multiplicative;
   context.MultiplyValue(2.0f, "Critical");
   ```

3. **Используй Final для clamp/cap**
   ```csharp
   public TriggerGroup Group => TriggerGroup.Final;
   context.SetValue(Math.Clamp(value, 1, 999), "Clamp");
   ```

### ❌ DON'T

1. **Не используй Reduction для множителей**
   ```csharp
   // ❌ Плохо
   public TriggerGroup Group => TriggerGroup.Reduction;
   context.MultiplyValue(0.5f, "HalfDamage");  // Это multiplicative!

   // ✅ Хорошо
   public TriggerGroup Group => TriggerGroup.Multiplicative;
   context.MultiplyValue(0.5f, "HalfDamage");
   ```

2. **Не игнорируй порядок групп**
   ```csharp
   // ❌ Плохо: все в Default
   public TriggerGroup Group => TriggerGroup.Default;  // Недетерминировано!

   // ✅ Хорошо: явная группа
   public TriggerGroup Group => TriggerGroup.Additive;
   ```

---

## Таблица групп

| Группа | Когда использовать | Пример |
|--------|-------------------|--------|
| **Additive** | Плоские бонусы | +5 damage, +10 armor |
| **Multiplicative** | Множители | x2 damage, x1.5 armor |
| **Reduction** | Вычитание/снижение | -3 damage, -50% damage |
| **Final** | Финальная обработка | min 1, max 999 |
| **First** | Самый первый | Инициализация |
| **Early** | Ранний | Пре-обработка |
| **Normal** | Обычный | Стандартные эффекты |
| **Late** | Поздний | Пост-обработка |
| **Last** | Самый последний | Финализация |

---

## Итог

**Порядок исполнения:**
```
Priority → Group → RegistrationOrder
   ↓          ↓           ↓
 -100    Additive      Order = 1
  -50    Multiplicative Order = 2
   0     Reduction     Order = 3
  50     Final         Order = 4
 100
```

**Результат:**
- ✅ Детерминированный порядок
- ✅ Правильный порядок модификаторов (Add → Mult → Reduce)
- ✅ Предсказуемые результаты
- ✅ Легкая отладка через Trace
