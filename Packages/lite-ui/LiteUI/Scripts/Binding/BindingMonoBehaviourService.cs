using System;
using System.Collections.Generic;
using System.Reflection;
using LiteUI.Binding.Attributes;
using LiteUI.Common.Attributes;
using UnityEngine;
using VContainer.Unity;

namespace LiteUI.Binding
{
    [Injectable]
    public class BindingMonoBehaviourService : IInitializable
    {
        private static readonly Type BINDED_ATTRIBUTE_TYPE = typeof(BindedAttribute);

        internal static BindingMonoBehaviourService Instance { get; private set; } = null!;

        private readonly Dictionary<Type, BindData> _bindDatas = new();

        void IInitializable.Initialize()
        {
            Instance = this;
        }

        public void BindComponents(MonoBehaviour monoBehaviour)
        {
            Type monoBehaviourType = monoBehaviour.GetType();
            BindData bindData;
            if (_bindDatas.ContainsKey(monoBehaviourType)) {
                bindData = _bindDatas[monoBehaviourType];
            } else {
                bindData = CollectBindData(monoBehaviourType);
                _bindDatas[monoBehaviourType] = bindData;
            }

            GameObject targetObject = monoBehaviour.gameObject;
            if (bindData.FieldsWithoutChildren != null) {
                foreach (FieldInfo fieldInfo in bindData.FieldsWithoutChildren) {
                    fieldInfo.SetValue(monoBehaviour, targetObject.GetComponent(fieldInfo.FieldType));
                }
            }
            if (bindData.FieldsWithChildren != null) {
                foreach (FieldInfo fieldInfo in bindData.FieldsWithChildren) {
                    fieldInfo.SetValue(monoBehaviour, targetObject.GetComponentInChildren(fieldInfo.FieldType));
                }
            }
            if (bindData.PropertiesWithoutChildren != null) {
                foreach (PropertyInfo propertyInfo in bindData.PropertiesWithoutChildren) {
                    propertyInfo.SetValue(monoBehaviour, targetObject.GetComponent(propertyInfo.PropertyType));
                }
            }
            if (bindData.PropertiesWithChildren != null) {
                foreach (PropertyInfo propertyInfo in bindData.PropertiesWithChildren) {
                    propertyInfo.SetValue(monoBehaviour, targetObject.GetComponentInChildren(propertyInfo.PropertyType));
                }
            }
        }

        private BindData CollectBindData(Type targetType)
        {
            BindData result = new();
            Type? type = targetType;
            while (type != null) {
                CollectFieldData(type, ref result);
                CollectPropertyData(type, ref result);

                type = type.BaseType;
                if (type == typeof(MonoBehaviour)) {
                    break;
                }
            }

            return result;
        }

        private void CollectFieldData(Type type, ref BindData bindData)
        {
            FieldInfo[] fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            foreach (FieldInfo fieldInfo in fields) {
                if (!fieldInfo.IsDefined(BINDED_ATTRIBUTE_TYPE)) {
                    continue;
                }
                if (fieldInfo.GetCustomAttribute<BindedAttribute>().IncludeChildren) {
                    bindData.FieldsWithChildren ??= new List<FieldInfo>();
                    bindData.FieldsWithChildren.Add(fieldInfo);
                } else {
                    bindData.FieldsWithoutChildren ??= new List<FieldInfo>();
                    bindData.FieldsWithoutChildren.Add(fieldInfo);
                }
            }
        }
        
        private void CollectPropertyData(Type type, ref BindData bindData)
        {
            PropertyInfo[] properties = type.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo propertyInfo in properties) {
                if (!propertyInfo.IsDefined(BINDED_ATTRIBUTE_TYPE)) {
                    continue;
                }
                if (propertyInfo.GetCustomAttribute<BindedAttribute>().IncludeChildren) {
                    bindData.PropertiesWithChildren ??= new List<PropertyInfo>();
                    bindData.PropertiesWithChildren.Add(propertyInfo);
                } else {
                    bindData.PropertiesWithoutChildren ??= new List<PropertyInfo>();
                    bindData.PropertiesWithoutChildren.Add(propertyInfo);
                }
            }
        }

        private class BindData
        {
            public List<FieldInfo>? FieldsWithChildren { get; set; }
            public List<FieldInfo>? FieldsWithoutChildren { get; set; }
            public List<PropertyInfo>? PropertiesWithChildren { get; set; }
            public List<PropertyInfo>? PropertiesWithoutChildren { get; set; }
        }
    }
}
