using System;
using MessagePipe;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Input.Messages;
using Project.Gameplay.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using VContainer;

namespace Project.Gameplay.Gameplay.UI
{
    /// <summary>
    /// Компонент обработки наведения на характеристику для показа тултипа.
    /// </summary>
    public class StatTooltipHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private TextMeshProUGUI _targetText;
        [SerializeField] private string _statKey = string.Empty;
        [SerializeField] private Vector2 _tooltipOffset = new Vector2(0, 30);

        private IPublisher<string, TooltipMessage> _tooltipPublisher = null!;
        private ILogger<StatTooltipHandler> _logger = null!;

        private FigureInfoWindow.FigureInfoModel? _currentModel;

        [Inject]
        private void Construct(
            IPublisher<string, TooltipMessage> tooltipPublisher,
            ILogService logService)
        {
            _tooltipPublisher = tooltipPublisher;
            _logger = logService.CreateLogger<StatTooltipHandler>();
        }

        public void Setup(FigureInfoWindow.FigureInfoModel model)
        {
            _currentModel = model;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_currentModel == null || string.IsNullOrEmpty(_statKey))
            {
                return;
            }

            string tooltip = GetStatTooltip(_statKey, _currentModel);

            if (string.IsNullOrEmpty(tooltip))
            {
                return;
            }

            Vector2 staticPosition = _targetText != null
                ? _targetText.rectTransform.TransformPoint(Vector3.zero)
                : ((RectTransform)transform).TransformPoint(Vector3.zero);

            staticPosition.x += _tooltipOffset.x;
            staticPosition.y += _tooltipOffset.y;

            _tooltipPublisher.Publish(TooltipMessage.SHOW,
                TooltipMessage.Show(tooltip, staticPosition, useStaticPosition: true));
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _tooltipPublisher.Publish(TooltipMessage.HIDE, TooltipMessage.Hide());
        }

        private static string GetStatTooltip(string statKey, FigureInfoWindow.FigureInfoModel model)
        {
            FigureStats stats = model.Figure.Stats;

            string statName;
            string statDescription;
            string statValue;

            switch (statKey.ToLower())
            {
                case "attack":
                    statName = "Атака";
                    statDescription = "Базовый урон фигуры.\nМодифицируется пассивками и эффектами.";
                    statValue = $"{stats.Attack.Value} (базовый: {stats.Attack.BaseValue})";
                    break;

                case "defence":
                    statName = "Защита";
                    statDescription = "Снижает получаемый урон.\nКаждое очко защиты уменьшает урон на 1.";
                    statValue = $"{stats.Defence.Value} (базовый: {stats.Defence.BaseValue})";
                    break;

                case "evasion":
                    statName = "Уклонение";
                    statDescription = "Шанс избежать атаки.\nПроверяется при каждой атаке врага.";
                    statValue = $"{stats.Evasion.Value} (базовый: {stats.Evasion.BaseValue})";
                    break;

                case "hp":
                    statName = "Здоровье";
                    statDescription = "Количество жизней фигуры.\nПри достижении 0 фигура погибает.";
                    statValue = $"{stats.CurrentHp.Value}/{stats.MaxHp}";
                    break;

                case "attack_range":
                    statName = "Дальность атаки";
                    statDescription = "Максимальное расстояние атаки.\nИзмеряется в клетках (Chebyshev distance).";
                    statValue = $"{stats.AttackRange}";
                    break;

                default:
                    return string.Empty;
            }

            return $"<size=25><color=#FFFFFF>{statName}</color></size>\n" +
                   $"<size=20><color=#D4B9B9>{statValue}</color></size>\n" +
                   $"<size=18><color=#B0B0B0>{statDescription}</color></size>";
        }
    }
}
