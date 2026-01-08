using System;
using JetBrains.Annotations;

namespace LiteUI.Binding.Attributes.Method
{
    [PublicAPI, MeansImplicitUse(ImplicitUseKindFlags.Access), AttributeUsage(AttributeTargets.Method)]
    public class UIOnJoystickAttribute : Attribute
    {
        public string Name { get; }

        public UIOnJoystickAttribute(string name)
        {
            Name = name;
        }
    }
}
