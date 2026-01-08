using System;

namespace LiteUI.Dialog.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DialogAliasAttribute : Attribute
    {
        public string? Alias { get; } 

        public DialogAliasAttribute(string alias)
        {
            Alias = alias;
        }
    }
}
