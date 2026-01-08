using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static LiteUI.Common.Preconditions;

namespace LiteUI.UI.Service
{
    public class UiObjectCollector : MonoBehaviour
    {
        public T? GetObjectByType<T>(bool includeInactive = false)
                where T : class
        {
            return gameObject.GetComponentInChildren<T>(includeInactive);
        }

        public T RequireObjectByType<T>(bool includeInactive = false)
                where T : class
        {
            return CheckNotNull(gameObject.GetComponentInChildren<T>(includeInactive));
        }

        public List<T> GetObjectsByType<T>(bool includeInactive = false)
        {
            return gameObject.GetComponentsInChildren<T>(includeInactive).ToList();
        }

        public GameObject? GetObjectByName(string objectName, bool includeInactive = false)
        {
            Transform[] transforms = UiContainer.transform.GetComponentsInChildren<Transform>(includeInactive);
            return transforms.FirstOrDefault(t => t.name == objectName)?.gameObject;
        }

        public GameObject RequireObjectByName(string objectName, bool includeInactive = false)
        {
            return CheckNotNull(GetObjectByName(objectName, includeInactive))!;
        }

        public GameObject UiContainer => gameObject;
    }
}
