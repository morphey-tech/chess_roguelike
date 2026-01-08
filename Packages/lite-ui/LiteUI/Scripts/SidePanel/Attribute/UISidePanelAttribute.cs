using LiteUI.Common.Model;

namespace LiteUI.SidePanel.Attribute
{
    public class UISidePanelAttribute : System.Attribute
    {
        public Direction Side { get; private set; }
        
        public float ShowOffset { get; private set; }
        
        public UISidePanelAttribute(Direction side, float showOffset)
        {
            Side = side;
            ShowOffset = showOffset;
        }
        
        public UISidePanelAttribute(Direction side)
        {
            Side = side;
            ShowOffset = 0;
        }
    }
}
