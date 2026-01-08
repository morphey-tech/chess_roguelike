using System;
using JetBrains.Annotations;

namespace LiteUI.Binding.Attributes.Method
{
    [PublicAPI, MeansImplicitUse(ImplicitUseKindFlags.Access), AttributeUsage(AttributeTargets.Method)]
    public class UIOnBeginDragAttribute : Attribute
    {
        public string Name { get; }

        public UIOnBeginDragAttribute(string name)
        {
            Name = name;
        }
    }
}
