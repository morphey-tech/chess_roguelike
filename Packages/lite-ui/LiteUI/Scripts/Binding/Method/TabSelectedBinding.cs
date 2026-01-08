using System;
using System.Reflection;
using LiteUI.Element.Tab;
using UnityEngine;

namespace LiteUI.Binding.Method
{
    public class TabSelectedBinding : MethodBinding
    {
        public TabSelectedBinding(string name, MethodInfo methodInfo) : base(name, methodInfo)
        {
        }

        public override void Bind(MonoBehaviour controller, GameObject prefab)
        {
            GameObject? go = FindBindingTarget(prefab);
            if (go == null) {
                throw new NullReferenceException("target not found = " + Name + " in prefab = " + prefab.name);
            }
            TabGroup tabGroup = go.GetComponent<TabGroup>();
            if (tabGroup == null) {
                throw new ArgumentException("tab group not found prefab=" + prefab.name + " Id=" + Name);
            }

            
            tabGroup.OnTabSelected += (tab => { MethodInfo.Invoke(controller, new object[] { tab.GetComponent<TabElement>() }); });
        }
    }
}
