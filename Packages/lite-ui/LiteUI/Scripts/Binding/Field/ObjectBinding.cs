using System;
using System.Reflection;
using LiteUI.Common.Extensions;
using UnityEngine;
using static LiteUI.Common.Preconditions;

namespace LiteUI.Binding.Field
{
    public class ObjectBinding : FieldBinding
    {
        public ObjectBinding(string name, MemberInfo memberInfo) : base(name, memberInfo)
        {
            CheckNotNull(Name, $"child name='{Name}' not found");
        }

        public override void Bind(MonoBehaviour controller, GameObject prefab)
        {
            GameObject? child = prefab.GetChildRecursive(Name!, true);
            if (child == null) {
                throw new ArgumentException($"Child with name '{Name}' not found at object '{prefab}'" 
                                            + $" for controller of type '{controller.GetType().Name}'");
            }

            SetField(controller, child);
        }
    }
}
