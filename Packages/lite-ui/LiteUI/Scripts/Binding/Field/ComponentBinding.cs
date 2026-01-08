using System;
using System.Collections.Generic;
using System.Reflection;
using LiteUI.Common.Logger;
using LiteUI.Common.Extensions;
using UnityEngine;

namespace LiteUI.Binding.Field
{
    public class ComponentBinding : FieldBinding
    {
        private static readonly IUILogger _logger = LoggerFactory.GetLogger<ComponentBinding>();

        public Type ComponentType { get; }
        public bool BindController { get; }

        public ComponentBinding(string? name, Type componentType, MemberInfo field, bool bindController = false) : base(name, field)
        {
            ComponentType = componentType;
            BindController = bindController;
        }

        public override void Bind(MonoBehaviour controller, GameObject prefab)
        {
            if (!typeof(Component).IsAssignableFrom(ComponentType)) {
                throw new ArgumentException($"Bad component type '{ComponentType.Name}' for object='{Name}'" 
                                            + $" in prefab='{prefab.name}' at controller='{controller.GetType().Name}'");
            }

            GameObject child = prefab;
            if (Name != null) {
                List<GameObject> children = prefab.GetChildrenByName(Name, true);
                if (children.Count == 0) {
                    throw new ArgumentException($"Not found child object with name='{Name}' and type='{ComponentType.Name}' " 
                                                + $" in prefab='{prefab.name}' at controller='{controller.GetType().Name}'");
                }
                if (children.Count > 1) {
                    _logger.Warn($"Prefab has different children with name='{Name}' Prefab='{prefab.name}'");
                                 
                }
                child = children[0];
            }
            
            Component component = child.GetComponent(ComponentType);
            if (component == null) {
                throw new ArgumentException($"Not found component for child name='{Name}' and type='{ComponentType.Name}' "
                                            + $" in prefab='{prefab.name}' at controller='{controller.GetType().Name}' Bind={BindController}");
            }
            SetField(controller, component);
        }
    }
}
