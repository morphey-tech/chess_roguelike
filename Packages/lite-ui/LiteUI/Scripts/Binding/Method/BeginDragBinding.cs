using System;
using System.Reflection;
using LiteUI.Binding.Components;
using UnityEngine;

namespace LiteUI.Binding.Method
{
    public class BeginDragBinding : MethodBinding
    {
        public BeginDragBinding(string name, MethodInfo methodInfo) : base(name, methodInfo)
        {
        }

        public override void Bind(MonoBehaviour controller, GameObject prefab)
        {
            GameObject? go = FindBindingTarget(prefab);
            if (go == null) {
                throw new NullReferenceException($"target not found={Name} in prefab={prefab.name}");
            }
            UIDragAndDropComponent component = go.GetComponent<UIDragAndDropComponent>() ?? go.AddComponent<UIDragAndDropComponent>();
            component.OnBeginDragAction += d => MethodInfo.Invoke(controller, new object[] { d });
        }
    }
}
