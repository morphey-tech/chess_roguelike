# Логи для отладки UI

## Порядок инициализации (обновлённый)

1. **Фильтры (GameSceneBootstrap.RunFilters)**
   - `AddressablesInitFilter`
   - `AnnotationScanFilter`
   - **`UIInitializationFilter`** ← новый фильтр
2. **AssetService.IInitializable.Initialize** → `InitializeAsync`
3. **Addressables.InitializeAsync** → загрузка каталога
4. **UIService конструктор** → `InitAsync`
5. **UIAssetService.CreateAsync<WindowsController>** → загрузка префаба
6. **WindowsController.InitAsync**
7. **UIService.Initialized** → разблокировка ожидателей
8. **UIInitializationFilter.RunAsync** → завершение фильтра
9. **FigureInfoPreviewService.InitializeAsync** → создание `FigureInfoWindow`

## Фильтр UIInitializationFilter

Гарантирует, что UI полностью инициализирован перед запуском игровых сервисов.

### Логи фильтра:
```
[UIInitializationFilter] Started
[UIInitializationFilter] Waiting for UIService.Initialized...
[UIInitializationFilter] UIService initialized successfully
[UIInitializationFilter] Completed
```

### Преимущества:
- ✅ Гарантирует инициализацию UI до начала работы сервисов
- ✅ Явная ошибка, если UI не инициализировался
- ✅ Убирает гонку условий между сервисами
- ✅ Централизованное управление инициализацией

## Добавленные логи

### UIService
```
[UIService] Constructor called, IUIAssetService injected
[UIService] InitAsync started
[UIService] LoadControllerAsync started
[UIService] Creating new WindowsController
[UIService] IUIAssetService available, loading 'UI/Windows'
[UIService] WindowsController created: {instance}
[UIService] Calling WindowsController.InitAsync
[UIService] WindowsController.InitAsync completed
[UIService] LoadControllerAsync completed
[UIService] Initialization completed successfully
```

### UIAssetService
```
[UIAssetService] CreateAsync<WindowsController> started, address='UI/Windows'
[UIAssetService] CreateAsyncInternal started, address='UI/Windows'
[UIAssetService] No cache, loading from Addressables...
[UIAssetService] Prefab loaded: {name}
[UIAssetService] CreateAsyncInternal completed successfully
[UIAssetService] CreateAsyncInternal completed, gameObject={name}
[UIAssetService] CreateAsync<WindowsController> completed successfully
```

### AssetService
```
[AssetService] IInitializable.Initialize called
[AssetService] InitializeAsync started
[AssetService] InitCatalog attempt 1/3
[AssetService] Addressables.InitializeAsync called
[AssetService] Addressables.InitializeAsync completed, locations={count}
[AssetService] Addressables catalog initialized successfully
[AssetService] Loading asset: UI/Windows
[AssetService] Asset loaded: UI/Windows, instanceId={id}
```

### WindowsController
```
[WindowsController] InitAsync started
[WindowsController] InitAsync completed
```

### FigureInfoPreviewService
```
[FigureInfoPreviewService] InitializeAsync started
[FigureInfoPreviewService] FigureInfoConfigRepository loaded
[FigureInfoPreviewService] PassiveConfigRepository loaded
[FigureInfoPreviewService] Creating FigureInfoWindow...
[FigureInfoPreviewService] FigureInfoWindow created
```

## Возможные проблемы и решения

### "UI is not valid"
**Причина:** `UIService.Initialized` завершился с ошибкой

**Проверьте логи:**
1. `[AssetService] Addressables catalog initialized successfully` — был ли инициализирован каталог?
2. `[UIService] IUIAssetService available` — был ли доступен сервис?
3. `[UIAssetService] Prefab loaded` — загрузился ли префаб?
4. `[WindowsController] InitAsync completed` — инициализировался ли контроллер?
5. `[UIInitializationFilter] UIService initialized successfully` — прошёл ли фильтр?

### "Failed to load prefab: UI/Windows"
**Причина:** Префаб не найден в Addressables

**Решение:**
- Проверьте адрес в Addressables: должен быть `"UI/Windows"`
- Проверьте, что префаб помечен как Addressable

### "IUIAssetService is NULL"
**Причина:** Неправильный порядок регистрации в DI

**Решение:**
- Убедитесь, что `UIAssetService` регистрируется **ДО** `GameplayContainerConfiguration.Register`
- Проверьте `GameLifetimeScope.ConfigureServices`

## Как смотреть логи

Включите Debug уровень логирования в вашем логгере (ZLogger/Serilog и т.д.)

Ищите по префиксам:
- `[UIService]`
- `[UIAssetService]`
- `[AssetService]`
- `[WindowsController]`
- `[FigureInfoPreviewService]`
- `[UIInitializationFilter]`
