using System;
using JetBrains.Annotations;

namespace LiteUI.Binding.Attributes
{
    [PublicAPI, MeansImplicitUse(ImplicitUseKindFlags.Assign), AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class UIObjectBindingAttribute : Attribute
    {
        public string Name { get; }

        public UIObjectBindingAttribute(string name)
        {
            Name = name;
        }
    }
}
