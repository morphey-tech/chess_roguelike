using System;
using JetBrains.Annotations;

namespace LiteUI.Binding.Attributes.Method
{
    [PublicAPI, MeansImplicitUse(ImplicitUseKindFlags.Access), AttributeUsage(AttributeTargets.Method)]
    public class UIOnInputChangeAttribute : Attribute
    {
        public string Name { get; }

        public UIOnInputChangeAttribute(string name)
        {
            Name = name;
        }
    }
}
