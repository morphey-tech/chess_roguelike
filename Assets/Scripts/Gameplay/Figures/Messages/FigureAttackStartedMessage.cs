namespace Project.Gameplay.Gameplay.Figures
{
    /// <summary>
    /// Published when a figure starts attacking. Used to clear damage preview.
    /// </summary>
    public readonly struct FigureAttackStartedMessage
    {
        public int AttackerId { get; }
        public int TargetId { get; }

        public FigureAttackStartedMessage(int attackerId, int targetId)
        {
            AttackerId = attackerId;
            TargetId = targetId;
        }
    }
}
