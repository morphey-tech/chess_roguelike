using System;
using JetBrains.Annotations;

namespace LiteUI.Binding.Attributes.Method
{
    [PublicAPI, MeansImplicitUse(ImplicitUseKindFlags.Access), AttributeUsage(AttributeTargets.Method)]
    public class UIOnDoubleClickAttribute : Attribute
    {
        public string? Name { get; }

        public UIOnDoubleClickAttribute(string? name = null)
        {
            Name = name;
        }
    }
}
