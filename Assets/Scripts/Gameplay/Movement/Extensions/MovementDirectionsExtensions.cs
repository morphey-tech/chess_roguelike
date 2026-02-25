using System;

namespace Project.Gameplay.Gameplay.Movement.Extensions
{
    /// <summary>
    /// Extensions for movement direction calculations.
    /// </summary>
    public static class MovementDirectionsExtensions
    {
        private static readonly (int dr, int dc)[] StraightDirections =
        {
            (-1, 0), (1, 0), (0, -1), (0, 1)
        };

        private static readonly (int dr, int dc)[] DiagonalDirections =
        {
            (-1, -1), (-1, 1), (1, -1), (1, 1)
        };

        private static readonly (int dr, int dc)[] AllDirections =
        {
            (-1, -1), (-1, 0), (-1, 1),
            (0, -1),           (0, 1),
            (1, -1),  (1, 0),  (1, 1)
        };

        /// <summary>
        /// Returns direction vectors for the specified direction type.
        /// </summary>
        public static (int dr, int dc)[] GetDirections(this string directionsType)
        {
            return directionsType switch
            {
                "straight" => StraightDirections,
                "diagonal" => DiagonalDirections,
                "all" => AllDirections,
                "cross" => StraightDirections,
                _ => StraightDirections
            };
        }

        /// <summary>
        /// Checks if the given delta (dr, dc) matches the specified direction type.
        /// </summary>
        public static bool IsDirection(this string directionsType, int dr, int dc)
        {
            var directions = GetDirections(directionsType);
            foreach ((int rdr, int rdc) in directions)
            {
                if (IsSameDirection(dr, dc, rdr, rdc))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Calculates movement distance for the given delta.
        /// For straight moves: max(|dr|, |dc|)
        /// For diagonal moves: |dr| (equals |dc|)
        /// </summary>
        public static int GetMovementDistance(this (int dr, int dc) delta)
        {
            return Math.Max(Math.Abs(delta.dr), Math.Abs(delta.dc));
        }

        /// <summary>
        /// Calculates movement distance for the given delta.
        /// </summary>
        public static int GetMovementDistance(int dr, int dc)
        {
            return GetMovementDistance((dr, dc));
        }

        /// <summary>
        /// Returns all 8 adjacent directions (1 cell in any direction).
        /// </summary>
        public static (int dr, int dc)[] GetAdjacentDirections() => AllDirections;

        /// <summary>
        /// Checks if the move is exactly 1 cell in any direction.
        /// </summary>
        public static bool IsAdjacentMove(int dr, int dc)
        {
            return Math.Max(Math.Abs(dr), Math.Abs(dc)) == 1;
        }

        /// <summary>
        /// Checks if the move is straight (horizontal or vertical).
        /// </summary>
        public static bool IsStraightMove(this (int dr, int dc) delta)
        {
            return delta.dr == 0 || delta.dc == 0;
        }

        /// <summary>
        /// Checks if the move is straight (horizontal or vertical).
        /// </summary>
        public static bool IsStraightMove(int dr, int dc)
        {
            return dr == 0 || dc == 0;
        }

        private static bool IsSameDirection(int dr, int dc, int rdr, int rdc)
        {
            if (rdr == 0 && rdc == 0)
                return false;

            if (rdr == 0 && dr == 0 && Math.Sign(dc) == Math.Sign(rdc))
                return true;

            if (rdc == 0 && dc == 0 && Math.Sign(dr) == Math.Sign(rdr))
                return true;

            if (rdr != 0 && rdc != 0 && Math.Abs(dr) == Math.Abs(dc) &&
                Math.Sign(dr) == Math.Sign(rdr) && Math.Sign(dc) == Math.Sign(rdc))
                return true;

            return false;
        }
    }
}
