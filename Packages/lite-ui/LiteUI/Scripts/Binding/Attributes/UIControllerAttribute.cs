using System;
using JetBrains.Annotations;

namespace LiteUI.Binding.Attributes
{
    [PublicAPI, MeansImplicitUse(ImplicitUseKindFlags.Assign), AttributeUsage(AttributeTargets.Class)]
    public class UIControllerAttribute : Attribute
    {
        public string Id { get; }

        public UIControllerAttribute(string id)
        {
            Id = id;
        }
    }
}
