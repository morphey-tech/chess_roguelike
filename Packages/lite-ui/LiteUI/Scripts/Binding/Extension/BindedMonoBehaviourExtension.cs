using UnityEngine;

namespace LiteUI.Binding.Extension
{
    public static class BindedMonoBehaviourExtension
    {
        public static void BindComponents(this MonoBehaviour monoBehaviour)
        {
            BindingMonoBehaviourService.Instance.BindComponents(monoBehaviour);
        }
    }
}
