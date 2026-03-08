# TriggerContext Mutation Rules

## Правила изменения контекста

### Immutable поля (нельзя менять)

```csharp
public sealed class TriggerContext
{
    public TriggerType Type { get; }      // ❌ Immutable
    public TriggerPhase Phase { get; }    // ❌ Immutable
    public object? Actor { get; }         // ❌ Immutable
    public object? Target { get; }        // ❌ Immutable
    
    public int Value { get; set; }        // ✅ Mutable (в нужной фазе)
    public object? Data { get; set; }     // ✅ Mutable (в нужной фазе)
}
```

---

## Что можно менять в каждой фазе

### Damage Pipeline

```
┌─────────────────────────────────────────────────────────────────┐
│ BeforeCalculation (1)                                           │
│ ├─ Можно: проверять условия (стун, сон, заморозка)             │
│ ├─ Можно: отменять атаку (TriggerResult.Cancel)                │
│ └─ Нельзя: менять урон (ещё не рассчитан)                      │
└─────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│ ModifyCalculation (2) ← ЕДИНСТВЕННАЯ фаза для модификации урона│
│ ├─ Можно: hit.DamageMultiplier *= 2.0f                         │
│ ├─ Можно: hit.BonusDamage += 10                                │
│ ├─ Можно: context.ModifyValue(+5, "Buff")                      │
│ └─ Нельзя: context.SetValue(100) // Переписывание плохая идея  │
└─────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│ BeforeApplication (3)                                           │
│ ├─ Можно: поглощать урон (щит)                                  │
│ ├─ Можно: отменять урон (TriggerResult.Cancel)                 │
│ ├─ Можно: перенаправлять урон                                   │
│ └─ Нельзя: менять BaseDamage (уже рассчитан!)                  │
└─────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│ AfterApplication (4)                                            │
│ ├─ Можно: читать FinalDamage                                     │
│ ├─ Можно: применять последствия (кровотечение, вампиризм)      │
│ └─ Нельзя: менять урон (уже применён!)                         │
└─────────────────────────────────────────────────────────────────┘
```

---

## Примеры правильного использования

### ✅ ПРАВИЛЬНО: Модификация в ModifyCalculation

```csharp
public class CriticalPassive : IPassive
{
    public TriggerPhase Phase => TriggerPhase.ModifyCalculation;
    
    public TriggerResult Execute(TriggerContext context)
    {
        if (!context.TryGetData<BeforeHitContext>(out var hit)) 
            return TriggerResult.Continue;

        // ✅ ПРАВИЛЬНО: модификация в нужной фазе
        hit.DamageMultiplier *= 2.0f;
        hit.IsCritical = true;
        
        return TriggerResult.Continue;
    }
}

public class DamageBuffPassive : IPassive
{
    public TriggerPhase Phase => TriggerPhase.ModifyCalculation;
    
    public TriggerResult Execute(TriggerContext context)
    {
        if (!context.TryGetData<BeforeHitContext>(out var hit)) 
            return TriggerResult.Continue;

        // ✅ ПРАВИЛЬНО: бонус через модификатор
        hit.BonusDamage += 10;
        
        // ✅ ПРАВИЛЬНО: с указанием источника
        context.ModifyValue(+5, "DamageBuffPassive");
        
        return TriggerResult.Continue;
    }
}
```

### ❌ НЕПРАВИЛЬНО: Модификация не в той фазе

```csharp
public class WrongTimingPassive : IPassive
{
    public TriggerPhase Phase => TriggerPhase.AfterApplication;
    
    public TriggerResult Execute(TriggerContext context)
    {
        if (!context.TryGetData<BeforeHitContext>(out var hit)) 
            return TriggerResult.Continue;

        // ❌ НЕПРАВИЛЬНО: урон уже применён, это не имеет эффекта
        hit.DamageMultiplier *= 2.0f;
        
        // ❌ НЕПРАВИЛЬНО: попытка изменить после применения
        context.SetValue(100, "TooLatePassive");
        
        return TriggerResult.Continue;
    }
}
```

### ✅ ПРАВИЛЬНО: Чтение в AfterApplication

```csharp
public class LifestealPassive : IPassive
{
    public TriggerPhase Phase => TriggerPhase.AfterApplication;
    
    public TriggerResult Execute(TriggerContext context)
    {
        if (!context.TryGetData<AfterHitContext>(out var hit)) 
            return TriggerResult.Continue;

        // ✅ ПРАВИЛЬНО: только чтение
        int damageDealt = hit.DamageDealt;
        int heal = (int)(damageDealt * _percent);
        
        attacker.Stats.Heal(heal);
        
        return TriggerResult.Continue;
    }
}
```

---

## Отслеживание модификаций

```csharp
public class TracingDamageModifier : IPassive
{
    public TriggerPhase Phase => TriggerPhase.ModifyCalculation;
    
    public TriggerResult Execute(TriggerContext context)
    {
        // Проверяем, был ли контекст уже модифицирован
        if (context.IsModified)
        {
            _logger.Debug($"Context already modified by {context.ModifiedBy}");
        }
        
        context.ModifyValue(+5, "TracingDamageModifier");
        
        // Теперь IsModified = true, ModifiedBy = "TracingDamageModifier"
        
        return TriggerResult.Continue;
    }
}
```

---

## Рекомендации

### ✅ DO

1. **Модифицируй только в Modify* фазах**
   ```csharp
   public TriggerPhase Phase => TriggerPhase.ModifyCalculation;
   ```

2. **Используй именованные методы для модификации**
   ```csharp
   context.ModifyValue(+5, "MyPassive");  // Ясно кто изменил
   ```

3. **Проверяй фазу перед модификацией**
   ```csharp
   if (context.Phase != TriggerPhase.ModifyCalculation)
       return TriggerResult.Continue;
   ```

4. **Документируй что меняет твой триггер**
   ```csharp
   /// <summary>
   /// Adds +10 bonus damage. Use in ModifyCalculation phase only.
   /// </summary>
   ```

### ❌ DON'T

1. **Не меняй Value после Apply фазы**
   ```csharp
   // ❌ Бесполезно!
   public TriggerPhase Phase => TriggerPhase.AfterApplication;
   context.Value += 10;  // Урон уже применён
   ```

2. **Не переписывай Value полностью**
   ```csharp
   // ❌ Плохо: переписывает все предыдущие модификации
   context.SetValue(100);
   
   // ✅ Хорошо: добавляет к существующему
   context.ModifyValue(+10);
   ```

3. **Не меняй immutable поля**
   ```csharp
   // ❌ Не скомпилируется
   context.Type = TriggerType.OnAfterHit;  // Compilation error
   ```

---

## Debugging модификаций

```csharp
// В логе будет видно кто и когда модифицировал контекст
if (context.IsModified)
{
    _logger.Debug($"Value changed from {originalValue} to {context.Value}");
    _logger.Debug($"Modified by: {context.ModifiedBy}");
    _logger.Debug($"Phase: {context.Phase}");
}
```

---

## Порядок модификаций в ModifyCalculation

```
ModifyCalculation Phase (Priority order)
│
├─ Priority: Critical (-100)
│   └─ (нет модификаторов урона, только отмена)
│
├─ Priority: High (-50)
│   ├─ CriticalPassive: DamageMultiplier *= 2.0
│   └─ VulnerabilityDebuff: DamageMultiplier *= 1.5
│
├─ Priority: Normal (0)
│   ├─ DamageBuff: BonusDamage += 10
│   └─ WeaponEnchant: ModifyValue(+5, "FireEnchant")
│
└─ Priority: Low (50)
    └─ (баффы применяются, но не меняют урон напрямую)
```

**Итоговый урон:** `Base * Multiplier + Bonus`

---

## Таблица разрешений

| Поле | BeforeCalc | ModifyCalc | BeforeApp | AfterApp |
|------|-----------|------------|-----------|----------|
| `Type` | ❌ | ❌ | ❌ | ❌ |
| `Phase` | ❌ | ❌ | ❌ | ❌ |
| `Actor` | ❌ | ❌ | ❌ | ❌ |
| `Target` | ❌ | ❌ | ❌ | ❌ |
| `Value` | ⚠️ | ✅ | ⚠️ | ❌ |
| `Data` | ✅ | ✅ | ✅ | ✅ (read-only) |
| `CustomData` | ✅ | ✅ | ✅ | ✅ |

**Условные обозначения:**
- ✅ Можно
- ⚠️ Можно с осторожностью
- ❌ Нельзя
