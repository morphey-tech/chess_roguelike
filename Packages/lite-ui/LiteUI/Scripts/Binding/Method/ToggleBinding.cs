using System;
using System.Reflection;
using LiteUI.Element.Buttons;
using UnityEngine;
using UnityEngine.UI;

namespace LiteUI.Binding.Method
{
    public class ToggleBinding : MethodBinding
    {
        public ToggleBinding(string name, MethodInfo methodInfo) : base(name, methodInfo)
        {
        }

        public override void Bind(MonoBehaviour controller, GameObject prefab)
        {
            GameObject? go = FindBindingTarget(prefab);
            if (go == null) {
                throw new NullReferenceException($"target not found={Name} in prefab={prefab.name}");
            }

            if (go.GetComponent<AnimatedToggle>() != null) {
                AnimatedToggle toggle = go.GetComponent<AnimatedToggle>();
                toggle.onValueChanged.AddListener(delegate {
                    if (toggle.IgnoreClicks) {
                        return;
                    }
                    object[] invokeParams = { toggle.isOn };
                    MethodInfo.Invoke(controller, invokeParams);
                });
                return;
            }
            if (go.GetComponent<Toggle>() != null) {
                Toggle toggle = go.GetComponent<Toggle>();
                toggle.onValueChanged.AddListener(delegate {
                    object[] invokeParams = { toggle.isOn };
                    MethodInfo.Invoke(controller, invokeParams);
                });
                return;
            }
            
            throw new ArgumentException($"toggle not found={Name} prefab={prefab.name}");
        }
    }
}
