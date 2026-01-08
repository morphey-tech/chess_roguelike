using LiteUI.Popup.Manager;
using MessagePipe;
using VContainer;

namespace LiteUI.DI
{
    public class PopupModule
    {
        public static PopupModule Create()
        {
            return new PopupModule();
        }
        
        public void Register(IContainerBuilder builder, MessagePipeOptions options)
        {
            builder.Register<PopupManager>(Lifetime.Scoped).AsSelf().AsImplementedInterfaces();
            builder.Register<IPopupLoader, SimplePopupLoader>(Lifetime.Scoped);

            RegisterPipes(builder, options);
        }
        
        private static void RegisterPipes(IContainerBuilder builder, MessagePipeOptions options)
        {
            // builder.RegisterMessageBroker<string, DialogEvent>(options);
        }
    }
}
