using System;

namespace Project.Core.Core.Filters.Messages
{
    public struct AppFilterMessage
    {
        public const string STARTED = "appfilterStarted";
        public const string COMPLETED = "appFilterCompleted";

        public Type FilterType { get; }
        
        public AppFilterMessage(Type filterType)
        {
            FilterType = filterType;
        }
    
    }
}