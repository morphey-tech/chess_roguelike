using System;

namespace LiteUI.Dialog.Event
{
    public class DialogMessage
    {
        public const string OPENED = "sceneBeforeLoadEvent";
        public const string CLOSED = "sceneLoadedEvent";
        
        public Type? ControllerType { get; }

        public DialogMessage(Type? controllerType)
        {
            ControllerType = controllerType;
        }
    }
}
