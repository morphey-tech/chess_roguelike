namespace Project.Gameplay.Gameplay.Player
{
    public struct EyesStateChangedMessage
    {
        public bool IsClosed { get; private set; }
        
        public EyesStateChangedMessage(bool isClosed)
        {
            IsClosed = isClosed;
        }
    }
}