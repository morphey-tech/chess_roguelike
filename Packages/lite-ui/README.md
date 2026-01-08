# LiteUI

Легковесная UI система для Unity с биндингом компонентов, диалогами, попапами, нотификациями и анимациями.

## Содержание

- [Quick Start — Быстрый старт](#quick-start--быстрый-старт)
- [Установка](#установка)
- [Зависимости](#зависимости)
- [Архитектура](#архитектура)
- [DialogManager — Модальные диалоги](#dialogmanager--модальные-диалоги)
- [PopupManager — Тултипы и контекстные меню](#popupmanager--тултипы-и-контекстные-меню)
- [OverlayManager — Полноэкранные оверлеи](#overlaymanager--полноэкранные-оверлеи)
- [SidePanelManager — Выдвижные панели](#sidepanelmanager--выдвижные-панели)
- [NotificationService — Тост-уведомления](#notificationservice--тост-уведомления)
- [Screen — Переключение режимов](#screen--переключение-режимов)
- [Binding System — Автобиндинг UI](#binding-system--автобиндинг-ui)
- [UITweenPlayer — Анимации](#uitweenplayer--анимации)

---

## Quick Start — Быстрый старт

### Шаг 1: Настройка проекта

1. Убедись что установлены зависимости (UniTask, Addressables, DOTween, VContainer)
2. Добавь в Player Settings → Scripting Define Symbols: `UNITASK_ADDRESSABLE_SUPPORT`

### Шаг 2: Создай UI контейнер на сцене

```
UIRoot (Canvas)
├── DialogContainer (пустой RectTransform, stretch all)
│   └── DialogBackgoundShade (Image, чёрный, alpha 0)
├── PopupContainer
├── OverlayContainer
└── SidePanelContainer
```

### Шаг 3: Зарегистрируй менеджеры в VContainer

```csharp
public class UILifetimeScope : LifetimeScope
{
    [SerializeField] private GameObject _dialogContainer;
    [SerializeField] private GameObject _popupContainer;
    
    protected override void Configure(IContainerBuilder builder)
    {
        // Регистрация сервисов
        builder.Register<AddressableManager>(Lifetime.Singleton);
        builder.Register<UIService>(Lifetime.Singleton);
        builder.Register<UIMetaRegistry>(Lifetime.Singleton);
        builder.Register<BindingService>(Lifetime.Singleton);
        builder.Register<DialogManager>(Lifetime.Singleton);
        builder.Register<PopupManager>(Lifetime.Singleton);
        builder.Register<OverlayManager>(Lifetime.Singleton);
        builder.Register<SidePanelManager>(Lifetime.Singleton);
        
        // MessagePipe для событий диалогов (опционально)
        builder.RegisterMessagePipe();
        builder.RegisterMessageBroker<string, DialogEvent>();
    }
    
    private void Start()
    {
        var dialogManager = Container.Resolve<DialogManager>();
        dialogManager.AttachRootContainer(_dialogContainer);
        
        var popupManager = Container.Resolve<PopupManager>();
        popupManager.AttachRootContainer(_popupContainer);
    }
}
```

### Шаг 4: Создай префаб диалога

1. Создай префаб: `Assets/UI/Dialogs/ConfirmDialog.prefab`
2. Структура префаба:

```
ConfirmDialog (RectTransform)
├── Background (Image)
├── TitleText (TMP_Text)
├── MessageText (TMP_Text)
├── ConfirmButton (Button)
└── CancelButton (Button)
```

3. Добавь на корень компонент `UITweenAnimator` и настрой анимации (опционально)

### Шаг 5: Сделай префаб Addressable

1. Выдели префаб в Project
2. В Inspector отметь галку "Addressable"
3. Задай адрес: `Dialogs/ConfirmDialog`

### Шаг 6: Создай контроллер диалога

```csharp
using LiteUI.Dialog.Attributes;
using LiteUI.Dialog.Controllers;
using LiteUI.Binding.Attributes;
using UnityEngine;
using System;

[UIController("Dialogs/ConfirmDialog")]  // Addressable путь
[UIDialog]                                // Маркер диалога
public class ConfirmDialog : AnimatedDialogBase  // Или просто MonoBehaviour
{
    [UIComponentBinding("TitleText")]
    private TMPro.TMP_Text _title;
    
    [UIComponentBinding("MessageText")]
    private TMPro.TMP_Text _message;
    
    private Action _onConfirm;
    private Action _onCancel;
    
    [UICreated]
    private void Init(string title, string message, Action onConfirm = null, Action onCancel = null)
    {
        _title.text = title;
        _message.text = message;
        _onConfirm = onConfirm;
        _onCancel = onCancel;
    }
    
    [UIOnClick("ConfirmButton")]
    private void OnConfirm()
    {
        _onConfirm?.Invoke();
        // Нужно вызвать Hide через DialogManager
    }
    
    [UIOnClick("CancelButton")]
    private void OnCancel()
    {
        _onCancel?.Invoke();
    }
}
```

### Шаг 7: Покажи диалог из кода

```csharp
public class GameController : MonoBehaviour
{
    [Inject] private DialogManager _dialogManager;
    
    public void ShowExitConfirm()
    {
        _dialogManager.Show<ConfirmDialog>(
            "Выход",                           // title
            "Вы уверены что хотите выйти?",   // message
            () => Application.Quit(),          // onConfirm
            null                               // onCancel
        );
    }
    
    public async void ShowAndWait()
    {
        var dialog = await _dialogManager.ShowModalAsync<ConfirmDialog>("Title", "Message");
        // dialog готов к использованию
    }
}
```

---

## Установка

Добавь в `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.liteui.core": "file:lite-ui"
  }
}
```

## Зависимости

| Пакет | Обязательный | Назначение |
|-------|--------------|------------|
| UniTask | ✅ | Async/await |
| Addressables | ✅ | Загрузка UI |
| DOTween | ✅ | Анимации |
| VContainer | ✅ | DI |
| MessagePipe | ⚪ | События диалогов |

---

## Архитектура

```
┌─────────────────────────────────────────────────────────┐
│                      UI Managers                         │
├──────────┬──────────┬──────────┬──────────┬─────────────┤
│ Dialog   │ Popup    │ Overlay  │ SidePanel│ Notification│
│ Manager  │ Manager  │ Manager  │ Manager  │ Service     │
├──────────┴──────────┴──────────┴──────────┴─────────────┤
│                    BindingService                        │
├─────────────────────────────────────────────────────────┤
│                 UIService + Addressables                 │
└─────────────────────────────────────────────────────────┘
```

### Когда что использовать

| Тип | Стекается | Блокирует ввод | Позиция | Юзкейс |
|-----|-----------|----------------|---------|--------|
| **Dialog** | ✅ Да | ✅ Да | Центр | Подтверждения, формы, алерты |
| **Popup** | ❌ Один | ❌ Нет | У элемента | Тултипы, контекстное меню |
| **Overlay** | ❌ Один | ✅ Да | Fullscreen | Лоадинг, переходы |
| **SidePanel** | ✅ По сторонам | ❌ Нет | Край экрана | Меню, инвентарь |
| **Notification** | ✅ Очередь | ❌ Нет | Угол | Ачивки, награды |

---

## DialogManager — Модальные диалоги

Стек модальных окон с затемнением фона и блокировкой ввода.

### Создание диалога

```csharp
using LiteUI.Dialog.Attributes;
using LiteUI.Dialog.Controllers;
using LiteUI.Binding.Attributes;
using LiteUI.Tweening;
using UnityEngine;

// UIController — путь к Addressable префабу
// UIDialog — маркер что это диалог
[UIController("Dialogs/ConfirmDialog")]
[UIDialog]
public class ConfirmDialog : AnimatedDialogBase
{
    [UIComponentBinding("TitleText")]
    private TMPro.TMP_Text _title;
    
    [UIComponentBinding("MessageText")]
    private TMPro.TMP_Text _message;
    
    private System.Action _onConfirm;
    
    [UICreated]
    private void Init(string title, string message, System.Action onConfirm)
    {
        _title.text = title;
        _message.text = message;
        _onConfirm = onConfirm;
    }
    
    [UIOnClick("ConfirmButton")]
    private void OnConfirm()
    {
        _onConfirm?.Invoke();
    }
    
    [UIOnClick("CancelButton")]
    private void OnCancel()
    {
        // Просто закрываем
    }
}
```

### Варианты анимаций диалога

**Вариант 1: AnimatedDialogBase (рекомендуется)**
```csharp
[UIController("Dialogs/MyDialog")]
[UIDialog]
public class MyDialog : AnimatedDialogBase
{
    // _showAnimation и _hideAnimation задаются в инспекторе
    // Если не заданы — используются PopIn/PopOut по умолчанию
}
```

**Вариант 2: UITweenAnimator на префабе**
```csharp
[UIController("Dialogs/MyDialog")]
[UIDialog]
public class MyDialog : MonoBehaviour
{
    // На префабе должен быть компонент UITweenAnimator
}
```

**Вариант 3: Своя реализация IUIAnimatable**
```csharp
[UIController("Dialogs/MyDialog")]
[UIDialog]
public class MyDialog : MonoBehaviour, IUIAnimatable
{
    public async UniTask AnimateShow() { /* своя логика */ }
    public async UniTask AnimateHide() { /* своя логика */ }
}
```

### Показ диалога

```csharp
public class GameService
{
    [Inject] private DialogManager _dialogManager;
    
    public void ShowConfirm()
    {
        // Fire-and-forget
        _dialogManager.Show<ConfirmDialog>("Выход", "Точно выйти?", () => Application.Quit());
    }
    
    public async UniTask ShowAndWait()
    {
        // Ждём пока диалог создастся
        var dialog = await _dialogManager.ShowModalAsync<ConfirmDialog>("Title", "Message", null);
    }
    
    public void CloseDialog(MonoBehaviour dialog)
    {
        _dialogManager.Hide(dialog);
    }
    
    public void CloseAllDialogs()
    {
        _dialogManager.HideAll();
    }
}
```

### Методы DialogManager

| Метод | Описание |
|-------|----------|
| `Show<T>(params)` | Показать в очередь (после текущего) |
| `ShowModal<T>(params)` | Показать сразу поверх |
| `ShowModalAsync<T>(params)` | Показать и дождаться создания |
| `Hide(dialog)` | Скрыть конкретный диалог |
| `HideAll()` | Закрыть все диалоги |
| `HasDialog<T>()` | Есть ли диалог типа T |
| `HasAnyDialog()` | Есть ли хоть один диалог |
| `MuteDialogs()` | Приостановить показ новых |
| `UnmuteDialogs()` | Возобновить показ |

---

## PopupManager — Тултипы и контекстные меню

Одиночный попап, привязанный к UI элементу. Новый попап автоматически скрывает предыдущий.

### Создание попапа

```csharp
using LiteUI.Popup.Panel;
using LiteUI.Binding.Attributes;
using LiteUI.Tweening;
using Cysharp.Threading.Tasks;
using UnityEngine;

[UIController("Popups/TooltipPopup")]
public class TooltipPopup : MonoBehaviour, IPopup
{
    [UIComponentBinding("Text")]
    private TMPro.TMP_Text _text;
    
    private UITweenPlayer _tweenPlayer;
    
    [UICreated]
    private void Init(string text)
    {
        _text.text = text;
        _tweenPlayer = new UITweenPlayer(gameObject);
    }
    
    public async UniTask Show(RectTransform target, PopupAlign align, Vector2 offset)
    {
        transform.position = target.position + (Vector3)offset;
        await _tweenPlayer.Play(UITweenPresets.FadeIn());
    }
    
    public async UniTask Hide()
    {
        await _tweenPlayer.Play(UITweenPresets.FadeOut());
    }
}
```

### Использование

```csharp
public class ItemSlot : MonoBehaviour
{
    [Inject] private PopupManager _popupManager;
    
    private string _tooltipId;
    
    public void OnPointerEnter()
    {
        _tooltipId = _popupManager.Show<TooltipPopup>(
            target: GetComponent<RectTransform>(),
            defaultAlign: PopupAlign.Top,
            offset: new Vector2(0, 10),
            parameters: "Меч +5 к атаке"
        );
    }
    
    public void OnPointerExit()
    {
        _popupManager.Hide(_tooltipId);
    }
}
```

---

## OverlayManager — Полноэкранные оверлеи

Полноэкранные UI поверх всего (лоадинг, затемнение). Кешируются для быстрого показа.

### Создание оверлея

```csharp
using LiteUI.Overlay;
using LiteUI.Binding.Attributes;
using UnityEngine;

[UIController("Overlays/LoadingOverlay")]
public class LoadingOverlay : MonoBehaviour, IOverlayController
{
    [UIComponentBinding("StatusText")]
    private TMPro.TMP_Text _statusText;
    
    public bool IsHiding { get; private set; }
    
    public void Show()
    {
        IsHiding = false;
        gameObject.SetActive(true);
    }
    
    public void Hide()
    {
        IsHiding = true;
        gameObject.SetActive(false);
    }
    
    public void SetStatus(string text) => _statusText.text = text;
}
```

### Использование

```csharp
public class SceneLoader
{
    [Inject] private OverlayManager _overlayManager;
    
    public async UniTask LoadScene(string sceneName)
    {
        var loading = await _overlayManager.Show<LoadingOverlay>();
        loading.SetStatus("Загрузка...");
        
        await SceneManager.LoadSceneAsync(sceneName);
        
        _overlayManager.Hide<LoadingOverlay>(unload: false);
    }
}
```

---

## SidePanelManager — Выдвижные панели

Панели, выезжающие с краёв экрана. Могут иметь вложенные панели.

### Создание панели

```csharp
using LiteUI.SidePanel.Attribute;
using LiteUI.Binding.Attributes;
using UnityEngine;

[UIController("Panels/InventoryPanel")]
[UISidePanel(Direction.LEFT, showOffset: 20f)]
public class InventoryPanel : MonoBehaviour
{
    [UIComponentBinding("GoldText")]
    private TMPro.TMP_Text _goldText;
    
    [UICreated]
    private void Init(int gold)
    {
        _goldText.text = gold.ToString();
    }
    
    [UIOnClick("CloseButton")]
    private void OnClose()
    {
        // Панель скроется автоматически через SidePanelManager
    }
}
```

### Использование

```csharp
public class HUDController
{
    [Inject] private SidePanelManager _sidePanelManager;
    
    public async void OpenInventory()
    {
        var panel = await _sidePanelManager.ShowPanel<InventoryPanel>(playerGold);
    }
    
    public async void CloseInventory()
    {
        await _sidePanelManager.HidePanel(Direction.LEFT);
    }
}
```

### Direction

```csharp
public enum Direction { UP, DOWN, LEFT, RIGHT }
```

---

## NotificationService — Тост-уведомления

Очередь уведомлений с приоритетами и тегами.

### Использование

```csharp
public class RewardService
{
    [Inject] private NotificationQueueService _notificationQueue;
    
    public void ShowReward(RewardData reward)
    {
        var model = new NotificationModel("reward_" + reward.Id, new[] { "reward" });
        model.SetData(reward);
        
        _notificationQueue.AddLast(model);  // В конец очереди
        // _notificationQueue.AddFirst(model);  // В начало (приоритет)
    }
    
    public void CancelRewardNotifications()
    {
        _notificationQueue.RemoveByTag("reward");
    }
}
```

---

## Screen — Переключение режимов

Стейт-машина для переключения между режимами экрана.

### Создание экрана

```csharp
public enum LobbyView { Main, Shop, Inventory, Settings }

public class LobbyScreen : Screen<LobbyView>
{
    [SerializeField] private MainPanel _mainPanel;
    [SerializeField] private ShopPanel _shopPanel;
    
    private void Start()
    {
        RegisterPanel(LobbyView.Main, _mainPanel);
        RegisterPanel(LobbyView.Shop, _shopPanel);
        SwitchMode(LobbyView.Main).Forget();
    }
    
    public void GoToShop() => SwitchMode(LobbyView.Shop).Forget();
}
```

### Панель с IActivatable

```csharp
using LiteUI.World;
using LiteUI.Tweening;

public class ShopPanel : MonoBehaviour, IActivatable
{
    private UITweenPlayer _tweenPlayer;
    
    void Awake() => _tweenPlayer = new UITweenPlayer(gameObject);
    
    public async UniTask Activate()
    {
        gameObject.SetActive(true);
        await _tweenPlayer.Play(UITweenPresets.PopIn());
    }
    
    public async UniTask Deactivate()
    {
        await _tweenPlayer.Play(UITweenPresets.PopOut());
        gameObject.SetActive(false);
    }
}
```

---

## Binding System — Автобиндинг UI

Автоматическая привязка полей и методов к UI элементам по имени.

### Атрибуты

| Атрибут | Назначение |
|---------|------------|
| `[UIController("AddressablePath")]` | Путь к Addressable префабу |
| `[UIDialog]` | Маркер диалога |
| `[UIComponentBinding("ChildName")]` | Привязка компонента |
| `[UIObjectBinding("ChildName")]` | Привязка GameObject |
| `[UIOnClick("ButtonName")]` | Обработчик клика |
| `[UIOnValueChanged("SliderName")]` | Изменение значения |
| `[UICreated]` | Вызывается после биндинга |

### Пример

```csharp
[UIController("UI/PlayerHUD")]
public class PlayerHUD : MonoBehaviour
{
    [UIComponentBinding("HealthBar")]
    private Slider _healthBar;
    
    [UIObjectBinding("Avatar")]
    private GameObject _avatar;
    
    [UIOnClick("AttackButton")]
    private void OnAttack() => Debug.Log("Attack!");
    
    [UIOnValueChanged("VolumeSlider")]
    private void OnVolumeChanged(float value) => AudioListener.volume = value;
    
    [UICreated]
    private void Init(PlayerData data) => _healthBar.value = data.Health;
}
```

---

## UITweenPlayer — Анимации

Универсальная система анимаций на DOTween.

### Способ 1: Конфиг в редакторе

1. **Create → LiteUI → Tween Config**
2. Настрой анимации в инспекторе
3. Используй в коде:

```csharp
[SerializeField] private UITweenConfig _showConfig;

private UITweenPlayer _tweenPlayer;

void Start() => _tweenPlayer = new UITweenPlayer(gameObject);

public async UniTask Show() => await _tweenPlayer.Play(_showConfig);
```

### Способ 2: Готовые пресеты

```csharp
var player = new UITweenPlayer(gameObject);

// Fade
await player.Play(UITweenPresets.FadeIn());
await player.Play(UITweenPresets.FadeOut());

// Scale
await player.Play(UITweenPresets.ScaleIn());
await player.Play(UITweenPresets.ScaleOut());
await player.Play(UITweenPresets.ScaleBounce());

// Slide
await player.Play(UITweenPresets.SlideInLeft());
await player.Play(UITweenPresets.SlideInRight());
await player.Play(UITweenPresets.SlideInTop());
await player.Play(UITweenPresets.SlideInBottom());

// Combined
await player.Play(UITweenPresets.PopIn());   // Fade + Scale
await player.Play(UITweenPresets.PopOut());

// Effects
await player.Play(UITweenPresets.Shake());
await player.Play(UITweenPresets.Pulse());
```

### Способ 3: UITweenAnimator компонент

Добавь `UITweenAnimator` на любой UI объект:

```csharp
// В коде
var animator = GetComponent<UITweenAnimator>();
await animator.Show();
await animator.Hide();
await animator.PlayClick();
await animator.PlayCustom("MyAnimation");
```

### Типы анимаций

| Тип | Что анимирует |
|-----|---------------|
| `Fade` | CanvasGroup.alpha |
| `Scale` | transform.localScale |
| `ScaleUniform` | Равномерный scale |
| `Move` | RectTransform.anchoredPosition |
| `Rotate` | transform.localEulerAngles |
| `Size` | RectTransform.sizeDelta |
| `Color` | Graphic.color |
| `PunchScale` | Пружинящий scale |
| `PunchPosition` | Пружинящая позиция |
| `ShakePosition` | Тряска позиции |
| `ShakeRotation` | Тряска поворота |

---

## Namespace

```
LiteUI.Dialog.*        — Диалоги
LiteUI.Popup.*         — Попапы  
LiteUI.Overlay.*       — Оверлеи
LiteUI.SidePanel.*     — Боковые панели
LiteUI.Notification.*  — Уведомления
LiteUI.UI.*            — Screen, UIService
LiteUI.Binding.*       — Биндинг
LiteUI.Tweening.*      — Анимации
LiteUI.Common.*        — Утилиты
```
