# TriggerSource — Источник триггера

## Зачем нужен TriggerSource

### Проблема: Бесконечный цикл

```csharp
// Artifact: "При получении урона, нанеси 2 урона атакующему"
public class ThornArmorArtifact : ArtifactBase
{
    public override TriggerResult Execute(TriggerContext context)
    {
        // ❌ ПЛОХО: Наносим урон который вызывает OnDamage снова
        target.TakeDamage(2);
        // → OnDamage → этот же артефакт → бесконечность!
        return TriggerResult.Continue;
    }
}
```

**Результат:**
```
OnDamage (depth 0)
   └─ ThornArmorArtifact
      └─ TakeDamage(2)
         └─ OnDamage (depth 1)
            └─ ThornArmorArtifact
               └─ TakeDamage(2)
                  └─ OnDamage (depth 2)
                     └─ ... STACK OVERFLOW!
```

---

## Решение: TriggerSource

### Добавляем источник в контекст

```csharp
public enum TriggerSource
{
    Unknown = 0,
    Combat = 1,          // Боевое действие (атака, урон)
    Artifact = 2,        // Эффект артефакта
    Passive = 3,         // Пассивная способность
    StatusEffect = 4,    // Бафф/дебафф
    Environment = 5,     // Окружение (террейн, погода)
    DirectDamage = 6,    // Прямой урон (DoT, отражение)
    Heal = 7,            // Лечение
    Custom = 100         // Кастомный источник
}

public sealed class TriggerContext
{
    public TriggerSource SourceType { get; }      // Тип источника
    public object? SourceObject { get; }          // Конкретный объект
}
```

---

## Защита от рекурсии

### Способ 1: Проверка SourceType

```csharp
public class ThornArmorArtifact : ArtifactBase
{
    public override TriggerResult Execute(TriggerContext context)
    {
        // ✅ Игнорируем урон от других артефактов
        if (context.SourceType == TriggerSource.Artifact)
            return TriggerResult.Continue;

        // Наносим урон с указанием источника
        target.TakeDamage(2, source: TriggerSource.Artifact, sourceObject: this);
        return TriggerResult.Continue;
    }
}
```

### Способ 2: Проверка SourceObject

```csharp
public class ThornArmorArtifact : ArtifactBase
{
    private static readonly HashSet<object> _processing = new();

    public override TriggerResult Execute(TriggerContext context)
    {
        // ✅ Игнорируем если этот артефакт уже обрабатывается
        if (context.SourceObject == this)
            return TriggerResult.Continue;

        if (_processing.Contains(this))
            return TriggerResult.Continue;

        _processing.Add(this);
        try
        {
            target.TakeDamage(2, source: TriggerSource.Artifact, sourceObject: this);
        }
        finally
        {
            _processing.Remove(this);
        }

        return TriggerResult.Continue;
    }
}
```

### Способ 3: Depth + Source комбо

```csharp
public class DamageReflectionPassive : IPassive
{
    public TriggerResult Execute(TriggerContext context)
    {
        // ✅ Многоуровневая защита
        
        // 1. Игнорируем отражённый урон
        if (context.SourceType == TriggerSource.DirectDamage)
            return TriggerResult.Continue;

        // 2. Игнорируем урон от артефактов
        if (context.SourceType == TriggerSource.Artifact)
            return TriggerResult.Continue;

        // 3. Игнорируем если это мы вызвали
        if (context.SourceObject == this)
            return TriggerResult.Continue;

        // Наносим урон с правильным источником
        attacker.TakeDamage(5, 
            source: TriggerSource.DirectDamage, 
            sourceObject: this);

        return TriggerResult.Continue;
    }
}
```

---

## Примеры использования

### Combat Source

```csharp
// Атака в бою
public void Attack(Figure attacker, Figure target)
{
    var context = TriggerContextBuilder
        .For(TriggerType.OnBeforeHit, TriggerPhase.BeforeCalculation)
        .WithSource(TriggerSource.Combat)
        .WithActor(attacker)
        .WithTarget(target)
        .WithValue(attacker.Stats.Attack.Value)
        .Build();

    _triggerService.Execute(TriggerType.OnBeforeHit, context);
}
```

### Artifact Source

```csharp
// Артефакт наносит урон
public class FireAuraArtifact : ArtifactBase
{
    public override TriggerResult Execute(TriggerContext context)
    {
        // Игнорируем урон от других артефактов
        if (context.SourceType == TriggerSource.Artifact)
            return TriggerResult.Continue;

        // Наносим урон всем соседям
        foreach (var neighbor in GetNeighbors(target))
        {
            neighbor.TakeDamage(3, 
                source: TriggerSource.Artifact, 
                sourceObject: this);
        }

        return TriggerResult.Continue;
    }
}
```

### StatusEffect Source

```csharp
// Poison DoT
public class PoisonEffect : StatusEffectBase
{
    public override TriggerResult Execute(TriggerContext context)
    {
        // Наносим урон от яда
        owner.TakeDamage(_damage, 
            source: TriggerSource.StatusEffect, 
            sourceObject: this);

        return TriggerResult.Continue;
    }
}
```

### Environment Source

```csharp
// Лава: урон всем на клетке
public class LavaTile : IEnvironmentEffect
{
    public void ApplyDamage(Figure figure)
    {
        figure.TakeDamage(5, 
            source: TriggerSource.Environment, 
            sourceObject: this);
    }
}
```

---

## Фильтрация по Source

### Игнорирование определённых источников

```csharp
public class ShieldArtifact : ArtifactBase
{
    public override TriggerResult Execute(TriggerContext context)
    {
        // Игнорируем урон от окружения (лава, шипы)
        if (context.SourceType == TriggerSource.Environment)
            return TriggerResult.Continue;

        // Игнорируем DoT (яд, кровотечение)
        if (context.SourceType == TriggerSource.DirectDamage)
            return TriggerResult.Continue;

        // Поглощаем только боевой урон
        if (context.SourceType == TriggerSource.Combat)
        {
            _shield -= context.CurrentValue;
            return TriggerResult.Cancel;
        }

        return TriggerResult.Continue;
    }
}
```

### Приоритет источников

```csharp
public class DamageTracker : IPassive
{
    public TriggerResult Execute(TriggerContext context)
    {
        // Считаем только "честный" боевой урон
        if (context.SourceType == TriggerSource.Combat)
        {
            _combatDamageDealt += context.CurrentValue;
        }

        // Считаем урон от артефактов отдельно
        if (context.SourceType == TriggerSource.Artifact)
        {
            _artifactDamageDealt += context.CurrentValue;
        }

        return TriggerResult.Continue;
    }
}
```

---

## Логирование

### Debug Log с Source

```csharp
public class TriggerLogger
{
    public void LogExecution(TriggerContext context, string triggerName)
    {
        _logger.Debug(
            $"[{context.SourceType}] {triggerName}: " +
            $"{context.Actor} → {context.Target}, " +
            $"Value: {context.BaseValue} → {context.CurrentValue}");
    }
}
```

### Пример вывода

```
[Combat] CriticalPassive: PlayerKing → EnemyPawn, Value: 10 → 20
[Artifact] FireAuraArtifact: PlayerKing → EnemyPawn, Value: 0 → 3
[StatusEffect] PoisonEffect: EnemyPawn → EnemyPawn, Value: 0 → 2
[Environment] LavaTile: LavaTile → PlayerKing, Value: 0 → 5
```

---

## Best Practices

### ✅ DO

1. **Всегда указывай источник**
   ```csharp
   target.TakeDamage(5, 
       source: TriggerSource.Artifact, 
       sourceObject: this);  // ✅
   ```

2. **Игнорируй свой тип источника**
   ```csharp
   if (context.SourceType == TriggerSource.Artifact)
       return TriggerResult.Continue;  // ✅
   ```

3. **Используй SourceObject для точной фильтрации**
   ```csharp
   if (context.SourceObject == this)
       return TriggerResult.Continue;  // ✅
   ```

4. **Логируй Source для отладки**
   ```csharp
   _logger.Debug($"[{context.SourceType}] {triggerName}");  // ✅
   ```

### ❌ DON'T

1. **Не вызывай события без указания источника**
   ```csharp
   target.TakeDamage(5);  // ❌ Unknown source!
   ```

2. **Не игнорируй SourceType**
   ```csharp
   // ❌ Может вызвать бесконечный цикл
   public TriggerResult Execute(TriggerContext context)
   {
       target.TakeDamage(5);  // Нет проверки!
   }
   ```

3. **Не используй Unknown без необходимости**
   ```csharp
   // ❌ Плохо
   source: TriggerSource.Unknown

   // ✅ Хорошо
   source: TriggerSource.Custom, sourceObject: this
   ```

---

## Таблица источников

| Источник | Когда использовать | Пример |
|----------|-------------------|--------|
| **Combat** | Боевые действия | Атака, урон от оружия |
| **Artifact** | Эффекты артефактов | Отражение, аура |
| **Passive** | Пассивные способности | Крит, вампиризм |
| **StatusEffect** | Баффы/дебаффы | Яд, кровотечение, горение |
| **Environment** | Окружение | Лава, шипы, погода |
| **DirectDamage** | Прямой урон | DoT, отражённый урон |
| **Heal** | Лечение | Хил, регенерация |
| **Custom** | Кастомные эффекты | Уникальные механики |

---

## Отладка циклов

### Trigger Chain Visualization

```csharp
public class TriggerDebugger
{
    private static readonly Stack<string> _callStack = new();

    public void OnExecute(TriggerContext context, string triggerName)
    {
        _callStack.Push($"{context.SourceType}:{triggerName}");

        if (_callStack.Count > 5)
        {
            _logger.Warning($"Deep trigger chain detected:\n{string.Join("\n", _callStack)}");
        }

        // Проверка на рекурсию
        var sameSource = _callStack.Count(s => s.StartsWith(context.SourceType.ToString()));
        if (sameSource > 2)
        {
            _logger.Error($"Possible infinite loop: {context.SourceType} triggered {sameSource} times");
        }
    }

    public void OnComplete()
    {
        _callStack.Pop();
    }
}
```

### Пример вывода

```
Trigger Chain:
├─ Combat:PrimaryHitEffect
│  ├─ Passive:CriticalPassive
│  ├─ Passive:DamageBuff
│  └─ Artifact:FireAuraArtifact
│     └─ ⚠️ DirectDamage:FireDamage (blocked - same source type)
```

---

## Итог

**TriggerSource решает:**
- ✅ Бесконечные циклы (артефакт → урон → артефакт)
- ✅ Непредсказуемые взаимодействия
- ✅ Сложную отладку
- ✅ Фильтрацию по типу источника

**Всегда указывай источник:**
```csharp
TriggerContextBuilder
    .For(TriggerType.OnDamageReceived)
    .WithSource(TriggerSource.Artifact, this)  // ✅
    .Build();
```
