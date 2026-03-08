# Trigger Recursion Protection

## Защита от рекурсивных триггеров

### Проблема

```
OnDamage (триггер 1)
   ↓
artifact наносит доп. урон
   ↓
OnDamage (триггер 2)
   ↓
artifact снова наносит урон
   ↓
OnDamage (триггер 3)
   ↓
...
   ↓
STACK OVERFLOW! 💥
```

---

## Реализация защиты

### 1. MaxTriggerDepth

```csharp
public class TriggerExecutor<T>
{
    [ThreadStatic]
    private static int _currentDepth;
    
    public int MaxTriggerDepth { get; set; } = 10;
    
    public TriggerResult Execute(...)
    {
        if (_currentDepth >= MaxTriggerDepth)
        {
            _logger.Error($"Trigger depth exceeded {MaxTriggerDepth}. Aborting.");
            return TriggerResult.Cancel;
        }
        
        _currentDepth++;
        try
        {
            // Execute triggers
        }
        finally
        {
            _currentDepth--;
        }
    }
}
```

### 2. ExecutingTriggers (Direct Recursion Guard)

```csharp
[ThreadStatic]
private static HashSet<string>? _executingTriggers;

public TriggerResult Execute(...)
{
    string triggerId = entry.Source.GetType().FullName;
    
    if (_executingTriggers.Contains(triggerId))
    {
        _logger.Warning($"Recursive trigger: {triggerId}. Skipping.");
        continue; // Пропускаем рекурсивный вызов
    }
    
    _executingTriggers.Add(triggerId);
    try
    {
        entry.Trigger.Execute(context);
    }
    finally
    {
        _executingTriggers.Remove(triggerId);
    }
}
```

---

## Примеры

### ❌ Плохо: Бесконечный цикл

```csharp
public class DamageReflectionArtifact : ArtifactBase
{
    public override TriggerPhase Phase => TriggerPhase.AfterApplication;
    
    public override TriggerResult Execute(TriggerContext context)
    {
        if (!context.TryGetData<AfterHitContext>(out var hit)) 
            return TriggerResult.Continue;

        // ❌ ПЛОХО: Наносим урон который вызывает OnDamage снова
        hit.Target.TakeDamage(5);  // → OnDamage → этот же триггер → бесконечность
        
        return TriggerResult.Continue;
    }
}
```

### ✅ Хорошо: Защита через флаг

```csharp
public class DamageReflectionArtifact : ArtifactBase
{
    private static bool _isReflecting;
    
    public override TriggerPhase Phase => TriggerPhase.AfterApplication;
    
    public override TriggerResult Execute(TriggerContext context)
    {
        if (_isReflecting) return TriggerResult.Continue;
        if (!context.TryGetData<AfterHitContext>(out var hit)) 
            return TriggerResult.Continue;

        _isReflecting = true;
        try
        {
            // ✅ Наносим урон с флагом
            hit.Target.TakeDamage(5, ignoreTriggers: true);
        }
        finally
        {
            _isReflecting = false;
        }
        
        return TriggerResult.Continue;
    }
}
```

### ✅ Хорошо: Отложенный урон

```csharp
public class DamageReflectionArtifact : ArtifactBase
{
    public override TriggerPhase Phase => TriggerPhase.AfterApplication;
    
    public override TriggerResult Execute(TriggerContext context)
    {
        if (!context.TryGetData<AfterHitContext>(out var hit)) 
            return TriggerResult.Continue;

        // ✅ Откладываем урон на после обработки всех триггеров
        context.SetCustomData(new PendingDamage(5, hit.Target));
        
        return TriggerResult.Continue;
    }
}

// В DamagePipeline:
public void ApplyDamage(...)
{
    var result = ExecutePipeline(...);
    
    // Применяем отложенный урон после pipeline
    if (context.GetCustomData<PendingDamage>() is var pending && pending != null)
    {
        pending.Target.TakeDamage(pending.Amount);
    }
}
```

---

## Уровни защиты

### Уровень 1: Depth Limit

```csharp
// Глубина 10 — достаточно для большинства сценариев
MaxTriggerDepth = 10;

// Пример легального chain:
// OnBeforeHit (depth 0)
//   → CriticalCheck (depth 1)
//   → DamageBuff (depth 2)
//   → VulnerabilityDebuff (depth 3)
// OnAfterHit (depth 0, сброс)
//   → Lifesteal (depth 1)
//   → Thorns (depth 2)
//   → BleedApply (depth 3)
```

### Уровень 2: Direct Recursion Guard

```csharp
// Один и тот же триггер не может выполняться рекурсивно
ExecutingTriggers = { "ThornsPassive" }

// Если ThornsPassive пытается вызвать себя → пропускаем
```

### Уровень 3: Custom Protection

```csharp
public class ThornArmorPassive : IPassive
{
    private static int _activationCount;
    private const int MaxActivationsPerTurn = 1;
    
    public TriggerResult Execute(TriggerContext context)
    {
        if (_activationCount >= MaxActivationsPerTurn)
            return TriggerResult.Continue;
        
        _activationCount++;
        // ... логика
        _activationCount--;
        
        return TriggerResult.Continue;
    }
}
```

---

## Логирование

### Debug Log

```
[TriggerExecutor] Depth: 0 → Executing OnBeforeHit
[TriggerExecutor] Depth: 1 → Executing CriticalPassive
[TriggerExecutor] Depth: 2 → Executing DamageBuff
[TriggerExecutor] Depth: 3 → Executing VulnerabilityDebuff
[TriggerExecutor] Depth: 0 → Executing OnAfterHit
[TriggerExecutor] Depth: 1 → Executing LifestealPassive
[TriggerExecutor] Depth: 2 → Executing ThornsPassive
[TriggerExecutor] Recursive trigger detected: ThornsPassive. Skipping.
```

### Error Log (превышение глубины)

```
[ERROR] Trigger execution depth exceeded 10. Possible infinite loop.
Trigger chain:
  0: OnDamageReceived
  1: DamageReflectionArtifact
  2: OnDamageReceived
  3: DamageReflectionArtifact
  ...
  10: [ABORTED]

Aborting trigger chain.
```

---

## Рекомендации

### ✅ DO

1. **Используй отложенный урон**
   ```csharp
   context.SetCustomData(new PendingDamage(5, target));
   // Применяется после pipeline
   ```

2. **Проверяй глубину в критичных местах**
   ```csharp
   if (context.GetCustomData<TriggerDepth>()?.Current >= 5)
   {
       _logger.Warning("Deep trigger chain detected");
   }
   ```

3. **Документируй триггеры которые могут вызывать рекурсию**
   ```csharp
   /// <summary>
   /// Deals 5 damage back to attacker.
   /// WARNING: May cause recursive OnDamage triggers.
   /// Use with caution in combination with DamageReflection.
   /// </summary>
   ```

### ❌ DON'T

1. **Не вызывай события напрямую из триггера**
   ```csharp
   // ❌ ПЛОХО
   public TriggerResult Execute(...)
   {
       _eventService.Raise(new DamageEvent(...));  // → рекурсия!
   }
   ```

2. **Не полагайся только на MaxTriggerDepth**
   ```csharp
   // ❌ Глубина 100 — скрывает проблему, не решает
   MaxTriggerDepth = 100;
   ```

3. **Не игнорируй предупреждения о рекурсии**
   ```
   [WARNING] Recursive trigger detected: ThornsPassive
   // ❌ Не игнорируй! Это баг в дизайне.
   ```

---

## Настройка

### В TriggerService

```csharp
public class TriggerService
{
    public int MaxTriggerDepth 
    { 
        get => _executor?.MaxTriggerDepth ?? 10;
        set
        {
            if (_executor != null)
                _executor.MaxTriggerDepth = value;
        }
    }
}

// Использование
triggerService.MaxTriggerDepth = 5;  // Более строгий лимит
```

### В TriggerExecutor

```csharp
var executor = new TriggerExecutor<ITrigger>(
    () => triggers,
    logService)
{
    MaxTriggerDepth = 5  // Custom limit
};
```

---

## Тестирование

### Unit Test

```csharp
[Test]
public void RecursiveTrigger_ShouldBeBlocked()
{
    var executor = new TriggerExecutor<ITrigger>(...);
    var recursiveTrigger = new RecursiveTrigger();
    
    var context = TriggerContextBuilder.For(TriggerType.OnDamage)
        .WithActor(attacker)
        .WithTarget(target)
        .Build();
    
    // Первый вызов
    executor.Execute(TriggerType.OnDamage, context);
    
    // Второй вызов (рекурсивный) должен быть заблокирован
    Assert.DoesNotThrow(() => 
        executor.Execute(TriggerType.OnDamage, context)
    );
}

[Test]
public void DeepTriggerChain_ShouldBeAborted()
{
    var executor = new TriggerExecutor<ITrigger>(...)
    {
        MaxTriggerDepth = 3
    };
    
    var context = TriggerContextBuilder.For(TriggerType.OnDamage)
        .Build();
    
    // Цепочка из 5 триггеров
    var result = executor.Execute(TriggerType.OnDamage, context);
    
    // Должен быть отменён после depth=3
    Assert.AreEqual(TriggerResult.Cancel, result);
}
```

---

## Диагностика

### Trigger Depth Tracker

```csharp
public class TriggerDepthTracker
{
    [ThreadStatic]
    private static Stack<string>? _callStack;
    
    public static void Push(string trigger)
    {
        _callStack ??= new Stack<string>();
        _callStack.Push(trigger);
        
        if (_callStack.Count > 5)
        {
            Debug.LogWarning($"Deep trigger chain:\n{string.Join("\n", _callStack)}");
        }
    }
    
    public static void Pop()
    {
        _callStack?.Pop();
    }
}

// В TriggerExecutor:
TriggerDepthTracker.Push(triggerId);
try { entry.Trigger.Execute(context); }
finally { TriggerDepthTracker.Pop(); }
```

### Визуализация цепочки

```
Trigger Chain Visualization:
├─ OnBeforeHit (depth 0)
│  ├─ CriticalPassive (depth 1)
│  ├─ DamageBuff (depth 2)
│  └─ VulnerabilityDebuff (depth 3)
├─ OnAfterHit (depth 0)
│  ├─ LifestealPassive (depth 1)
│  ├─ ThornsPassive (depth 2)
│  └─ DamageReflection → BLOCKED (would be depth 3)
```

---

## Итог

| Защита | Описание | Когда срабатывает |
|--------|----------|-------------------|
| **MaxTriggerDepth** | Лимит глубины | Глубина ≥ 10 |
| **ExecutingTriggers** | Direct recursion | Тот же триггер выполняется |
| **Custom flags** | Ручная защита | По твоему условию |

**Рекомендуемая конфигурация:**
- `MaxTriggerDepth = 10` (по умолчанию)
- `ExecutingTriggers` (всегда включено)
- Custom flags для триггеров с известными проблемами
