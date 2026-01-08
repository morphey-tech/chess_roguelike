using System;
using System.Reflection;
using LiteUI.Binding.Components;
using UnityEngine;

namespace LiteUI.Binding.Method
{
    public class TouchBinding : MethodBinding
    {
        private readonly bool _useButtonState;

        public TouchBinding(string? name, bool useButtonState, MethodInfo methodInfo) : base(name, methodInfo)
        {
            _useButtonState = useButtonState;
        }

        public override void Bind(MonoBehaviour controller, GameObject prefab)
        {
            GameObject? go = FindBindingTarget(prefab);
            if (go == null) {
                throw new NullReferenceException($"target not found={Name} in prefab={prefab.name}");
            }

            UIClickComponent clickComponent = go.GetComponent<UIClickComponent>() ?? go.AddComponent<UIClickComponent>();
            clickComponent.Init(_useButtonState);
            clickComponent.OnTouch += () => MethodInfo.Invoke(controller, null);
        }
    }
}
