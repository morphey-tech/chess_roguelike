using System;
using JetBrains.Annotations;

namespace LiteUI.Binding.Attributes.Method
{
    [PublicAPI, MeansImplicitUse(ImplicitUseKindFlags.Access), AttributeUsage(AttributeTargets.Method)]
    public class UIOnTabAttribute : Attribute
    {
        public string Name { get; }

        public UIOnTabAttribute(string name)
        {
            Name = name;
        }
    }
}
