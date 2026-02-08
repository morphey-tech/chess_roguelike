namespace Project.Gameplay.Gameplay.Turn
{
    /// <summary>
    /// Holds the current ActionContext for visual hit callbacks.
    /// </summary>
    public sealed class ActionContextAccessor
    {
        public ActionContext Current { get; private set; }

        public void Set(ActionContext context)
        {
            Current = context;
        }

        public void Clear()
        {
            Current = null;
        }
    }
}
