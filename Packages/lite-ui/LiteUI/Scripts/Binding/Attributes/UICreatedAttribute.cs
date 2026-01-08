using System;
using JetBrains.Annotations;

namespace LiteUI.Binding.Attributes
{
    [PublicAPI, MeansImplicitUse(ImplicitUseKindFlags.Assign), AttributeUsage(AttributeTargets.Method)]
    public class UICreatedAttribute : Attribute
    {
    }
}
