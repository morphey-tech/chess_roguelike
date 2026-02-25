namespace Project.Gameplay.Gameplay.Run
{
    /// <summary>
    /// Хранит ссылку на текущий Run.
    /// </summary>
    public class RunHolder
    {
        public Run? Current { get; private set; }

        public void Set(Run run)
        {
            Current = run;
        }

        public void Clear()
        {
            Current = null;
        }
    }
}
