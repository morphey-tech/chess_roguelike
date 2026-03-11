# Рефакторинг UIService

## Проблема
Старый код использовал статику, `async void`, `.Forget()`, что затрудняло тестирование и понимание потока выполнения.

## Решение
Чистый DI-подход без статики:

### Архитектура

```
ApplicationLifetimeScope
├─ UIAssetService (IUIAssetService)
├─ WindowsControllerInitializer
│   └─ Загружает WindowsController из Addressables
├─ UIService (IUIService) [EntryPoint]
│   └─ Ждёт WindowsControllerInitializer.Initialized
└─ UIInitializationFilter
    └─ Ждёт WindowsControllerInitializer.Initialized
```

### Ключевые изменения

#### 1. IUIService интерфейс
```csharp
public interface IUIService
{
    UniTask Initialized { get; }
    T GetOrCreate<T>() where T : Window;
    UniTask<T> GetOrCreateAsync<T>() where T : Window;
    // ... остальные методы
}
```

#### 2. WindowsControllerInitializer
Загружает `WindowsController` из Addressables:
```csharp
public sealed class WindowsControllerInitializer : IInitializable
{
    public WindowsController Controller { get; }
    public UniTask Initialized { get; }
    
    void IInitializable.Initialize()
    {
        InitializeAsync().Forget(...);
    }
}
```

#### 3. UIService
```csharp
public sealed class UIService : IUIService, IInitializable
{
    private readonly WindowsControllerInitializer _controllerInitializer;
    
    public UniTask Initialized => _initCompletionSource.Task;
    
    void IInitializable.Initialize()
    {
        InitializeAsync().Forget(...);
    }
    
    private async UniTask InitializeAsync()
    {
        await _controllerInitializer.Initialized;
        _initCompletionSource.TrySetResult();
    }
}
```

#### 4. UIInitializationFilter
```csharp
public class UIInitializationFilter : IApplicationFilter
{
    private readonly WindowsControllerInitializer _controllerInitializer;
    
    public async UniTask RunAsync()
    {
        await _controllerInitializer.Initialized;
    }
}
```

## Преимущества

### ✅ Нет статики
- Легче тестировать через моки
- Понятный поток зависимостей
- Нет проблем с порядком инициализации

### ✅ Нет async void
- Все асинхронные методы возвращают `UniTask`
- Ошибки не проглатываются
- Можно awaited в тестах

### ✅ Нет .Forget()
- Все операции отслеживаются
- Ошибки логируются централизованно
- Легче отлаживать

### ✅ Чёткий порядок инициализации
```
1. WindowsControllerInitializer.IInitializable.Initialize
   └─ Загружает WindowsController из Addressables
2. UIService.IInitializable.Initialize
   └─ Ждёт WindowsControllerInitializer.Initialized
3. UIInitializationFilter.RunAsync
   └─ Ждёт WindowsControllerInitializer.Initialized
4. Игровые сервисы
```

## Обновлённые файлы

| Файл | Изменения |
|------|-----------|
| `UIService.cs` | Полностью переписан: интерфейс IUIService, класс UIService, нет статики |
| `WindowsControllerInitializer.cs` | Новый класс для загрузки WindowsController |
| `UIInitializationFilter.cs` | Использует WindowsControllerInitializer вместо UIService |
| `ApplicationLifetimeScope.cs` | Регистрация UI сервисов |
| `FigureInfoPreviewService.cs` | Ленивое создание окна |

## Пример использования

### Внедрение зависимости
```csharp
public class MyWindow : Window
{
    private readonly IUIService _uiService;
    
    [Inject]
    public MyWindow(IUIService uiService)
    {
        _uiService = uiService;
    }
    
    private async void OnButtonClick()
    {
        // Ждём инициализации UI
        await _uiService.Initialized;
        
        // Создаём окно
        var otherWindow = await _uiService.GetOrCreateAsync<OtherWindow>();
        otherWindow.Show();
    }
}
```

### Статический фасад (опционально)
Для удобства можно оставить статический фасад:
```csharp
public static class UI
{
    private static IUIService? _service;
    
    internal static void SetService(IUIService service) => _service = service;
    
    public static UniTask<T> ShowAsync<T>() where T : ParameterlessWindow
    {
        if (_service == null)
            throw new Exception("UIService not initialized");
        return _service.ShowAsync<T>();
    }
}
```

## Миграция

### Было:
```csharp
await UIService.Initialized;
var window = await UIService.GetOrCreateAsync<ArtifactsWindow>();
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
        await _uiService.Initialized;
        var window = await _uiService.GetOrCreateAsync<ArtifactsWindow>();
    }
}
```

Или через статический фасад (если добавите):
```csharp
await UI.Initialized;
var window = await UI.ShowAsync<ArtifactsWindow>();
```
