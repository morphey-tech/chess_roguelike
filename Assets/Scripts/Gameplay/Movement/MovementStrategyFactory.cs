using System.Collections.Generic;
using System.Linq;
using Project.Gameplay.Gameplay.Movement.Strategies;

namespace Project.Gameplay.Gameplay.Movement
{
    /// <summary>
    /// Factory for movement strategies. Maps movement ID to strategy.
    /// </summary>
    public sealed class MovementStrategyFactory
    {
        private readonly Dictionary<string, IMovementStrategy> _strategies;
        private readonly IMovementStrategy _fallback;

        private MovementStrategyFactory()
        {
            IMovementStrategy[] all =
            {
                new PawnMovement(),
                new KnightMovement(),
                new RookMovement(),
                new BishopMovement(),
                new QueenMovement()
            };

            _strategies = all.ToDictionary(s => s.Id);
            _fallback = new PawnMovement();
        }

        public IMovementStrategy Get(string movementId)
        {
            return string.IsNullOrEmpty(movementId) 
                ? _fallback : _strategies.GetValueOrDefault(movementId, _fallback);
        }
    }
}
