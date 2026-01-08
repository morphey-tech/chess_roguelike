using System;
using System.Reflection;
using UnityEngine;

namespace LiteUI.Binding.Method
{
    public class SwipeBinding : MethodBinding
    {
        public SwipeBinding(string? name, MethodInfo methodInfo) : base(name, methodInfo)
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
            // UISwipeComponent component = go.GetComponent<UISwipeComponent>() ?? go.AddComponent<UISwipeComponent>();
            // component.OnSwipe.AddListener(delegate(Swipe swipe) { MethodInfo.Invoke(controller, new object[] {swipe}); });
#pragma warning restore S125
        }
    }
}
