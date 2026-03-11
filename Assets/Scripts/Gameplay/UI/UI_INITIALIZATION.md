# Инициализация UI через фильтр

## Архитектура

```
ApplicationLifetimeScope
├─ UIAssetService (IUIAssetService)
├─ WindowsControllerInitializer
│   └─ InitializeAsync() → загружает WindowsController
├─ UIService (IUIService)
│   └─ InitializeAsync() → вызывает WindowsControllerInitializer.InitializeAsync()
└─ UIInitializationFilter
    └─ RunAsync() → вызывает IUIService.InitializeAsync()
```

## Порядок инициализации

1. **ApplicationLifetimeScope** создаётся → регистрируются сервисы
2. **UIInitializationFilter.RunAsync()** вызывается из `GameSceneBootstrap.RunFilters()`
3. **IUIService.InitializeAsync()** → инициализирует UI
4. **WindowsControllerInitializer.InitializeAsync()** → загружает префаб
5. **WindowsController.InitAsync()** → инициализирует контроллер
6. **IUIService.Initialized** → разблокируется
7. **UIInitializationFilter** завершается
8. **Игровые сервисы** → могут использовать UI

## Код

### WindowsControllerInitializer
```csharp
public sealed class WindowsControllerInitializer
{
    public WindowsController Controller { get; }
    public bool IsInitialized { get; }
    
    public async UniTask InitializeAsync()
    {
        if (_initialized) return;
        
        _controller = await _uiAssetService.CreateAsync<WindowsController>("UI/Windows");
        await _controller.InitAsync(_uiAssetService, _logService);
        _initialized = true;
    }
}
```

### UIService
```csharp
public sealed class UIService : IUIService
{
    public UniTask Initialized => _initCompletionSource.Task;
    
    public async UniTask InitializeAsync()
    {
        await _controllerInitializer.InitializeAsync();
        _initCompletionSource.TrySetResult();
    }
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

## Преимущества

### ✅ Полный контроль над флоу
- Фильтр явно вызывает инициализацию
- Нет скрытых `.Forget()` или `IInitializable`
- Понятный порядок выполнения

### ✅ Нет EntryPoint
- Сервисы не инициализируются автоматически
- Нет гонки между `IInitializable` и фильтрами
- Явная зависимость: фильтр → сервис → контроллер

### ✅ Легко тестировать
- Можно замокать `IUIService` и проверить вызов `InitializeAsync()`
- Можно проверить состояние `IsInitialized`
- Нет скрытой асинхронной логики

### ✅ Обработка ошибок
- Ошибки пробрасываются через фильтр
- Фильтр упадёт, если UI не инициализировался
- Нет проглоченных исключений

## Диаграмма последовательности

```
GameSceneBootstrap
    └─ RunFilters()
        └─ UIInitializationFilter.RunAsync()
            └─ IUIService.InitializeAsync()
                └─ WindowsControllerInitializer.InitializeAsync()
                    └─ IUIAssetService.CreateAsync<WindowsController>()
                        └─ AssetService.LoadAssetAsync<GameObject>("UI/Windows")
                    └─ WindowsController.InitAsync()
                └─ _initCompletionSource.TrySetResult()
            └─ await _uiService.Initialized
        └─ Filter completed
    └─ Игровые сервисы запускаются
```

## Пример использования в сервисах

```csharp
public class FigureInfoPreviewService : IStartable
{
    private readonly IUIService _uiService;
    
    [Inject]
    public FigureInfoPreviewService(IUIService uiService)
    {
        _uiService = uiService;
    }
    
    void IStartable.Start()
    {
        // UI уже инициализирован фильтром
        // Можно безопасно использовать
    }
    
    private async void ShowFigureInfo(Figure figure)
    {
        // Ждём инициализации (на всякий случай)
        await _uiService.Initialized;
        
        var window = await _uiService.GetOrCreateAsync<FigureInfoWindow>();
        window.Show(model);
    }
}
```

## Миграция

### Было (с EntryPoint):
```csharp
// UIService инициализировался автоматически через IInitializable
// Фильтр ждал UIService.Initialized
// Непонятно, когда точно произошла инициализация
```

### Стало (явный вызов):
```csharp
// Фильтр явно вызывает IUIService.InitializeAsync()
// Гарантированный порядок инициализации
// Понятный поток выполнения
```
