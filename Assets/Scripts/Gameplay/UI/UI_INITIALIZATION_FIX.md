# Исправление инициализации UI

## Проблема
`FigureInfoPreviewService` вызывал `UIService.GetOrCreateAsync<FigureInfoWindow>()` в `InitializeAsync()`, который запускался в `IStartable.Start()` **до** завершения фильтра `UIInitializationFilter`.

Это вызывало гонку условий:
1. `FigureInfoPreviewService.Start()` → `InitializeAsync()` → `GetOrCreateAsync<FigureInfoWindow>()`
2. `UIService.GetController()` → выбрасывает `"UI is not valid"`
3. Фильтр `UIInitializationFilter` ещё не завершился

## Решение

### 1. Ленивая инициализация окна
Окно `FigureInfoWindow` теперь создаётся не в `InitializeAsync()`, а при первом вызове `ShowFigureInfo()`:

```csharp
private async void ShowFigureInfo(Figure figure)
{
    // Ленивое создание окна при первом показе
    if (_window == null)
    {
        _window = await UIService.GetOrCreateAsync<FigureInfoWindow>();
    }
    
    // ... показ окна
}
```

### 2. Фильтр UIInitializationFilter
Гарантирует, что UI инициализирован до запуска игровых сервисов:

```csharp
public class UIInitializationFilter : IApplicationFilter
{
    public async UniTask RunAsync()
    {
        await UIService.Initialized;  // Ждём инициализации
        
        // Проверка с retry
        while (!UIService.IsValid && retryCount < maxRetries)
        {
            await UniTask.Delay(100ms);
        }
    }
}
```

### 3. Порядок инициализации
```
1. AddressablesInitFilter
2. AnnotationScanFilter
3. UIInitializationFilter  ← UI инициализирован
4. IStartable.Start()       ← FigureInfoPreviewService.Start()
   └─ InitializeAsync()     ← только загрузка конфигов
5. Игровой цикл
   └─ OnCellClicked()
      └─ ShowFigureInfo()   ← ленивое создание окна
```

## Обновлённые файлы

| Файл | Изменения |
|------|-----------|
| `FigureInfoPreviewService.cs` | Убрано `GetOrCreateAsync` из `InitializeAsync()`, ленивое создание в `ShowFigureInfo()` |
| `UIInitializationFilter.cs` | Добавлен retry для проверки `IsValid` |
| `UIService.cs` | Добавлены логи, защита от раннего доступа |
| `UIAssetService.cs` | Добавлена обработка ошибок в `CreateAsync` |
| `AssetService.cs` | Добавлены логи инициализации Addressables |

## Логи для отладки

```
[UIInitializationFilter] Started
[UIInitializationFilter] Waiting for UIService.Initialized...
[UIService] Constructor called, IUIAssetService injected
[UIService] InitAsync started
[UIAssetService] CreateAsync<WindowsController> started
[AssetService] Loading asset: UI/Windows
[AssetService] Asset loaded: UI/Windows
[UIAssetService] CreateAsync completed successfully
[UIService] WindowsController created
[UIService] Initialization completed, IsValid=True
[UIInitializationFilter] UIService.Initialized task completed
[UIInitializationFilter] Completed successfully
[FigureInfoPreviewService] InitializeAsync started
[FigureInfoPreviewService] InitializeAsync completed (window will be created on first show)
```

При первом показе окна:
```
[FigureInfoPreviewService] Creating FigureInfoWindow on first show...
[UIAssetService] CreateAsync<FigureInfoWindow> started
[AssetService] Loading asset: UI/Windows/FigureInfoWindow
[FigureInfoPreviewService] FigureInfoWindow created
```
