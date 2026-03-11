using LiteUI.Popup.Manager;
using VContainer;

namespace LiteUI.DI
{
    public class PopupModule
    {
        public static PopupModule Create()
        {
            return new PopupModule();
        }
        
        public void Register(IContainerBuilder builder)
        {
            builder.Register<PopupManager>(Lifetime.Singleton);
            builder.Register<IPopupLoader, SimplePopupLoader>(Lifetime.Singleton);
        }
    }
}
