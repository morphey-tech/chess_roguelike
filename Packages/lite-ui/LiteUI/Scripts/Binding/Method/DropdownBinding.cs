using System;
using System.Reflection;
using UnityEngine;

namespace LiteUI.Binding.Method
{
    public class DropdownBinding : MethodBinding
    {
        public DropdownBinding(string? name, MethodInfo methodInfo) : base(name, methodInfo)
        {
        }

        public override void Bind(MonoBehaviour controller, GameObject prefab)
        {
            GameObject? go = FindBindingTarget(prefab);
            if (go == null) {
                throw new NullReferenceException($"target not found={Name} in prefab={prefab.name}");
            }
            
            throw new NotImplementedException("todo:");
#pragma warning disable S125
            // UIDropdownComponent component = go.GetComponent<UIDropdownComponent>() ?? go.AddComponent<UIDropdownComponent>();
            // component.OnDropdownEvent.AddListener(delegate(int index) { MethodInfo.Invoke(controller, new object[] {index}); });
#pragma warning restore S125
        }
    }
}
