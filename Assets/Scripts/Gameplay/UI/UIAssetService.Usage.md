# UIAssetService - Улучшенная версия на основе LiteUI.UIService

## Адреса в Addressables

### Окна (Windows)
Используют **путь к префабу в Addressables**:
- `"UI/Windows"` → `WindowsController` (префаб с компонентом `WindowsController`)
- `"UI/Windows/ArtifactsWindow"` → `ArtifactsWindow`
- `"UI/Windows/FigureInfoWindow"` → `FigureInfoWindow`

**Важно:** Адреса должны быть настроены в Addressables Group соответственно!

### UI Элементы
Используют **путь в формате**: `"UI/<Category>/<ItemName>"`:
- `"UI/Artifacts/ArtifactItemView"`
- `"UI/Passives/PassiveIconView"`

---

## Архитектура

```
UIService (статический фасад)
    ↓
WindowsController (IWindowsController)
    ↓
IUIAssetService
    ↓
IAssetService (Addressables)
```

---

## Основные возможности UIAssetService

### 1. Асинхронное создание окон
```csharp
// Создание окна через WindowsController
var window = await windowsController.GetOrCreateWindowAsync<ArtifactsWindow>();
// → загрузит префаб "UI/Windows/ArtifactsWindow" из Addressables
```

### 2. Создание UI элементов внутри окон
```csharp
// В ArtifactsWindow.cs:
var view = await _uiAssetService.CreateAsync<ArtifactItemView>(
    "UI/Artifacts/ArtifactItemView",
    parent: _contentParent,
    cancellationToken: ct
);
await view.Initialize(config, instance.Stack);
```

### 3. Создание коллекций
```csharp
var itemViews = await uiAssetService.CreateCollectionAsync<ItemView>(
    "UI/Items/ItemView",
    itemsParams: items.Select(item => new object[] { item.Id, item.Data }).ToList(),
    parent: scrollContent,
    cancellationToken: ct
);
```

### 4. AttachController
```csharp
// Добавить контроллер на существующий GameObject
var controller = uiAssetService.AttachController<MyController>(
    gameObject, 
    new object[] { param1, param2 }
);
```

### 5. Управление кешем
```csharp
uiAssetService.Caching = false;  // Отключить кеш
uiAssetService.FlushCache();     // Очистить кеш
uiAssetService.Release(gameObject);
```

---

## Обновлённые файлы

| Файл | Изменения |
|------|-----------|
| **IAssetService.cs** | + `InitializeAsync()`, + `CancellationToken`, + `IsRemote()` |
| **AssetService.cs** | Полностью переписан с retry-логикой и кешированием |
| **IUIAssetService.cs** | + `CreateAsync()`, + `CreateCollectionAsync()`, + `AttachController()` |
| **UIAssetService.cs** | Новая реализация с Fluent API моделями |
| **IWindowController.cs** | `InitAsync(IUIAssetService, ILogService)` — убран `IAssetService` |
| **WindowController.cs** | Использует адрес префаба из Addressables |
| **Window.cs** | `Init(ILogService)` — убран `IAssetService` |
| **UIService.cs** | Передаёт `IUIAssetService` в `WindowsController` |
| **FigureInfoWindow.cs** | Обновлено на `CreateAsync()` |
| **ArtifactsWindow.cs** | Обновлено на `CreateAsync()` |
| **GameLifetimeScope.cs** | `UIAssetService` регистрируется ДО `GameplayContainerConfiguration` |

---

## Ключевые изменения

### 1. Убран `IAssetService` из `WindowController`
Теперь только `IUIAssetService` — более высокоуровневый сервис.

### 2. Адреса в Addressables
- Окна: `"UI/Windows"`, `"UI/Windows/ArtifactsWindow"`, и т.д.
- Элементы: `"UI/Artifacts/ArtifactItemView"`, `"UI/Passives/PassiveIconView"`

### 3. Порядок регистрации в DI
`UIAssetService` должен быть зарегистрирован **ДО** `UIService`, т.к. `UIService` зависит от `IUIAssetService`.

### 4. Упрощённая инициализация
- **Было:** `Init(IAssetService, ILogService)`
- **Стало:** `Init(ILogService)`

---

## Пример настройки Addressables

### Группы:
```
UI_Windows/
  - Windows.prefab              (address: "UI/Windows")
  - ArtifactsWindow.prefab      (address: "UI/Windows/ArtifactsWindow")
  - FigureInfoWindow.prefab     (address: "UI/Windows/FigureInfoWindow")

UI_Elements/
  - ArtifactItemView.prefab     (address: "UI/Artifacts/ArtifactItemView")
  - PassiveIconView.prefab      (address: "UI/Passives/PassiveIconView")
```

---

## Отличия от LiteUI.UIService

| Функция | LiteUI.UIService | Наш UIAssetService |
|---------|------------------|-------------------|
| AddressableManager | Свой | Использует IAssetService |
| BindingService | Свой | Использует VContainer.InjectGameObject |
| UIMetaRegistry | Есть (сканирование атрибутов) | Нет (упрощено) |
| Init метод | Через UICreatedAttribute | Через метод Init() |
| Кеш префабов | Есть | Есть |
| Отмена операций | CancellationToken | CancellationToken |
| Создание коллекции | Есть | Есть |
| Fluent API модели | Есть | Есть (упрощённые) |
