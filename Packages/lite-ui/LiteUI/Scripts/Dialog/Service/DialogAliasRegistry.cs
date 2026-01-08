using System;
using System.Collections.Generic;
using LiteUI.Common.Attributes;
using LiteUI.Common.Extensions;
using LiteUI.Dialog.Attributes;

namespace LiteUI.Dialog.Service
{
    [Injectable]
    public class DialogAliasRegistry
    {
        private readonly Dictionary<string, Type> _handlers = new();

        public void Register(Type type, DialogAliasAttribute attribute)
        {
            if (string.IsNullOrEmpty(attribute.Alias)) {
                return;
            }
            _handlers[attribute.Alias] = type;
        }

        public Type? GetDialogByAlias(string alias)
        {
            return _handlers.GetOrDefault(alias);
        }
    }
}
