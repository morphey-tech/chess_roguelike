namespace Project.Gameplay.Gameplay.Interaction
{
    /// <summary>
    /// Resolves click actions into game intents.
    /// Contains all game rules for determining what action a click should perform.
    /// </summary>
    public interface IClickIntentResolver
    {
        /// <summary>
        /// Resolves the intent from a click given the current interaction context.
        /// </summary>
        /// <param name="context">The context containing grid, selection state, and clicked position.</param>
        /// <returns>The resolved click intent.</returns>
        ClickIntent Resolve(InteractionContext context);
    }
}
