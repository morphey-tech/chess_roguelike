using System;
using JetBrains.Annotations;

namespace LiteUI.Binding.Attributes.Method
{
    [PublicAPI, MeansImplicitUse(ImplicitUseKindFlags.Access), AttributeUsage(AttributeTargets.Method)]
    public class UIOnDropdownAttribute : Attribute
    {
        public string? Name { get; }

        public UIOnDropdownAttribute(string? name = null)
        {
            Name = name;
        }
    }
}
