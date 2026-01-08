using System;
using JetBrains.Annotations;

namespace LiteUI.Binding.Attributes.Method
{
    [PublicAPI, MeansImplicitUse(ImplicitUseKindFlags.Access), AttributeUsage(AttributeTargets.Method)]
    public class UIOnClickAttribute : Attribute
    {
        public string? Name { get; }
        public bool UseButtonState { get; }

        public UIOnClickAttribute(string? name = null, bool useButtonState = true)
        {
            Name = name;
            UseButtonState = useButtonState;
        }
    }
}
