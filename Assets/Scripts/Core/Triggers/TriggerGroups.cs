namespace Project.Core.Core.Triggers
{
    /// <summary>
    /// Constants for common group combinations.
    /// </summary>
    public static class TriggerGroups
    {
        /// <summary>
        /// Damage modification pipeline in order.
        /// </summary>
        public static readonly TriggerGroup[] DamageModificationPipeline =
        {
            TriggerGroup.Additive,
            TriggerGroup.Multiplicative,
            TriggerGroup.Reduction,
            TriggerGroup.Final
        };

        /// <summary>
        /// Generic pipeline in order.
        /// </summary>
        public static readonly TriggerGroup[] GenericPipeline =
        {
            TriggerGroup.First,
            TriggerGroup.Early,
            TriggerGroup.Normal,
            TriggerGroup.Late,
            TriggerGroup.Last
        };
    }
}