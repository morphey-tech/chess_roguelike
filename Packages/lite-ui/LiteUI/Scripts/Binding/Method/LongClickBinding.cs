using System;
using System.Reflection;
using LiteUI.Binding.Components;
using UnityEngine;

namespace LiteUI.Binding.Method
{
    public class LongClickBinding : MethodBinding
    {
        public LongClickBinding(string? name, MethodInfo methodInfo) : base(name, methodInfo)
        {
        }

        public override void Bind(MonoBehaviour controller, GameObject prefab)
        {
            GameObject? go = FindBindingTarget(prefab);
            if (go == null) {
                throw new NullReferenceException($"target not found={Name} in prefab={prefab.name}");
            }
            UIClickComponent component = go.GetComponent<UIClickComponent>() ?? go.AddComponent<UIClickComponent>();
            component.OnLongClick += () => MethodInfo.Invoke(controller, null);
        }
    }
}
