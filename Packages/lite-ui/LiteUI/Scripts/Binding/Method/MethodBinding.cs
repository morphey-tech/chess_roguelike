using System.Reflection;
using LiteUI.Common.Extensions;
using UnityEngine;

namespace LiteUI.Binding.Method
{
    public abstract class MethodBinding
    {
        protected string? Name { get; }

        protected MethodInfo MethodInfo { get; }

        protected MethodBinding(string? name, MethodInfo methodInfo)
        {
            Name = name;
            MethodInfo = methodInfo;
        }

        public abstract void Bind(MonoBehaviour controller, GameObject prefab);

        protected GameObject? FindBindingTarget(GameObject prefab)
        {
            GameObject go = prefab;
            return Name == null ? go : prefab.GetChildRecursive(Name, true);
        }
    }
}
