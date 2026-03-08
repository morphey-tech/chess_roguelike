using Project.Core.Core.Combat.Contexts;
using Project.Gameplay.Gameplay.Combat.Contexts;
using Project.Gameplay.Gameplay.Figures;

namespace Project.Gameplay.Gameplay.Combat.Contexts
{
    /// <summary>
    /// Extension methods for converting between Core and Gameplay contexts.
    /// Lives in Gameplay layer to avoid Core→Gameplay dependency.
    /// </summary>
    public static class ContextExtensions
    {
        #region To Gameplay

        /// <summary>
        /// Safely cast IDamageContext to BeforeHitContext.
        /// </summary>
        public static BeforeHitContext? AsBeforeHit(this IDamageContext context)
        {
            return context as BeforeHitContext;
        }

        /// <summary>
        /// Safely cast IMoveContext to MoveContext.
        /// </summary>
        public static MoveContext? AsMove(this IMoveContext context)
        {
            return context as MoveContext;
        }

        /// <summary>
        /// Safely cast ITurnContext to TurnContext.
        /// </summary>
        public static TurnContext? AsTurn(this ITurnContext context)
        {
            return context as TurnContext;
        }

        /// <summary>
        /// Safely cast object Attacker to Figure.
        /// </summary>
        public static Figure? GetAttacker(this IDamageContext context)
        {
            return context.Attacker as Figure;
        }

        /// <summary>
        /// Safely cast object Target to Figure.
        /// </summary>
        public static Figure? GetTarget(this IDamageContext context)
        {
            return context.Target as Figure;
        }

        #endregion

        #region From Gameplay

        /// <summary>
        /// Create IDamageContext from BeforeHitContext.
        /// </summary>
        public static IDamageContext ToDamageContext(this BeforeHitContext context)
        {
            return context; // Already implements IDamageContext
        }

        /// <summary>
        /// Create IMoveContext from MoveContext.
        /// </summary>
        public static IMoveContext ToMoveContext(this MoveContext context)
        {
            return context; // Already implements IMoveContext
        }

        /// <summary>
        /// Create ITurnContext from TurnContext.
        /// </summary>
        public static ITurnContext ToTurnContext(this TurnContext context)
        {
            return context; // Already implements ITurnContext
        }

        #endregion

        #region Create Core Contexts

        /// <summary>
        /// Create a Core DamageContext from components.
        /// </summary>
        public static DamageContext CreateDamageContext(
            object attacker,
            object target,
            float baseDamage,
            float multiplier = 1f,
            float bonus = 0f,
            bool isCritical = false,
            bool isDodged = false,
            bool isCancelled = false)
        {
            return new DamageContext
            {
                Attacker = attacker,
                Target = target,
                BaseDamage = baseDamage,
                DamageMultiplier = multiplier,
                BonusDamage = bonus,
                IsCritical = isCritical,
                IsDodged = isDodged,
                IsCancelled = isCancelled
            };
        }

        #endregion
    }
}
