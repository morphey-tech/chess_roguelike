# Миграция: Step-Based → Action-Based System

## Резюме

Полная миграция системы ходов с **Step-based** на **Action-based** архитектуру завершена. Система теперь использует единый источник истины для валидации, UI подсветки, AI и выполнения действий.

---

## Что было сделано

### 1. Создана новая Action-система

#### Интерфейсы:
- **`ICombatAction`** — базовый интерфейс действия с методами:
  - `CanExecute(context)` — проверка возможности выполнения
  - `GetValidTargets(actor, from, grid)` — получение валидных целей (для UI/AI)
  - `ExecuteAsync(context)` — выполнение действия

- **`IActionBuilder`** — фабрика для создания действий из конфигов
- **`IActionBuilderContext`** — контекст с сервисами для builders
- **`ActionBuilderRegistry`** — реестр всех builders

#### Реализованные действия:
- **`MoveAction`** — базовое движение
- **`AttackAction`** — базовая атака
- **`MoveThenAttackAction`** — составное действие для ладьи (движение + атака)
- **`MoveToTargetAction`** — движение к цели для ближнего боя
- **`MoveToKilledTargetAction`** — движение к убитой цели
- **`SequentialAction`** — последовательное выполнение нескольких действий

### 2. Миграция конфигов

**`TurnPatternDescriptionsConfig.json`** полностью мигрирован:
- Все `steps` заменены на `action`
- Составные действия используют `sequential` с `sub_actions`
- Примеры:
  ```json
  {
    "id": "melee_attack",
    "action": {
      "type": "sequential",
      "sub_actions": [
        { "type": "attack" },
        { "type": "move_to_killed" }
      ]
    }
  }
  ```

### 3. Обновлена архитектура

- **`TurnPatternDescription`** теперь хранит `ICombatAction` вместо `ITurnStep`
- **`TurnPatternFactory`** создает действия через builders
- **`TurnPatternResolver`** возвращает `ICombatAction`
- **`TurnExecutor`** использует действия с валидацией через `CanExecute`
- **`StageQueryService`** использует `GetValidTargets` из действий для UI подсветки

### 4. Удален legacy код

**Удалены все Step-классы:**
- `ITurnStep` (интерфейс)
- `MoveStep`, `AttackStep`
- `MoveToTargetStep`, `MoveToKilledTargetStep`
- `CompositeTurnStep`, `ConditionalStep`
- `TurnStepFactory`
- `StepConfig`

**Удалены Obsolete поля:**
- `Steps` из `TurnPatternDescriptionConfig`
- Регистрация `TurnStepFactory` из DI

---

## Ключевые преимущества

### ✅ Единый источник истины
- UI, AI и execution используют одни и те же методы `GetValidTargets` и `CanExecute`
- Невозможна десинхронизация между отображением и выполнением

### ✅ Составные действия как first-class
- Ладья использует `MoveThenAttackAction` с одним (from, to) контрактом
- `GetValidTargets` возвращает все врагов, которых можно атаковать после движения по линии

### ✅ Конфигурация через данные
- Паттерны остаются конфигурационными через `TurnPatternFactory`
- Условия (`ConditionRegistry`) работают как раньше

### ✅ Чистая архитектура
- Нет дублирования логики между Steps и Actions
- Все действия используют `AttackQueryService` для проверки `CanAttack` (не просто range)

---

## Исправления

### Исправлено отображение целей атаки для ладьи

**Проблема:** `MoveToTargetAction` использовал `IsInRange` вместо реальной проверки `CanAttack`, что не работало для ладьи с `StraightLine` targeting.

**Решение:**
- `MoveToTargetAction` теперь использует `IAttackQueryService.GetTargets()` для проверки `CanAttack`
- Это правильно обрабатывает profiled атаки (ладья) и обычные стратегии атаки
- `GetValidTargets` проверяет возможность атаки через реальную стратегию, а не просто range

---

## Структура файлов

```
Assets/Scripts/Gameplay/Turn/Actions/
├── ICombatAction.cs              # Базовый интерфейс действия
├── IActionBuilder.cs             # Интерфейс builder'а
├── ActionConfig.cs               # Конфиг для кода (Gameplay)
├── ActionBuilderContext.cs       # Реализация контекста
├── ActionBuilderRegistry.cs      # Реестр builders
├── Impl/
│   ├── MoveAction.cs
│   ├── AttackAction.cs
│   ├── MoveThenAttackAction.cs   # Для ладьи
│   ├── MoveToTargetAction.cs
│   ├── MoveToKilledTargetAction.cs
│   └── SequentialAction.cs
└── Builders/
    ├── MoveActionBuilder.cs
    ├── AttackActionBuilder.cs
    ├── MoveThenAttackActionBuilder.cs
    ├── MoveToTargetActionBuilder.cs
    ├── MoveToKilledTargetActionBuilder.cs
    └── SequentialActionBuilder.cs

Assets/Scripts/Core/Configs/Turn/
├── ActionConfig.cs               # Конфиг для JSON (Core)
└── TurnPatternDescriptionConfig.cs

Assets/Content/Configs/Figure/
└── TurnPatternDescriptionsConfig.json  # Мигрированные конфиги
```

---

## Пример использования

### Конфиг для ладьи (move_then_attack):
```json
{
  "id": "rook_move_then_attack",
  "priority": 10,
  "condition_id": "target_is_enemy",
  "action": {
    "type": "move_then_attack"
  }
}
```

### Как это работает:
1. `TurnPatternFactory` создает `MoveThenAttackAction` через builder
2. `StageQueryService.GetSelectionInfo()` вызывает `action.GetValidTargets()`
3. `GetValidTargets` возвращает всех врагов на линии, которых можно атаковать после движения
4. UI подсвечивает эти позиции
5. При клике `TurnExecutor` проверяет `action.CanExecute()` и выполняет `action.ExecuteAsync()`

---

## Миграция завершена ✅

- ✅ Все конфиги мигрированы
- ✅ Все Step-классы удалены
- ✅ Legacy код удален
- ✅ Исправлено отображение целей атаки для ладьи
- ✅ Единый источник истины для UI/AI/execution
