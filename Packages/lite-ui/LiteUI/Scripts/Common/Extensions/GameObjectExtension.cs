using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LiteUI.Common.Extensions
{
    [PublicAPI]
    public static class GameObjectExtension
    {
        private static readonly TimeSpan REGEX_TIMEOUT = TimeSpan.FromSeconds(0.5f); 
        
        public static void SetLayerRecursively(this GameObject gameObject, int layer, bool includeNotActive = false)
        {
            gameObject.layer = layer;
            Transform[] allChildren = gameObject.GetComponentsInChildren<Transform>(includeNotActive);
            foreach (Transform child in allChildren) {
                child.gameObject.layer = layer;
            }
        }
        
        public static bool IsNullOrDestroyed(this System.Object obj) {

            if (ReferenceEquals(obj, null)) {
                return true;
            }
 
            if (obj is Object o) {
                return o == null;
            }

            return false;
        }

        public static void DestroyAllChildren(this GameObject gameObject)
        {
            foreach (Transform child in gameObject.transform) {
                Object.Destroy(child.gameObject);
            }
        }

        public static T GetOrCreateComponent<T>(this GameObject gameObject)
                where T : Component
        {
            return (T) GetOrCreateComponent(gameObject, typeof(T));
        }

        public static Component GetOrCreateComponent(this GameObject gameObject, Type concreteType)
        {
            Component result = gameObject.GetComponent(concreteType);
            if (result == null) {
                result = gameObject.AddComponent(concreteType);
            }
            return result;
        }
        public static T RequireComponent<T>(this GameObject gameObject) 
                where T : class
        {
            Component result = gameObject.GetComponent(typeof(T));
            if (result == null) {
                throw new NullReferenceException($"component not found type={typeof(T).Name} object={gameObject.name}");
            }
            return (result as T)!;
        }
        
        public static T RequireComponentInChildren<T>(this GameObject gameObject)
                where T : class
        {
            T result = gameObject.GetComponentInChildren<T>();
            if (result == null) {
                throw new NullReferenceException($"child component not found type={typeof(T).Name} object={gameObject.name}");
            }
            return result;
        }

        public static T RequireComponentInChildren<T>(this GameObject gameObject, string childName) 
                where T : class
        {
            return gameObject.transform.RequireComponentInChildren<T>(childName);
        }

        
        public static T RequireComponentInChildren<T>(this Component component, bool includeInactive = true) 
                where T : class
        {
            return component.GetComponentInChildren<T>(includeInactive)
                   ?? throw new NullReferenceException($"Component {typeof(T).Name} not found in children of object {component.name}");
        }
        
        public static T[] GetComponentsOnlyInChildren<T>(this GameObject gameObject, bool includeNotActive = false) 
                where T : class
        {
            return gameObject.GetComponentsInChildren<T>(includeNotActive)
                             .Where(classInChild => {
                                 Component? componentInChild = classInChild as Component;
                                 return componentInChild != null && componentInChild.gameObject != gameObject;
                             })
                             .ToArray();
        }

        public static List<GameObject> GetChildren(this GameObject gameObject)
        {
            List<GameObject> result = new();
            for (int i = 0; i < gameObject.transform.childCount; i++) {
                Transform t = gameObject.transform.GetChild(i);
                result.Add(t.gameObject);
            }

            return result;
        }

        public static GameObject RequireChildRecursive(this GameObject gameObject, string name, bool includeNotActive = false)
        {
            GameObject? go = GetChildRecursive(gameObject, name, includeNotActive);
            if (go == null) {
                throw new NullReferenceException($"child not found for object name='{name}' objectName='{gameObject.name}'");
            }
            return go;
        }

        public static GameObject? GetChildRecursive(this GameObject gameObject, string name, bool includeNotActive = false)
        {
            Transform? child = gameObject.transform.GetChildRecursive(name, includeNotActive);
            return child != null ? child.gameObject : null;
        }

        public static bool HasChildRecursive(this GameObject gameObject, GameObject child, bool includeNotActive = false)
        {
            Transform[] childComponents = gameObject.GetComponentsOnlyInChildren<Transform>(includeNotActive);
            Transform? t = childComponents.FirstOrDefault(childComponent => childComponent.gameObject == child);
            return t != null;
        }

        public static GameObject? GetChildByPath(this GameObject gameObject, string path, bool includeNotActive = false, bool strict = false)
        {
            string[] pathParts = path.Split('/');

            GameObject? obj = gameObject;
            foreach (string pathPart in pathParts) {
                string pathItem = strict ? $"^{pathPart}$" : pathPart;
                obj = obj.GetChildByPattern(pathItem, includeNotActive);
                if (obj == null) {
                    return null;
                }
            }
            return obj;
        }

        public static List<GameObject> GetChildrenByPath(this GameObject gameObject, string path, bool includeNotActive = false)
        {
            string[] names = path.Split('/');
            List<GameObject> objs = new() {
                    gameObject
            };

            foreach (string name in names) {
                List<List<GameObject>> list = objs.Select(o => o.GetChildrenByPattern(name, includeNotActive))
                                                  .Where(objects => objects.Count > 0)
                                                  .ToList();
                objs = new List<GameObject>();
                list.ForEach(objects => objs.AddRange(objects));
                if (objs.Count == 0) {
                    return new List<GameObject>();
                }
            }

            return objs;
        }

        public static GameObject? GetChildByPattern(this GameObject gameObject, string pattern, bool includeNotActive = false)
        {
            Transform[] childComponents = gameObject.GetComponentsOnlyInChildren<Transform>(includeNotActive);
            Transform? t = childComponents.FirstOrDefault(c => Regex.IsMatch(c.name, pattern, RegexOptions.None, REGEX_TIMEOUT));
            return t == null ? null : t.gameObject;
        }

        public static List<GameObject> GetChildrenByPattern(this GameObject gameObject, string pattern, bool includeNotActive = false)
        {
            Transform[] childComponents = gameObject.GetComponentsOnlyInChildren<Transform>(includeNotActive);
            return childComponents
                   .Where(c => {
                       GameObject childObject = c.gameObject;
                       if (!Regex.IsMatch(childObject.name, pattern, RegexOptions.None, REGEX_TIMEOUT)) {
                           return false;
                       }
                       return includeNotActive || childObject.activeInHierarchy;
                   })
                   .Select(transform => transform.gameObject)
                   .ToList();
        }
        
        public static List<GameObject> GetChildrenByName(this GameObject gameObject, string name, bool includeNotActive = false)
        {
            List<GameObject> result = new();
            Transform[] childComponents = gameObject.GetComponentsInChildren<Transform>(includeNotActive);
            foreach (Transform transform in childComponents) {
                if (transform.name == name) {
                    result.Add(transform.gameObject);
                }
            }
            return result;
        }

        public static string GetGameObjectPath(this GameObject target)
        {
            Transform targetTransform = target.transform;
            string path = targetTransform.name;
            while (targetTransform.parent != null)
            {
                targetTransform = targetTransform.parent;
                path = $"{targetTransform.name}/{path}";
            }
            return path;
        }
    }
}
