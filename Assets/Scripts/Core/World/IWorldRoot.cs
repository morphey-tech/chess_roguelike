using UnityEngine;

namespace Project.Core.Core.World
{
    /// <summary>
    /// Предоставляет корневые Transform'ы для спавна объектов.
    /// Реализация находится в Unity слое.
    /// </summary>
    public interface IWorldRoot
    {
        Transform BoardRoot { get; }
        Transform FigureRoot { get; }
        Transform PrepareRoot { get; }
        Transform EffectsRoot { get; }
    }
}
