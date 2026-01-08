using System.Reflection;
using UnityEngine;

namespace LiteUI.Binding.Field
{
    public abstract class FieldBinding
    {
        public string? Name { get; }
        private readonly MemberInfo _field;

        protected FieldBinding(string? name, MemberInfo field)
        {
            Name = name;
            _field = field;
        }

        public abstract void Bind(MonoBehaviour controller, GameObject prefab);

        protected void SetField(MonoBehaviour controller, object value)
        {
            FieldInfo? info = _field as FieldInfo;
            if (info != null) {
                info.SetValue(controller, value);
                return;
            }
            (_field as PropertyInfo)?.SetValue(controller, value, null);
        }
    }
}
