using System;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LiteUI.Binding.Method
{
    public class InputChangeBinding : MethodBinding
    {
        public InputChangeBinding(string name, MethodInfo methodInfo) : base(name, methodInfo)
        {
        }

        public override void Bind(MonoBehaviour controller, GameObject prefab)
        {
            GameObject? go = FindBindingTarget(prefab);
            if (go == null) {
                throw new NullReferenceException($"target not found={Name} in prefab={prefab.name}");
            }

            InputField input = go.GetComponent<InputField>();
            if (input != null) {
                input.onValueChanged.AddListener(delegate { MethodInfo.Invoke(controller, null); });
                return;
            }

            TMP_InputField tpmInput = go.GetComponent<TMP_InputField>();
            if (tpmInput == null) {
                throw new ArgumentException($"Input not found prefab={prefab.name}");
            }

            tpmInput.onValueChanged.AddListener(delegate { MethodInfo.Invoke(controller, null); });
        }
    }
}
