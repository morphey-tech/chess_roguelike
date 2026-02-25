namespace Project.Core.Core.Random
{
    /// <summary>
    /// Service for generating random numbers.
    /// Abstraction over UnityEngine.Random to avoid Unity dependency in domain layer.
    /// </summary>
    public interface IRandomService
    {
        /// <summary>
        /// Returns a random float between 0.0 (inclusive) and 1.0 (inclusive).
        /// </summary>
        float Value { get; }

        /// <summary>
        /// Returns a random integer between min (inclusive) and max (inclusive).
        /// </summary>
        int Range(int min, int max);

        /// <summary>
        /// Returns a random float between min (inclusive) and max (inclusive).
        /// </summary>
        float Range(float min, float max);

        /// <summary>
        /// Returns true with the specified probability (0.0 to 1.0).
        /// </summary>
        bool Chance(float probability);
    }
}
