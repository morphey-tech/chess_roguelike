using System;
using JetBrains.Annotations;

namespace LiteUI.Binding.Attributes.Method
{
    [PublicAPI, MeansImplicitUse(ImplicitUseKindFlags.Access), AttributeUsage(AttributeTargets.Method)]
    public class UIOnLongClickAttribute : Attribute
    {
        public string? Name { get; }

        public UIOnLongClickAttribute(string? name = null)
        {
            Name = name;
        }
    }
}
