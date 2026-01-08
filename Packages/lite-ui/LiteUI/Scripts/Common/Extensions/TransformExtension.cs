using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LiteUI.Common.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LiteUI.Common.Extensions
{
    [PublicAPI]
    public static class TransformExtension
    {
        
        public static void LookAt2D(this Transform transform, Transform target)
        {
            transform.LookAt2D(target.position);
        }

        public static void LookAt2D(this Transform transform, Vector3 target)
        {
            Vector3 direction = MathUtils.Direction(transform.position, target);
            direction.y = transform.forward.y;
            if (direction == Vector3.zero) {
                return;
            }
            transform.forward = direction;
        }
        
        public static void DestroyAllChildren(this Transform transform)
        {
            foreach (Transform child in transform) {
                Object.Destroy(child.gameObject);
            }
        }
        
        public static void DestroyAllChildrenImmediate(this Transform transform)
        {
          foreach (Transform child in transform) {
            Object.DestroyImmediate(child.gameObject);
          }
        }
        
        public static T RequireComponent<T>(this Transform transform)
                where T : class
        {
            return transform.gameObject.RequireComponent<T>();
        }

        public static Transform RequireChildRecursive(this Transform transform, string name, bool includeNotActive = false)
        {
            Transform? go = GetChildRecursive(transform, name, includeNotActive);
            if (go == null) {
                throw new NullReferenceException($"child not found for object name='{name}' objectName='{transform.name}'");
            }
            return go;
        }
        
        public static T? GetComponentInChildren<T>(this Transform transform, string childName, bool includeInactive = true)
                where T : class
        {
            foreach (Transform child in transform) {
                if (!includeInactive && !child.gameObject.activeSelf) {
                    continue;
                }

                if (child.name == childName) {
                    return child.GetComponent<T>();
                }

                T? found = GetComponentInChildren<T>(child, childName, includeInactive);
                if (found != null) {
                    return found;
                }
            }

            return null;
        }

        public static Transform? GetChildRecursive(this Transform transform, string name, bool includeNotActive = false)
        {
            foreach (Transform child in transform) {
                if (!includeNotActive && !child.gameObject.activeSelf) {
                    continue;
                }
                if (child.name == name) {
                    return child;
                }
                Transform? founded = child.GetChildRecursive(name, includeNotActive);
                if (founded != null) {
                    return founded;
                }
            }
            return null;
        }

        public static IEnumerable<Transform> EnumerateAllChildren(this Transform transform)
        {
            foreach (Transform child in transform) {
                yield return child;

                foreach (Transform? subchild in child.EnumerateAllChildren()) {
                    yield return subchild;
                }
            }
        }
    }
}
