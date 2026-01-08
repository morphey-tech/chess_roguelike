using System;
using JetBrains.Annotations;

namespace LiteUI.Binding.Attributes.Method
{
    [PublicAPI, MeansImplicitUse(ImplicitUseKindFlags.Access), AttributeUsage(AttributeTargets.Method)]
    public class UIOnTouchAttribute : Attribute
    {
        public string? Name { get; }
        public bool UseButtonState { get; }

        public UIOnTouchAttribute(string? name = null, bool useButtonState = true)
        {
            Name = name;
            UseButtonState = useButtonState;
        }
    }
}
