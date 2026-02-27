using Project.Core.Window;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Gameplay.UI
{
    /// <summary>
    /// Окно tooltip для отображения всплывающих подсказок.
    /// </summary>
    public class TooltipWindow : ParameterlessWindow
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI _contentText;
        [SerializeField] private RectTransform _rootRect;  // Root контейнер

        [Header("Settings")]
        [SerializeField] private float _paddingX = 10f;
        [SerializeField] private float _paddingY = 8f;
        [SerializeField] private float _maxWidth = 300f;
        [SerializeField] private float _minWidth = 150f;  // Минимальная ширина tooltip
        [SerializeField] private float _cursorOffset = 10f;  // Отступ от курсора

        private RectTransform _rectTransform;

        protected override void OnInit()
        {
            base.OnInit();
            _rectTransform = GetComponent<RectTransform>();
            
            // Добавляем VerticalLayoutGroup на Root если нет
            if (_rootRect != null && _rootRect.GetComponent<VerticalLayoutGroup>() == null)
            {
                var layout = _rootRect.gameObject.AddComponent<VerticalLayoutGroup>();
                layout.padding = new RectOffset((int)_paddingX, (int)_paddingX, (int)_paddingY, (int)_paddingY);
                layout.spacing = 0;
                layout.childAlignment = TextAnchor.UpperLeft;
                layout.childForceExpandWidth = false;
                layout.childForceExpandHeight = false;
                layout.childControlWidth = false;
                layout.childControlHeight = false;
            }
        }

        /// <summary>
        /// Показать tooltip с текстом.
        /// </summary>
        public void Show(string content, Vector2 position)
        {
            if (_contentText == null || _rootRect == null)
                return;

            _contentText.text = content;
            
            // Включаем текст и ждем обновления размера
            _contentText.gameObject.SetActive(true);
            LayoutRebuilder.ForceRebuildLayoutImmediate(_rootRect);
            
            // Получаем предпочтительный размер текста
            Vector2 preferredSize = _contentText.GetPreferredValues();
            
            // Рассчитываем размер с padding - динамическая ширина
            float targetWidth = Mathf.Max(preferredSize.x + (_paddingX * 2), _minWidth);
            targetWidth = Mathf.Min(targetWidth, _maxWidth);
            float targetHeight = preferredSize.y + (_paddingY * 2);
            
            // Если текст шире максимума, пересчитываем высоту с учетом word wrap
            if (preferredSize.x + (_paddingX * 2) > _maxWidth)
            {
                // Устанавливаем максимальную ширину для текста
                _contentText.rectTransform.sizeDelta = new Vector2(_maxWidth - (_paddingX * 2), 0);
                LayoutRebuilder.ForceRebuildLayoutImmediate(_rootRect);
                preferredSize = _contentText.GetPreferredValues();
                targetWidth = _maxWidth;
                targetHeight = preferredSize.y + (_paddingY * 2);
            }

            // Минимальный размер
            targetWidth = Mathf.Max(targetWidth, _minWidth);
            targetHeight = Mathf.Max(targetHeight, 30f);

            _rootRect.sizeDelta = new Vector2(targetWidth, targetHeight);
            
            // Позиционируем Root у курсора - конвертируем из screen в canvas space
            Vector2 localPosition;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _rectTransform, 
                position, 
                null, 
                out localPosition))
            {
                // Позиционируем tooltip сбоку от курсора
                // localPosition - это позиция курсора в canvas space (относительно центра Canvas)
                // Нам нужно чтобы БЛИЖНИЙ КРАЙ tooltip был у курсора
                float halfWidth = targetWidth / 2;
                float halfHeight = targetHeight / 2;
                
                // Определяем с какой стороны экрана курсор (используем screen координаты)
                bool isLeftSide = position.x < Screen.width / 2;
                
                // Смещение: центр tooltip = позиция курсора + половина ширины + отступ
                float offsetX = isLeftSide ? _cursorOffset + halfWidth : -(_cursorOffset + halfWidth);
                float offsetY = -halfHeight;  // Центрируем по вертикали
                
                Vector2 targetPosition = localPosition + new Vector2(offsetX, offsetY);
                
                // Проверка границ Canvas - не даем tooltip уйти за край
                // Границы Canvas в local координатах
                float canvasHalfWidth = _rectTransform.rect.width / 2;
                float canvasHalfHeight = _rectTransform.rect.height / 2;
                
                // Левая граница
                if (targetPosition.x - halfWidth < -canvasHalfWidth)
                {
                    targetPosition.x = -canvasHalfWidth + halfWidth + 5;
                }
                
                // Правая граница
                if (targetPosition.x + halfWidth > canvasHalfWidth)
                {
                    targetPosition.x = canvasHalfWidth - halfWidth - 5;
                }
                
                // Нижняя граница
                if (targetPosition.y - halfHeight < -canvasHalfHeight)
                {
                    targetPosition.y = -canvasHalfHeight + halfHeight + 5;
                }
                
                // Верхняя граница
                if (targetPosition.y + halfHeight > canvasHalfHeight)
                {
                    targetPosition.y = canvasHalfHeight - halfHeight - 5;
                }
                
                _rootRect.anchoredPosition = targetPosition;
            }
            
            Show();
        }

        public void Hide()
        {
            base.Hide();
        }
    }
}
