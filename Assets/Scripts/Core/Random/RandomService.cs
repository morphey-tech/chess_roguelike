using System;
using VContainer;

namespace Project.Core.Core.Random
{
    /// <summary>
    /// Default implementation of IRandomService using System.Random.
    /// Supports reseeding for deterministic runs.
    /// </summary>
    public sealed class RandomService : IRandomService
    {
        private System.Random _random;

        [Inject]
        private RandomService()
        {
            _random = new System.Random((int)DateTime.Now.Ticks);
        }

        /// <summary>
        /// Initialize with a specific seed for deterministic behavior.
        /// </summary>
        public void SetSeed(int seed)
        {
            _random = new System.Random(seed);
        }

        /// <summary>
        /// Returns a random float between 0.0 (inclusive) and 1.0 (inclusive).
        /// </summary>
        float IRandomService.Value => (float)_random.NextDouble();

        /// <summary>
        /// Returns a random integer between min (inclusive) and max (inclusive).
        /// </summary>
        int IRandomService.Range(int min, int max)
        {
            return _random.Next(min, max + 1);
        }

        /// <summary>
        /// Returns a random float between min (inclusive) and max (inclusive).
        /// </summary>
        float IRandomService.Range(float min, float max)
        {
            return (float)(_random.NextDouble() * (max - min) + min);
        }

        /// <summary>
        /// Returns true with the specified probability (0.0 to 1.0).
        /// </summary>
        bool IRandomService.Chance(float probability)
        {
            return ((IRandomService)this).Value < probability;
        }
    }
}
