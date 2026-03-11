namespace Project.Gameplay.Gameplay.Figures
{
    public struct FigureAttackMessage
    {
        public readonly int ActorID;
        public readonly int TargetID;

        public FigureAttackMessage(int actorID, int targetID)
        {
            ActorID = actorID;
            TargetID = targetID;
        }
    }
}