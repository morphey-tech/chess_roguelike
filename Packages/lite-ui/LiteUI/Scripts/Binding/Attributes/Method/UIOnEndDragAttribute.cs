using System;
using JetBrains.Annotations;

namespace LiteUI.Binding.Attributes.Method
{
    [PublicAPI, MeansImplicitUse(ImplicitUseKindFlags.Access), AttributeUsage(AttributeTargets.Method)]
    public class UIOnEndDragAttribute : Attribute
    {
        public string Name { get; }

        public UIOnEndDragAttribute(string name)
        {
            Name = name;
        }
    }
}
