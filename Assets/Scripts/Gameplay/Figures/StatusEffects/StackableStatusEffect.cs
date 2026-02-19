namespace Project.Gameplay.Gameplay.Figures.StatusEffects
{
    public class StackableStatusEffect : StatusEffectBase
    {
        public int Stacks { get; private set; }
        public int MaxStacks { get; }

        protected StackableStatusEffect(int stacks, int maxStacks, int turns = -1, int uses = -1) 
            : base(turns, uses)
        {
            Stacks = stacks;
            MaxStacks = maxStacks;
        }

        public void AddStack()
        {
            if (Stacks < MaxStacks)
            {
                Stacks++;
            }
        }
    }
}