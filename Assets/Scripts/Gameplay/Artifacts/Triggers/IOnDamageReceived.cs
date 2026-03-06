namespace Project.Gameplay.Gameplay.Artifacts.Triggers
{
    /// <summary>
    /// Called when receiving damage.
    /// </summary>
    public interface IOnDamageReceived
    {
        void OnDamageReceived(DamageContext context);
    }
}