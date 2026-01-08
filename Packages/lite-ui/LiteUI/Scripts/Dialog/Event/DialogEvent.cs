using System;

namespace LiteUI.Dialog.Event
{
    public class DialogEvent
    {
        public const string OPENED = "sceneBeforeLoadEvent";
        public const string CLOSED = "sceneLoadedEvent";
        
        public Type? ControllerType { get; }

        public DialogEvent(Type? controllerType)
        {
            ControllerType = controllerType;
        }
    }
}
