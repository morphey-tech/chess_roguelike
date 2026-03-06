# Система Артефактов (Artifacts System)

## Обзор

Система артефактов предоставляет механизм для создания пассивных бонусов и эффектов, срабатывающих по событиям (начало боя, смерть фигуры и т.д.).

**Примечание:** Артефакты не выпадают из лут-таблиц за убийства. Они выдаются за ивенты, достижения или другие специальные условия.

## Архитектура

```
Data Layer (Configs)
├── ArtifactConfig (JSON конфиги)
├── ArtifactEnums (Rarity, Trigger, EffectType)
└── ArtifactConfigRepository (загрузка)

Logic Layer
├── IArtifact (базовый интерфейс)
├── ArtifactBase (базовый класс)
├── ArtifactFactory (создание эффектов)
├── ArtifactService (управление коллекцией)
└── ArtifactTriggerService (обработка событий)

Integration
└── EconomyService.Artifacts (доступ через экономику)
```

## Конфигурация

### JSON формат (ArtifactsConfig.json)

```json
{
  "content": [
    {
      "id": "vampire_fang",
      "name": "Vampire Fang",
      "description": "Heal for 3 HP when killing an enemy",
      "rarity": "rare",
      "trigger": "on_unit_kill",
      "icon": "vampire_fang_icon",
      "effect": {
        "type": "heal",
        "target": "killer",
        "value": 3
      }
    }
  ]
}
```

### Перечисления

**ArtifactRarity:**
- `Common` - обычные артефакты
- `Rare` - редкие артефакты
- `Legendary` - легендарные артефакты

**ArtifactTrigger:**
- `Passive` - всегда активны
- `OnBattleStart` - начало боя
- `OnBattleEnd` - конец боя
- `OnUnitDeath` - смерть любой фигуры
- `OnUnitKill` - убийство врага
- `OnAllyDeath` - смерть союзника
- `OnDamageReceived` - получение урона
- `OnReward` - выбор награды

**ArtifactEffectType:**
- `StatBuff` - бафф статы
- `AllStatsBuff` - бафф все статы
- `Heal` - лечение
- `Shield` - щит
- `ReflectDamage` - отражение урона
- `Revive` - воскрешение
- `ExtraChoice` - дополнительный выбор

## Использование

### Получение артефакта

```csharp
// Через EconomyService
var artifact = economyService.AddArtifact("vampire_fang");

// Или напрямую через ArtifactService
var instance = artifactService.Add("phoenix_feather");
```

### Проверка наличия

```csharp
bool hasArtifact = artifactService.Has("vampire_fang");
var artifacts = artifactService.GetByTrigger(ArtifactTrigger.OnUnitKill);
```

### Триггеры (внутреннее использование)

```csharp
// Начало боя
artifactTriggerService.TriggerBattleStart();

// Конец боя
artifactTriggerService.TriggerBattleEnd();

// Смерть фигуры обрабатывается автоматически через FigureDeathMessage
```

## Расширение

### Добавление нового типа эффекта

1. Добавьте новый тип в `ArtifactEffectType` enum
2. Создайте класс эффекта, наследуясь от `ArtifactBase` или `TriggeredArtifactBase`
3. Добавьте маппинг в `ArtifactFactory.CreateFromConfig()`

Пример:
```csharp
public sealed class NewEffectArtifact : TriggeredArtifactBase
{
    public NewEffectArtifact(ArtifactConfig config) : base(config) { }

    public override void OnTrigger(ArtifactTriggerContext context)
    {
        // Логика эффекта
    }
}

// В ArtifactFactory:
ArtifactEffectType.NewEffect => new NewEffectArtifact(config)
```

### Добавление нового триггера

1. Добавьте тип в `ArtifactTrigger` enum
2. Добавьте обработчик в `ArtifactTriggerService`
3. Подпишитесь на нужное событие (через MessagePipe)

## Интеграция с другими системами

### EconomyService
Артефакты очищаются при `StartNewRun()`

### Loot System
Для учёта артефактов с `ExtraChoice`:
```csharp
int extraChoices = economyService.Artifacts.GetExtraChoices();
```

### Save System
Сохранение артефактов реализуется через `ArtifactSaveAdapter` (требуется реализовать)

## Примеры артефактов

| ID | Название | Эффект | Триггер |
|----|----------|--------|---------|
| warrior_banner | Warrior Banner | +2 attack allies | OnBattleStart |
| vampire_fang | Vampire Fang | +3 HP on kill | OnUnitKill |
| phoenix_feather | Phoenix Feather | Revive 50% HP | OnAllyDeath |
| thorn_armor | Thorn Armor | Reflect 2 damage | OnDamageReceived |
| crown_of_power | Crown of Power | +1 all stats | Passive |
