using System;
using JetBrains.Annotations;

namespace LiteUI.Binding.Attributes.Method
{
    [PublicAPI, MeansImplicitUse(ImplicitUseKindFlags.Access), AttributeUsage(AttributeTargets.Method)]
    public class UIOnDragAttribute : Attribute
    {
        public string Name { get; }

        public UIOnDragAttribute(string name)
        {
            Name = name;
        }
    }
}
