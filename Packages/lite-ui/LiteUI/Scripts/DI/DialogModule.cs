using LiteUI.Dialog.Service;
using LiteUI.Dialog.Event;
using MessagePipe;
using VContainer;

namespace LiteUI.DI
{
    public class DialogModule
    {
        public static DialogModule Create()
        {
            return new DialogModule();
        }
        
        public void Register(IContainerBuilder builder, MessagePipeOptions options)
        {
            builder.Register<DialogManager>(Lifetime.Singleton);
            builder.Register<IDialogLoader, SimpleDialogLoader>(Lifetime.Singleton);
            RegisterPipes(builder, options);
        }
        
        private static void RegisterPipes(IContainerBuilder builder, MessagePipeOptions options)
        {
            builder.RegisterMessageBroker<string, DialogMessage>(options);
        }
    }
}
