using System;
using System.Reflection;
using JetBrains.Annotations;
using UnityEngine;

namespace LiteUI.Common.Extensions
{
    [PublicAPI]
    public static class MonoBehaviourExtension
    {
        public static bool IsDestroyed(this MonoBehaviour? target)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            return target == null || target.gameObject == null;
        }
        
        public static void SetActive(this Component component, bool active)
        {
            component.gameObject.SetActive(active);
        }
        
        public static T RequireComponentInChildren<T>(this Component component, string childName)
                where T : class
        {
            return GetComponentInChildren<T>(component, childName)
                   ?? throw new NullReferenceException($"Component {typeof(T).Name} not found on child {childName} of object {component.name}");
        }
        
        public static T? GetComponentInChildren<T>(this Component component, string childName)
                where T : class
        {
            return component.transform.GetComponentInChildren<T>(childName);
        }
        
        public static T? GetCopyOf<T>(this Component comp, T other)
                where T : Component
        {
            Type type = comp.GetType();
            if (type != other.GetType()) {
                return null;
            }
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default
                                 | BindingFlags.DeclaredOnly;
            PropertyInfo[] pinfos = type.GetProperties(flags);
            foreach (PropertyInfo pinfo in pinfos) {
                if (!pinfo.CanWrite) {
                    continue;
                }
                try {
                    pinfo.SetValue(comp, pinfo.GetValue(other, null), null);
                } catch {
                    // In case of NotImplementedException being thrown.
                    // For some reason specifying that exception didn't seem to catch it,
                    // So I didn't catch anything specific.
                }
            }
            FieldInfo[] finfos = type.GetFields(flags);
            foreach (FieldInfo finfo in finfos) {
                finfo.SetValue(comp, finfo.GetValue(other));
            }
            return comp as T;
        }
    }
}
