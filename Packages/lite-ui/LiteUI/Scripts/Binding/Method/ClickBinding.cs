using System;
using System.Reflection;
using LiteUI.Binding.Components;
using UnityEngine;
using UnityEngine.UI;

namespace LiteUI.Binding.Method
{
    public class ClickBinding : MethodBinding
    {
        private readonly bool _useButtonState;
        
        public ClickBinding(string? name, bool useButtonState, MethodInfo methodInfo) : base(name, methodInfo)
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
            clickComponent.OnClick += () => MethodInfo.Invoke(controller, null);
        }
    }
}
