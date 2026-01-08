using System;
using System.Reflection;
using LiteUI.Binding.Components;
using UnityEngine;

namespace LiteUI.Binding.Method
{
    public class DoubleClickBinding : MethodBinding
    {
        public DoubleClickBinding(string? name, MethodInfo methodInfo) : base(name, methodInfo)
        {
        }

        public override void Bind(MonoBehaviour controller, GameObject prefab)
        {
            GameObject? go = FindBindingTarget(prefab);
            if (go == null) {
                throw new NullReferenceException($"target not found={Name} in prefab={prefab.name}");
            }
            UIClickComponent doubleTapComponent = go.GetComponent<UIClickComponent>() ?? go.AddComponent<UIClickComponent>();
            doubleTapComponent.OnDoubleClick += () => MethodInfo.Invoke(controller, null);
        }
    }
}
