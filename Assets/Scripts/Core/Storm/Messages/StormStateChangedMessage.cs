using Project.Core.Core.Storm.Core;

namespace Project.Core.Core.Storm.Messages
{
    public readonly struct StormStateChangedMessage
    {
        public readonly StormState NewState;

        public StormStateChangedMessage(StormState newState)
        {
            NewState = newState;
        }
    }
}