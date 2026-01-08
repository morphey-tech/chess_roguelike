using System;
using JetBrains.Annotations;

namespace LiteUI.Binding.Attributes
{
    [PublicAPI, MeansImplicitUse(ImplicitUseKindFlags.Assign), AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class UIComponentBindingAttribute : Attribute
    {
        public string? Name { get; }
        public bool Bind { get; }

        public UIComponentBindingAttribute(string? name, bool bind)
        {
            Name = name;
            Bind = bind;
        }

        public UIComponentBindingAttribute(string name) : this(name, false)
        {
            Name = name;
        }

        public UIComponentBindingAttribute() : this(null, false)
        {
        }
    }
}
