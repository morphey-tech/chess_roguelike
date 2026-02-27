using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Project.Gameplay.Gameplay.UI
{
    /// <summary>
    /// Сервис для управления tooltip.
    /// </summary>
    public interface ITooltipService
    {
        UniTaskVoid ShowTooltipAsync(string content, Vector2 position);
        void HideTooltip();
    }
}
