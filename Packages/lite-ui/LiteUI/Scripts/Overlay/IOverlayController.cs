namespace LiteUI.Overlay
{
    public interface IOverlayController
    {
        void Show();
        void Hide();
        bool IsHiding { get; }
    }
}
