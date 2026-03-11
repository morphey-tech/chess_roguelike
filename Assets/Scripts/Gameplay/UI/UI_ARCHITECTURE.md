# UI Architecture - Итоговая документация

## Обзор архитектуры

```
ApplicationLifetimeScope (Application Scope)
│
├── Addressables System
│   └── AssetService (IAssetService)
│       └── Загрузка префабов из Addressables
│
├── UI System (Project.Gameplay.Gameplay.UI)
│   ├── UIAssetService (IUIAssetService)
│   │   └── Создание UI элементов с DI
│   │
│   ├── WindowsControllerInitializer
│   │   └─ InitializeAsync() → загружает WindowsController
│   │
│   ├── UIService (IUIService)
│   │   └─ InitializeAsync() → вызывает WindowsControllerInitializer
│   │
│   └── WindowsController (IWindowsController)
│       └── Управление окнами, создание, показ
│
└── Filters
    └── UIInitializationFilter
        └─ RunAsync() → вызывает IUIService.InitializeAsync()
```

## Зависимости

```
Project.Core (Core модуль)
    └─ Нет зависимостей от UI

Project.Gameplay.Gameplay.UI (UI модуль)
    └─ Зависит от Project.Core.Core.Logging
    └─ Зависит от Project.Core.Core.Assets (IAssetService)
```

## Порядок инициализации

```
1. ApplicationLifetimeScope.Configure()
   └─ Регистрация сервисов

2. GameSceneBootstrap.OnBootstrapAsync()
   └─ RunFilters()
       └─ UIInitializationFilter.RunAsync()
           └─ IUIService.InitializeAsync()
               └─ WindowsControllerInitializer.InitializeAsync()
                   └─ IUIAssetService.CreateAsync<WindowsController>("UI/Windows")
                       └─ AssetService.LoadAssetAsync<GameObject>("UI/Windows")
                   └─ WindowsController.InitAsync()
               └─ _initCompletionSource.TrySetResult()
           └─ Фильтр завершён

3. Игровые сервисы (IStartable.Start())
   └─ UI уже инициализирован, можно использовать
```

## Ключевые классы

### IUIAssetService
```csharp
public interface IUIAssetService
{
    UniTask<T> CreateAsync<T>(string address, Transform parent = null, ...) 
        where T : MonoBehaviour;
    UniTask<List<T>> CreateCollectionAsync<T>(...);
    T AttachController<T>(GameObject uiObject, ...);
    void Release(GameObject instance);
    void FlushCache();
}
```

### WindowsControllerInitializer
```csharp
public sealed class WindowsControllerInitializer
{
    public WindowsController Controller { get; }
    public bool IsInitialized { get; }
    
    public async UniTask InitializeAsync()
    {
        _controller = await _uiAssetService.CreateAsync<WindowsController>("UI/Windows");
        await _controller.InitAsync(_uiAssetService, _logService);
        _initialized = true;
    }
}
```

### IUIService
```csharp
public interface IUIService
{
    UniTask Initialized { get; }
    
    UniTask InitializeAsync();  // Явная инициализация
    
    T GetOrCreate<T>() where T : Window;
    UniTask<T> GetOrCreateAsync<T>() where T : Window;
    T Show<T>(bool immediate = false) where T : ParameterlessWindow;
    UniTask<T> ShowAsync<T>() where T : ParameterlessWindow;
    // ... остальные методы
}
```

### UIInitializationFilter
```csharp
public class UIInitializationFilter : IApplicationFilter
{
    private readonly IUIService _uiService;
    
    public async UniTask RunAsync()
    {
        await _uiService.InitializeAsync();  // Явный вызов
        await _uiService.Initialized;        // Ждём завершения
    }
}
```

## Адреса в Addressables

### Окна
- `"UI/Windows"` → WindowsController префаб
- `"UI/Windows/ArtifactsWindow"` → ArtifactsWindow
- `"UI/Windows/FigureInfoWindow"` → FigureInfoWindow

### UI Элементы
- `"UI/Artifacts/ArtifactItemView"`
- `"UI/Passives/PassiveIconView"`
- `"UI/Items/ItemView"`

## Примеры использования

### Внедрение IUIService в сервис
```csharp
public class FigureInfoPreviewService : IStartable
{
    private readonly IUIService _uiService;
    private FigureInfoWindow? _window;
    
    [Inject]
    public FigureInfoPreviewService(IUIService uiService)
    {
        _uiService = uiService;
    }
    
    void IStartable.Start()
    {
        // UI уже инициализирован фильтром
    }
    
    private async void ShowFigureInfo(Figure figure)
    {
        // Ленивое создание окна
        if (_window == null)
        {
            _window = await _uiService.GetOrCreateAsync<FigureInfoWindow>();
        }
        
        var model = new FigureInfoWindow.FigureInfoModel { ... };
        _window.Show(model);
    }
}
```

### Внедрение IUIAssetService в окно
```csharp
public class ArtifactsWindow : ParameterlessWindow
{
    private readonly IUIAssetService _uiAssetService;
    
    [Inject]
    public ArtifactsWindow(IUIAssetService uiAssetService)
    {
        _uiAssetService = uiAssetService;
    }
    
    private async UniTask CreateItemViews()
    {
        foreach (var artifact in artifacts)
        {
            // Создание элемента с загрузкой из Addressables
            var view = await _uiAssetService.CreateAsync<ArtifactItemView>(
                "UI/Artifacts/ArtifactItemView",
                parent: _contentParent,
                cancellationToken: ct);
            
            await view.Initialize(config, instance.Stack);
        }
    }
}
```

### Создание окна с параметрами
```csharp
public class StageOutcomeWindow : ParameterWindow<A1, A2>
{
    [Inject]
    private IUIService _uiService;
    
    public async UniTask<StageFlowAction> ShowAsync(Model model)
    {
        var window = await _uiService.GetOrCreateAsync<StageOutcomeWindow>();
        window.Show(model);
        return await window.GetResultAsync();
    }
}
```

## Регистрация в DI

### ApplicationLifetimeScope.cs
```csharp
protected override void Configure(IContainerBuilder builder)
{
    // UI Services
    builder.Register<UIAssetService>(Lifetime.Singleton).As<IUIAssetService>();
    builder.Register<WindowsControllerInitializer>(Lifetime.Singleton);
    builder.Register<UIService>(Lifetime.Singleton).As<IUIService>();
    
    // Filters
    builder.Register<UIInitializationFilter>(Lifetime.Transient);
}
```

### GameSceneBootstrap.cs
```csharp
private async UniTask RunFilters()
{
    _filterService.AddFilter<AddressablesInitFilter>();
    _filterService.AddFilter<AnnotationScanFilter>();
    _filterService.AddFilter<UIInitializationFilter>();  // UI инициализация
    
    await _filterService.RunAsync();
}
```

## Преимущества архитектуры

### ✅ Нет статики
- Все сервисы внедряются через DI
- Легко тестировать через моки
- Нет проблем с порядком инициализации

### ✅ Явная инициализация
- Фильтр явно вызывает `InitializeAsync()`
- Нет скрытых `.Forget()` или `IInitializable`
- Понятный поток выполнения

### ✅ Асинхронная загрузка
- Префабы загружаются из Addressables
- Нет синхронных `Resources.Load`
- Кеширование предотвращает дублирование

### ✅ Ленивое создание окон
- Окна создаются при первом запросе
- Нет preload всех окон при старте
- Экономия памяти

### ✅ Обработка ошибок
- Ошибки пробрасываются через фильтр
- Фильтр упадёт, если UI не инициализировался
- Нет проглоченных исключений

## Миграция со старого кода

### Было:
```csharp
// Статический вызов
var window = await UIService.GetOrCreateAsync<ArtifactsWindow>();

// Синхронное создание из префаба
var view = uiPrefabFactory.Instantiate(prefab, parent);
```

### Стало:
```csharp
// Через DI
public class MyClass
{
    private readonly IUIService _uiService;
    
    [Inject]
    public MyClass(IUIService uiService) => _uiService = uiService;
    
    public async UniTask MyMethod()
    {
        var window = await _uiService.GetOrCreateAsync<ArtifactsWindow>();
    }
}

// Асинхронное создание из Addressables
var view = await _uiAssetService.CreateAsync<ArtifactItemView>(
    "UI/Artifacts/ArtifactItemView",
    parent: _contentParent);
```

## Отладка

Включите Debug уровень логирования и ищите по префиксам:
- `[UIInitializationFilter]`
- `[UIService]`
- `[WindowsControllerInitializer]`
- `[UIAssetService]`
- `[AssetService]`

Полный лог успешной инициализации:
```
[UIInitializationFilter] Started
[UIInitializationFilter] Calling IUIService.InitializeAsync()...
[UIService] InitializeAsync started
[WindowsControllerInitializer] InitializeAsync started
[UIAssetService] CreateAsync<WindowsController> started, address='UI/Windows'
[AssetService] Loading asset: UI/Windows
[AssetService] Asset loaded: UI/Windows, instanceId=12345
[UIAssetService] CreateAsync completed successfully
[WindowsControllerInitializer] InitializeAsync completed successfully
[UIService] InitializeAsync completed
[UIInitializationFilter] UI initialization completed successfully
[UIInitializationFilter] Completed
```
