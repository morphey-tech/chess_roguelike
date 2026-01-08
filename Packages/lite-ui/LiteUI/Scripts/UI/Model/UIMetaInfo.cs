using System;
using System.Collections.Generic;
using System.Reflection;
using LiteUI.Binding.Field;
using LiteUI.Binding.Method;

namespace LiteUI.UI.Model
{
    public class UIMetaInfo
    {
        public Type Type { get; }
        public string Id { get; }
        public MethodInfo? InitMethod { get; set; }

        public List<ObjectBinding> ObjectBindings { get; } = new();
        public List<ComponentBinding> ComponentBindings { get; } = new();
        public List<MethodBinding> MethodBindings { get; } = new();

        public UIMetaInfo(Type type, string id)
        {
            Type = type;
            Id = id;
        }
    }
}
