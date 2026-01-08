using System;
using JetBrains.Annotations;

namespace LiteUI.Binding.Attributes.Method
{
    [PublicAPI, MeansImplicitUse(ImplicitUseKindFlags.Access), AttributeUsage(AttributeTargets.Method)]
    public class UIOnDropAttribute : Attribute
    {
        public string Name { get; }

        public UIOnDropAttribute(string name)
        {
            Name = name;
        }
    }
}
