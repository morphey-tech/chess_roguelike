using UnityEngine;

namespace Project.Core.World
{
    /// <summary>
    /// Предоставляет корневые Transform'ы для спавна объектов.
    /// Реализация находится в Unity слое.
    /// </summary>
    public interface IWorldRoot
    {
        Transform BoardRoot { get; }
        Transform UnitsRoot { get; }
        Transform EffectsRoot { get; }
    }
}
