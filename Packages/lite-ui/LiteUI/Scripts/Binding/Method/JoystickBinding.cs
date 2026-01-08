using System;
using System.Reflection;
using LiteUI.Element.Widgets;
using UnityEngine;

namespace LiteUI.Binding.Method
{
    public class JoystickBinding : MethodBinding
    {
        public JoystickBinding(string? name, MethodInfo methodInfo) : base(name, methodInfo)
        {
        }

        public override void Bind(MonoBehaviour controller, GameObject prefab)
        {
            GameObject? go = FindBindingTarget(prefab);
            if (go == null) {
                throw new NullReferenceException($"target not found={Name} in prefab={prefab.name}");
            }

            IJoystick component = go.GetComponent<IJoystick>();
            if (component == null) {
                throw new ArgumentException("joystick not found");
            }
            component.OnJoystick += d => MethodInfo.Invoke(controller, new object[] { d });
        }
    }
}
