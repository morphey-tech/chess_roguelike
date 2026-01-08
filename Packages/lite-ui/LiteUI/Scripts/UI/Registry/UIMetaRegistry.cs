using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LiteUI.Common.Logger;
using LiteUI.Binding.Attributes;
using LiteUI.Binding.Attributes.Method;
using LiteUI.Binding.Field;
using LiteUI.Binding.Method;
using LiteUI.Common.Attributes;
using LiteUI.UI.Model;
using UnityEngine;

namespace LiteUI.UI.Registry
{
    [Injectable]
    public class UIMetaRegistry
    {
        private static readonly IUILogger _logger = LoggerFactory.GetLogger<UIMetaRegistry>();

        private readonly Dictionary<Type, UIMetaInfo> _uiMetaInfos = new();
        private readonly Dictionary<string, UIMetaInfo> _uiMetaInfoByIds = new();

        public void Register(Type type, UIControllerAttribute attribute)
        {
            if (_uiMetaInfoByIds.ContainsKey(attribute.Id)) {
                _logger.Warn($"Duplicate UIController Id={attribute.Id}");
                return;
            }
            UIMetaInfo metaInfo = new(type, attribute.Id);
            ScanBindings(type, metaInfo);
            _uiMetaInfos[metaInfo.Type] = metaInfo;
            _uiMetaInfoByIds[metaInfo.Id] = metaInfo;
        }

        public UIMetaInfo RequireMetaInfo(Type type)
        {
            if (!_uiMetaInfos.ContainsKey(type)) {
                throw new NullReferenceException($"UIController not registered for type {type.Name}");
            }
            return _uiMetaInfos[type];
        }

        public UIMetaInfo RequireMetaInfo(string id)
        {
            if (!_uiMetaInfoByIds.ContainsKey(id)) {
                throw new NullReferenceException($"UIController not registered for type {id}");
            }
            return _uiMetaInfoByIds[id];
        }

        private void ScanBindings(Type type, UIMetaInfo metaInfo)
        {
            foreach (FieldInfo fieldInfo in GetFields(type)) {
                foreach (Attribute attribute in fieldInfo.GetCustomAttributes(false)) {
                    switch (attribute) {
                        case UIObjectBindingAttribute objectAttr:
                            metaInfo.ObjectBindings.Add(new ObjectBinding(objectAttr.Name, fieldInfo));
                            break;
                        case UIComponentBindingAttribute componentAttr:
                            metaInfo.ComponentBindings.Add(new ComponentBinding(componentAttr.Name, fieldInfo.FieldType, fieldInfo,
                                                                                componentAttr.Bind));
                            break;
                    }
                }
            }

            foreach (PropertyInfo propertyInfo in GetProperties(type)) {
                foreach (Attribute attribute in propertyInfo.GetCustomAttributes(false)) {
                    switch (attribute) {
                        case UIObjectBindingAttribute objectBindingAttribute:
                            metaInfo.ObjectBindings.Add(new ObjectBinding(objectBindingAttribute.Name, propertyInfo));
                            continue;
                        case UIComponentBindingAttribute componentBindingAttribute:
                            metaInfo.ComponentBindings.Add(new ComponentBinding(componentBindingAttribute.Name, propertyInfo.PropertyType,
                                                                                propertyInfo, componentBindingAttribute.Bind));
                            continue;
                    }
                }
            }

            foreach (MethodInfo methodInfo in GetMethods(type)) {
                foreach (Attribute attribute in methodInfo.GetCustomAttributes(false)) {
                    switch (attribute) {
                        case UIOnTouchAttribute touchAttribute:
                            metaInfo.MethodBindings.Add(new TouchBinding(touchAttribute.Name, touchAttribute.UseButtonState, methodInfo));
                            continue;
                        case UIOnClickAttribute clickAttribute:
                            metaInfo.MethodBindings.Add(new ClickBinding(clickAttribute.Name, clickAttribute.UseButtonState, methodInfo));
                            continue;
                        case UIOnInputChangeAttribute changeAttribute1:
                            metaInfo.MethodBindings.Add(new InputChangeBinding(changeAttribute1.Name, methodInfo));
                            continue;
                        case UIOnTabAttribute changeAttribute:
                            metaInfo.MethodBindings.Add(new TabSelectedBinding(changeAttribute.Name, methodInfo));
                            continue;
                        case UIOnToggleAttribute toggleAttribute:
                            metaInfo.MethodBindings.Add(new ToggleBinding(toggleAttribute.Name, methodInfo));
                            break;
                        case UIOnLongClickAttribute longTapAttribute:
                            metaInfo.MethodBindings.Add(new LongClickBinding(longTapAttribute.Name, methodInfo));
                            break;
                        case UIOnSwipeAttribute swipeAttribute:
                            metaInfo.MethodBindings.Add(new SwipeBinding(swipeAttribute.Name, methodInfo));
                            break;
                        case UIOnDoubleClickAttribute doubleTapAttribute:
                            metaInfo.MethodBindings.Add(new DoubleClickBinding(doubleTapAttribute.Name, methodInfo));
                            break;
                        case UIOnDropdownAttribute dropdownAttribute:
                            metaInfo.MethodBindings.Add(new DropdownBinding(dropdownAttribute.Name, methodInfo));
                            break;
                        case UIOnJoystickAttribute joystickAttribute:
                            metaInfo.MethodBindings.Add(new JoystickBinding(joystickAttribute.Name, methodInfo));
                            break;
                        case UIOnBeginDragAttribute beginDragAttribute:
                            metaInfo.MethodBindings.Add(new BeginDragBinding(beginDragAttribute.Name, methodInfo));
                            break;
                        case UIOnDragAttribute dragAttribute:
                            metaInfo.MethodBindings.Add(new DragBinding(dragAttribute.Name, methodInfo));
                            break;
                        case UIOnEndDragAttribute endDragAttribute:
                            metaInfo.MethodBindings.Add(new EndDragBinding(endDragAttribute.Name, methodInfo));
                            break;
                        case UIOnDropAttribute dropAttribute:
                            metaInfo.MethodBindings.Add(new DropBinding(dropAttribute.Name, methodInfo));
                            break;
                        case UICreatedAttribute:
                            metaInfo.InitMethod = methodInfo;
                            break;
                    }
                }
            }
        }
        
#pragma warning disable S3011
        private List<FieldInfo> GetFields(Type controllerType)
        {
            List<FieldInfo> result = new();

            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            Type? currentType = controllerType;
            while (currentType != null && currentType != typeof(MonoBehaviour)) {
                foreach (FieldInfo fieldInfo in currentType.GetFields(flags)) {
                    if (result.All(f => f.Name != fieldInfo.Name)) {
                        result.Add(fieldInfo);
                    }
                }
                flags = BindingFlags.NonPublic | BindingFlags.Instance;
                currentType = currentType.BaseType ?? null;
            }
            return result;
        }

        private List<PropertyInfo> GetProperties(Type controllerType)
        {
            List<PropertyInfo> result = new();

            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            Type? currentType = controllerType;
            while (currentType != null && currentType != typeof(MonoBehaviour)) {
                foreach (PropertyInfo propertyInfo in currentType.GetProperties(flags)) {
                    if (result.All(f => f.Name != propertyInfo.Name)) {
                        result.Add(propertyInfo);
                    }
                }
                flags = BindingFlags.NonPublic | BindingFlags.Instance;
                currentType = (Type?) currentType.BaseType;
            }
            return result;
        }

        private List<MethodInfo> GetMethods(Type controllerType)
        {
            List<MethodInfo> result = new();

            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            Type? currentType = controllerType;
            while (currentType != null && currentType != typeof(MonoBehaviour)) {
                foreach (MethodInfo methodInfo in currentType.GetMethods(flags)) {
                    if (result.All(f => f.Name != methodInfo.Name)) {
                        result.Add(methodInfo);
                    }
                }
                flags = BindingFlags.NonPublic | BindingFlags.Instance;
                currentType = (Type?) currentType.BaseType;
            }
            return result;
        }
#pragma warning restore S3011
    }
}
