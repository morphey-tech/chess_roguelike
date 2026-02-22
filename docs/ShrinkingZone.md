# Shrinking Zone System

Гибкая система сужающейся зоны для пошаговой тактической игры на сетке 8x8.

## Архитектура

Система построена на **MessagePipe** с разделением на команды и события:

```
┌─────────────────────────────────────────────────────────────┐
│                    Command Messages                         │
│  - ZoneBattleStartedMessage                                 │
│  - ZoneTurnStartedMessage                                   │
│  - ZoneDamageDealtMessage                                   │
│  - ZoneUnitTurnEndedMessage                                 │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│              ZoneShrinkSystem (подписчик)                   │
│  - Подписывается на команды через ISubscriber<T>            │
│  - Публикует события через IPublisher<T>                    │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                    Event Messages                           │
│  - ZoneStateChangedMessage                                  │
│  - ZoneCellsUpdatedMessage                                  │
│  - UnitTakeZoneDamageMessage                                │
└─────────────────────────────────────────────────────────────┘
```

## Структура файлов

```
Assets/
├── Content/Configs/
│   └── ShrinkingZoneConfig.json
├── Scripts/
│   ├── Core/ShrinkingZone/
│   │   ├── Config/ZoneShrinkConfig.cs
│   │   ├── Core/
│   │   │   ├── IZoneShrinkStrategy.cs
│   │   │   ├── IZoneShrinkQueryService.cs  # Query интерфейс
│   │   │   ├── IZoneDamageTarget.cs
│   │   │   ├── ZoneTypes.cs
│   │   │   └── ZoneShrinkState.cs
│   │   ├── Messages/
│   │   │   └── ZoneMessages.cs             # Event messages
│   │   ├── Strategies/LayerRingStrategy.cs
│   │   └── SaveLoad/ZoneShrinkSaveLoad.cs
│   ├── Gameplay/ShrinkingZone/
│   │   ├── Messages/
│   │   │   └── ZoneCommandMessages.cs      # Command messages
│   │   ├── ZoneShrinkSystem.cs             # Основная система
│   │   ├── ZoneShrinkSystemFactory.cs      # Фабрика для стейджей
│   │   ├── ZoneCellEvaluator.cs
│   │   └── FigureZoneDamageTarget.cs
│   └── Unity/ShrinkingZone/
│       └── ZoneShrinkInstaller.cs          # DI регистрация
└── Tests/ShrinkingZone/
```

## Сообщения

### Команды (Commands)

```csharp
// Начало боя
public readonly struct ZoneBattleStartedMessage { }

// Начало хода
public readonly struct ZoneTurnStartedMessage
{
    public readonly int Turn;
}

// Урон нанесён (для активации)
public readonly struct ZoneDamageDealtMessage
{
    public readonly int Turn;
}

// Юнит завершил ход
public readonly struct ZoneUnitTurnEndedMessage
{
    public readonly IZoneDamageTarget Target;
    public readonly int Row;
    public readonly int Col;
}
```

### События (Events)

```csharp
// Состояние изменилось
public readonly struct ZoneStateChangedMessage
{
    public readonly ZoneState NewState;
}

// Клетки обновились
public readonly struct ZoneCellsUpdatedMessage
{
    public readonly BoardPosition[] WarningCells;
    public readonly BoardPosition[] DangerCells;
}

// Юнит получил урон
public readonly struct UnitTakeZoneDamageMessage
{
    public readonly IZoneDamageTarget Target;
    public readonly int Damage;
    public readonly BoardPosition Position;
}
```

## Использование

### Отправка команд

```csharp
// В начале боя
_publisher.Publish(new ZoneBattleStartedMessage());

// В начале хода
_publisher.Publish(new ZoneTurnStartedMessage(turn));

// При нанесении урона
_publisher.Publish(new ZoneDamageDealtMessage(turn));

// При завершении хода юнита
_publisher.Publish(new ZoneUnitTurnEndedMessage(target, row, col));
```

### Подписка на события

```csharp
public class ZonePresenter
{
    [Inject]
    public ZonePresenter(
        ISubscriber<ZoneStateChangedMessage> stateSubscriber,
        ISubscriber<ZoneCellsUpdatedMessage> cellsSubscriber,
        ISubscriber<UnitTakeZoneDamageMessage> damageSubscriber)
    {
        var bag = DisposableBag.CreateBuilder();
        stateSubscriber.Subscribe(OnZoneStateChanged).AddTo(bag);
        cellsSubscriber.Subscribe(OnZoneCellsUpdated).AddTo(bag);
        damageSubscriber.Subscribe(OnUnitTakeDamage).AddTo(bag);
    }

    private void OnZoneStateChanged(ZoneStateChangedMessage msg)
    {
        Debug.Log($"Zone state: {msg.NewState}");
    }

    private void OnZoneCellsUpdated(ZoneCellsUpdatedMessage msg)
    {
        // Обновить визуализацию зоны
    }

    private void OnUnitTakeDamage(UnitTakeZoneDamageMessage msg)
    {
        // Показать урон
    }
}
```

### Query-методы

```csharp
public class ZoneAIService
{
    private readonly IZoneShrinkQueryService _zoneQuery;

    [Inject]
    public ZoneAIService(IZoneShrinkQueryService zoneQuery)
    {
        _zoneQuery = zoneQuery;
    }

    public int GetCellCost(int row, int col)
    {
        var status = _zoneQuery.GetCellStatus(row, col);
        return status switch
        {
            CellStatus.Safe => 0,
            CellStatus.Warning => 50,
            CellStatus.Danger => 1000,
            _ => 0
        };
    }
}
```

## Регистрация в DI

```csharp
// В GameLifetimeScope
ZoneShrinkInstaller.RegisterZoneShrink(builder);
```

## Интеграция со StageConfig

```json
{
  "id": "stage_1",
  "type": "Duel",
  "board_id": "board_8x8",
  "zone_shrink_config_id": "stage_1_zone"
}
```

```csharp
public class StageService
{
    private readonly IZoneShrinkSystemFactory _zoneFactory;
    private ZoneShrinkSystem? _zoneSystem;

    [Inject]
    public StageService(IZoneShrinkSystemFactory zoneFactory, ...)
    {
        _zoneFactory = zoneFactory;
    }

    public void InitializeStage(StageConfig stageConfig)
    {
        _zoneSystem = _zoneFactory.Create(stageConfig.ZoneShrinkConfigId);
    }
}
```

## Конфигурация

```json
{
  "min_turn": 3,
  "max_turn": 6,
  "shrink_interval": 2,
  "zone_damage_flat": 5,
  "zone_damage_percent": 0.1,
  "safe_zone_min_size": 2,
  "board_size": 8
}
```

## LayerRingStrategy

Алгоритм послойного сужения:

### Шаг 0 (Warning rows)
```
W W W W W W W W  ← Warning
. . . . . . . .
. . . . . . . .
. . . . . . . .  ← Safe zone
. . . . . . . .
. . . . . . . .
. . . . . . . .
W W W W W W W W  ← Warning
```

### Шаг 1 (Danger rows + Warning cols)
```
D D D D D D D D  ← Danger
W . . . . . . W  ← Warning
W . . . . . . W  ← Warning
W . . . . . . W  ← Safe zone
W . . . . . . W  ← Warning
W . . . . . . W  ← Warning
W . . . . . . W  ← Warning
D D D D D D D D  ← Danger
```

### Шаг 2 (Danger rows + Danger cols)
```
D D D D D D D D  ← Danger
D . . . . . . D  ← Danger
D . . . . . . D  ← Danger
D . . . . . . D  ← Safe zone
D . . . . . . D  ← Danger
D . . . . . . D  ← Danger
D . . . . . . D  ← Danger
D D D D D D D D  ← Danger
```
