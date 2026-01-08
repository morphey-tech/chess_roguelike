using UnityEngine.UI;

namespace LiteUI.Element.Widgets
{
   
    public class RaycastCatcher : Text
    {
        protected override void Awake()
        {
            base.Awake();
            raycastTarget = true;
        }
    }
}
