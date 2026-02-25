namespace Project.Unity.UI.Components
{
    public interface IAnchorToTargetTicker
    {
        void Register(AnchorToTarget obj);
        void Unregister(AnchorToTarget obj);
    }
}