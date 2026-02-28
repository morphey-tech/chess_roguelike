namespace Project.Gameplay.Gameplay.Figures
{
    /// <summary>
    /// Контекст источника модификатора для управления жизненным циклом.
    /// </summary>
    public enum ModifierSourceContext
    {
        /// <summary>
        /// Модификатор для превью-расчёта (временный, очищается после расчёта).
        /// </summary>
        PreviewCalculation,

        /// <summary>
        /// Модификатор от боевого эффекта (например, бафф/дебафф).
        /// </summary>
        CombatEffect,

        /// <summary>
        /// Модификатор от пассивного умения.
        /// </summary>
        Passive,

        /// <summary>
        /// Модификатор от предмета экипировки.
        /// </summary>
        Item,
    }
}
