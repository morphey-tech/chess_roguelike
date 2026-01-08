using System;
using System.Linq;
using System.Reflection;
using LiteUI.Common.Logger;
using LiteUI.Binding.Attributes;
using LiteUI.Binding.Field;
using LiteUI.Binding.Method;
using LiteUI.Common.Extensions;
using LiteUI.UI.Model;
using LiteUI.UI.Registry;
using UnityEngine;
using VContainer;

namespace LiteUI.Binding
{
    public class BindingService
    {
        private static readonly IUILogger _logger = LoggerFactory.GetLogger<BindingService>();

        private readonly IObjectResolver _resolver;
        private readonly UIMetaRegistry _uiMetaRegistry;

        [Inject]
        public BindingService(IObjectResolver resolver, UIMetaRegistry uiMetaRegistry)
        {
            _resolver = resolver;
            _uiMetaRegistry = uiMetaRegistry;
        }

        public void Bind(GameObject uiObject, Type controllerType, object?[]? initParams)
        {
            UIMetaInfo metaInfo = _uiMetaRegistry.RequireMetaInfo(controllerType);

            MonoBehaviour controller = (MonoBehaviour) (uiObject.GetComponent(metaInfo.Type) ?? uiObject.AddComponent(metaInfo.Type));
            try {
                _resolver.Inject(controller);
            } catch (Exception e) {
                _logger.Error($"Error while inject to controller={metaInfo.Type.Name} prefab={uiObject.name}", e);
                throw;
            }

            BindObjects(metaInfo, controller);
            BindComponents(metaInfo, controller);
            BindMethods(metaInfo, controller);
            InvokeUICreated(metaInfo, controller, initParams);
        }

        private void BindObjects(UIMetaInfo metaInfo, MonoBehaviour controller)
        {
            foreach (ObjectBinding childBinding in metaInfo.ObjectBindings) {
                childBinding.Bind(controller, controller.gameObject);
            }
        }

        private void BindComponents(UIMetaInfo metaInfo, MonoBehaviour controller)
        {
            foreach (ComponentBinding component in metaInfo.ComponentBindings) {
                if (component.BindController) {
                    GameObject child = component.Name == null
                                               ? controller.gameObject
                                               : controller.gameObject.RequireChildRecursive(component.Name, true);
                    Bind(child, component.ComponentType, null);
                }
                component.Bind(controller, controller.gameObject);
            }
        }

        private void BindMethods(UIMetaInfo metaInfo, MonoBehaviour controller)
        {
            foreach (MethodBinding methodBinding in metaInfo.MethodBindings) {
                methodBinding.Bind(controller, controller.gameObject);
            }
        }

        private void InvokeUICreated(UIMetaInfo metaInfo, MonoBehaviour controller, object?[]? initParams)
        {
            if (metaInfo.InitMethod == null) {
                return;
            }

            try {
                ParameterInfo[] parameters = metaInfo.InitMethod.GetParameters();
                object?[] args = new object?[parameters.Length];
                for (int i = 0; i < args.Length; i++)
                {
                    if (i < initParams?.Length) {
                        args[i] = initParams[i];
                    }
                    else if (parameters[i].HasDefaultValue) {
                        args[i] = parameters[i].DefaultValue;
                    }
                }
                metaInfo.InitMethod.Invoke(controller, args);
            } catch (TargetParameterCountException) {
                string objectName = controller.gameObject.name;
                _logger.Error($"Incorrect parameters count at init method call for object '{objectName}' with class '{controller.GetType().Name}'");
            } catch (InvalidCastException) {
                string objectName = controller.gameObject.name;
                _logger.Error($"Incorrect parameters at init method call for object '{objectName}' with class '{controller.GetType().Name}'");
            } catch (ArgumentException) {
                string wait = string.Concat(metaInfo.InitMethod.GetParameters().Select(o => $"{o.ParameterType}, ").ToArray());
                string get = initParams != null ? string.Concat(initParams.Select(o => $"{o?.GetType()}, ").ToArray()) : "{}";
                string prefabName = metaInfo.Type.GetCustomAttribute<UIControllerAttribute>()?.Id ?? "Unknown prefab";
                _logger.Error($"Incorrect parameters type wait={wait} but get={get}', prefab name '{prefabName}'");
            } catch (Exception e) {
                string stackTrace = (e.InnerException != null ? e.InnerException.StackTrace : e.StackTrace);
                _logger.Error($"Exception at invoke init with message '{e.Message}' {stackTrace}");
                throw;
            }
        }
    }
}
