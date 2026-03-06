namespace Project.Gameplay.Gameplay.Artifacts.Triggers
{
    public readonly struct RewardContext
    {
        public readonly int ChoicesCount;

        public RewardContext(int choicesCount)
        {
            ChoicesCount = choicesCount;
        }
    }
}