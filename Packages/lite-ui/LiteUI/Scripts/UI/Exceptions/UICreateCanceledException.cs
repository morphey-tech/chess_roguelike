using System;

namespace LiteUI.UI.Exceptions
{
    public class UICreateCanceledException : Exception
    {
        public UICreateCanceledException(string message) : base(message)
        {
        }
        
        public UICreateCanceledException(string message, Exception e) : base(message, e)
        {
        }
    }
}
