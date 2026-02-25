namespace Project.Gameplay.Gameplay.Prepare
{
    public readonly struct PreparePlacementResult
    {
        public static PreparePlacementResult Ignored { get; } = new(false, false, false);

        public bool Processed { get; }
        public bool Success { get; }
        public bool Completed { get; }

        public PreparePlacementResult(bool processed, bool success, bool completed)
        {
            Processed = processed;
            Success = success;
            Completed = completed;
        }
    }
}