using System;
using JetBrains.Annotations;

namespace LiteUI.Binding.Attributes.Method
{
    [PublicAPI, MeansImplicitUse(ImplicitUseKindFlags.Access), AttributeUsage(AttributeTargets.Method)]
    public class UIOnSwipeAttribute : Attribute
    {
        public string? Name { get; }

        public UIOnSwipeAttribute(string? name = null)
        {
            Name = name;
        }
    }
}
