using System;
using JetBrains.Annotations;

namespace LiteUI.Binding.Attributes.Method
{
    [PublicAPI, MeansImplicitUse(ImplicitUseKindFlags.Access), AttributeUsage(AttributeTargets.Method)]
    public class UIOnToggleAttribute : Attribute
    {
        public string Name { get; }

        public UIOnToggleAttribute(string name)
        {
            Name = name;
        }
    }
}
