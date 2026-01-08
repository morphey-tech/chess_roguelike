using System;
using JetBrains.Annotations;

namespace LiteUI.Binding.Attributes
{
    [PublicAPI, MeansImplicitUse(ImplicitUseKindFlags.Assign), AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class BindedAttribute : Attribute
    {
        public bool IncludeChildren { get; }

        public BindedAttribute(bool includeChildren = false)
        {
            IncludeChildren = includeChildren;
        }
    }
}
