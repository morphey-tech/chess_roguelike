# Аудит проекта chess_roguelike

Дата: 2026-02-14  
Формат: технический аудит готовности фич и архитектуры

## 1) Статус фич (готово / частично / не готово)

| Фича | Статус | Подтверждение в коде | Что не хватает |
|---|---|---|---|
| Stage flow | Готово | `RunFlowService`, `StageFlowAction`, переходы `NextStage/Restart/GoHub` в `Assets/Scripts/Gameplay/Stage/Flow/RunFlowService.cs` | - |
| Combat | Готово | `CombatResolver` в `Assets/Scripts/Gameplay/Combat/CombatResolver.cs`, `AttackStep` в `Assets/Scripts/Gameplay/Turn/Steps/Impl/AttackStep.cs` | - |
| Turn system | Готово | `TurnSystem` в `Assets/Scripts/Gameplay/Turn/TurnSystem.cs` | - |
| Prepare phase | Готово | `PreparePlacementPhase` в `Assets/Scripts/Gameplay/Stage/Phase/PreparePlacementPhase.cs` | - |
| Loot + Economy | Готово | `LootService` и `EconomyService` в `Assets/Scripts/Gameplay/Loot/LootService.cs`, `Assets/Scripts/Gameplay/Economy/EconomyService.cs` | - |
| Input pipeline | Готово | `InputDispatcher : IStartable, IDisposable` в `Assets/Scripts/Gameplay/Input/InputDispatcher.cs` | - |
| Scene/bootstrap | Готово | Scene переходы используются в `RunFlowService`, `MainMenuController`, bootstrap-слое | - |
| Visual pipeline | Готово (с ограничением) | `VisualPipeline` в `Assets/Scripts/Gameplay/Visual/VisualPipeline.cs` | `SpeedMultiplier` пока не применен (TODO) |
| Save/Load ядро | Частично | `SaveService`, адаптеры и модели есть: `Assets/Scripts/Gameplay/Save/*` | Нет DI регистрации `FileSaveStorage` и `ISaveEnvironment`, нет E2E интеграции |
| Continue/New Game в меню | Частично | `MainMenuController`: `_continueButton.interactable = false`, обе кнопки вызывают `StartGame()` | Continue не грузит сейв |
| Settings | Частично | В `MainMenuController` есть `_settingsPanel` и переключение панелей | Нет реализованных настроек (громкость/графика/управление) |
| Audio | Частично | Точки расширения есть, но сквозной системы настроек/микширования не видно | Нет цельной аудио-политики |
| Автотесты | Не готово | Есть `Assets/Scripts/Tests/Editor.Tests.asmdef` | Нет test-файлов в `Assets/Scripts/Tests` |

## 2) Слабые архитектурные места

### High

1. **Статический `UIService` (service locator + race risk)**
   - Где: `Assets/Scripts/Gameplay/UI/UIService.cs`, использование в `Assets/Scripts/Unity/UI/GameUiService.cs`, `Assets/Scripts/Unity/Views/Presentations/FigureHealthPresenter.cs`
   - Проблема: глобальный статический доступ, `InitAsync(...).Forget()`, риск вызова UI до завершения инициализации.
   - Риск: нестабильный lifecycle, сложное unit-тестирование, скрытые зависимости.
   - Улучшение: перейти на DI-интерфейсы (`IWindowsController`/`IWorldUIProvider`), оставить `UIService` как временный адаптер.

### Medium

2. **Хрупкий startup через `OnContainerBuilt + Resolve<T>()`**
   - Где: `Assets/Scripts/Unity/Installers/GameLifetimeScope.cs`
   - Проблема: ручной порядок запуска и принудительный resolve большого списка сервисов.
   - Риск: легко забыть новый сервис, получить неявные баги подписок/lifecycle.
   - Улучшение: перевод стартовых сервисов на `IStartable`, выделить `StartupOrchestrator`.

3. **Save/Load не доведен до сквозного сценария**
   - Где: `Assets/Scripts/Unity/Installers/ApplicationLifetimeScope.cs`, `Assets/Scripts/Gameplay/Save/SaveSystem.cs`, `Assets/Scripts/Unity/Bootstrap/MainMenuController.cs`
   - Проблема: SaveService зарегистрирован, но его инфраструктурные зависимости (`FileSaveStorage`, `ISaveEnvironment`) не зарегистрированы; Continue отключен.
   - Риск: функционал формально есть, но для игрока не работает.
   - Улучшение: зарегистрировать хранилище/окружение в DI, подключить `Continue` к `HasSaveAsync/LoadAsync`.

4. **Смешение cleanup-семантики: shutdown vs stage transition**
   - Где: `Assets/Scripts/Gameplay/Stage/StageRuntimeResetService.cs`, `Assets/Scripts/Gameplay/Shutdown/GameShutdownCleanupService.cs`
   - Проблема: `GameShutdownCleanupService.Cleanup()` используется и при смене stage.
   - Риск: при расширении shutdown-логики можно сломать stage reload.
   - Улучшение: разделить на `IStageTransitionCleanup` и `IGameShutdownCleanup`.

5. **`FigureHealthPresenter` на polling через `Update()`**
   - Где: `Assets/Scripts/Unity/Views/Presentations/FigureHealthPresenter.cs`
   - Проблема: проверка HP каждый кадр + зависимость от статического UI.
   - Риск: лишняя нагрузка и неявный порядок UI-инициализации.
   - Улучшение: перейти на событие изменения HP (`HpChanged`) и DI-провайдер UI.

6. **`StagePhaseFactory` использует `IObjectResolver.Resolve(Type)`**
   - Где: `Assets/Scripts/Gameplay/Stage/Phase/StagePhaseFactory.cs`
   - Проблема: dependencies фаз скрыты, factory становится service locator.
   - Риск: сложнее рефакторить и тестировать фазовый пайплайн.
   - Улучшение: явные фабрики фазы (`IStagePhaseFactory`) или typed factories.

7. **`EntityInstances` слишком связан с `MonoBehaviour`**
   - Где: `Assets/Scripts/Gameplay/Presentation/EntityInstances.cs`
   - Проблема: приведение `((MonoBehaviour)presenter)` + `Debug.Log` в runtime-потоке.
   - Риск: слабая расширяемость презентационного слоя.
   - Улучшение: `IPresenterBinder` + централизованный `ILogger`.

### Low

8. **Ошибки cleanup подавляются в `OnDestroy`**
   - Где: `Assets/Scripts/Unity/Installers/GameLifetimeScope.cs`
   - Риск: silent failures в release.
   - Улучшение: хотя бы `Debug.LogException` или `ILogger.Error`.

9. **Fire-and-forget в критичных местах**
   - Где: `UIService`, `RunFlowService`, `StageReloadService`
   - Риск: незавершенные задачи при переходах сцен/закрытии.
   - Улучшение: ввести `CancellationToken` и явные точки await в orchestrator-слое.

## 3) Что в архитектуре уже хорошо

- Неплохое разделение слоев: domain-часть регистрируется через `GameplayContainerConfiguration`, Unity-детали в `GameLifetimeScope`.
- Используется `IGameUiService`, что позволяет держать domain в отрыве от конкретной UI реализации.
- Event-подход через MessagePipe уменьшает прямую связанность систем.
- Визуальный pipeline (`VisualScope` -> `VisualPipeline` -> `Executor`) удобен для эволюции эффектов.

## 4) Дорожная карта улучшений

## Быстрые победы (1-2 дня)

1. **Подключить рабочий Continue**
   - Минимальный шаг: в `MainMenuController` проверять `ISaveService.HasSaveAsync(defaultSlot)` и вызывать `LoadAsync` перед загрузкой сцены.
   - Эффект: пользовательский save/load путь становится рабочим.
   - Риск: неконсистентное состояние между сервисами после Load.
   - Проверка: сценарий New Game -> Save -> перезапуск -> Continue.

2. **Дорегистрировать Save инфраструктуру**
   - Минимальный шаг: в `ApplicationLifetimeScope` зарегистрировать `UnitySaveEnvironment` как `ISaveEnvironment`, и `FileSaveStorage` с `SavePath`.
   - Эффект: `SaveService` становится полностью разрешаемым DI.
   - Риск: ошибка пути/прав доступа.
   - Проверка: smoke тест `SaveAsync/LoadAsync/GetSlotsAsync`.

3. **Убрать silent catch в cleanup**
   - Минимальный шаг: заменить пустой catch в `GameLifetimeScope.OnDestroy()` на логирование.
   - Эффект: наблюдаемость ошибок shutdown.
   - Риск: шум в логах при ожидаемых ошибках.
   - Проверка: принудительная ошибка cleanup и проверка лога.

## Средний горизонт (1-2 спринта)

1. **Дестатизировать UI слой**
   - Минимальный шаг: внедрить `IWorldUIProvider` и перевести `FigureHealthPresenter`/`GameUiService` на DI.
   - Эффект: меньше гонок и выше тестопригодность.
   - Риск: поломка старых вызовов `UIService.*`.
   - Проверка: playmode smoke всех UI окон + health bars.

2. **Стабилизировать startup**
   - Минимальный шаг: сервисы с подписками переводить на `IStartable`; уменьшить ручные `Resolve<T>()`.
   - Эффект: прозрачный lifecycle.
   - Риск: изменится порядок подписок.
   - Проверка: запуск сцены, prepare phase, first turn, reload stage.

3. **Разделить cleanup responsibilities**
   - Минимальный шаг: выделить отдельный service для stage transition cleanup.
   - Эффект: предсказуемость при переходах и shutdown.
   - Риск: забытые очистки.
   - Проверка: циклы NextStage/Restart/ExitToMenu xN без утечек.

## Стратегические улучшения (2+ спринта)

1. **Тестовый каркас для domain-логики**
   - Минимальный шаг: добавить unit tests для `CombatResolver`, `TurnPatternResolver`, `ClickIntentResolver`, `LootService`.
   - Эффект: безопасный рефакторинг боевой логики.
   - Риск: потребуются тестовые doubles и стабилизация API.
   - Проверка: green CI на каждом PR + тесты на ключевые правила боя.

2. **Явные фабрики фаз**
   - Минимальный шаг: заменить `Resolve(Type)` на typed-фабрики.
   - Эффект: меньше скрытых зависимостей, лучше читаемость pipeline.
   - Риск: затронет DI конфигурацию и тесты фаз.
   - Проверка: полный прогон stage pipeline для разных типов stage.

## 5) Приоритет внедрения

1. Save/Continue E2E (чтобы фича реально работала для игрока).  
2. Startup и UI lifecycle (устранение нестабильности и гонок).  
3. Разделение cleanup semantics.  
4. Базовый набор unit tests для domain.

## 6) Критерий "стало лучше"

- Continue загружает сохранение, а не дублирует New Game.
- Нет ошибок DI при резолве `SaveService`.
- UI не зависит от статических глобальных точек доступа в новых/обновленных модулях.
- Startup предсказуем, без ручного "догоняющего" resolve.
- Есть минимум 8-12 unit тестов на core gameplay rules.

