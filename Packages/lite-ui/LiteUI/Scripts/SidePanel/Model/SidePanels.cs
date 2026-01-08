using System.Collections.Generic;
using LiteUI.Common.Logger;
using LiteUI.Common.Model;
using UnityEngine;

namespace LiteUI.SidePanel.Model
{
    public class SidePanels
    {
        private static readonly IUILogger _logger = LoggerFactory.GetLogger<SidePanels>();
        
        private readonly Dictionary<Direction, SidePanel> _panels = new();

        public Direction? FindPanelSide(GameObject panel)
        {
            foreach (KeyValuePair<Direction, SidePanel> pair in _panels) {
                if (pair.Value.PanelObject == panel) {
                    return pair.Key;
                }
            }
            return null;
        }

        public List<Direction>? GetChildPanels(Direction side)
        {
            if (!_panels.ContainsKey(side)) {
                return null;
            }
            return _panels[side].Dependent;
        }

        public GameObject? GetPanel(Direction side)
        {
            if (!_panels.ContainsKey(side)) {
                return null;
            }
            return _panels[side].PanelObject;
        }

        public void Remove(Direction side)
        {
            _panels.Remove(side);
            foreach (KeyValuePair<Direction, SidePanel> pair in _panels) {
                pair.Value.Dependent.Remove(side);
            }
        }

        public void Add<T>(int operationIndex, Direction side, GameObject? parentPanel)
                where T : MonoBehaviour
        {
            if (_panels.ContainsKey(side)) {
                _logger.Warn($"Panels already exist on side {side} newPanel={typeof(T).Name} oldPanel={_panels[side].PanelType.Name}");
            }
            _panels[side] = new SidePanel(typeof(T), operationIndex);
            if (parentPanel == null) {
                return;
            }
            Direction? sideOfParent = FindPanelSide(parentPanel);
            if (sideOfParent == null) {
                _logger.Warn($"Parent not found. RequiredParent={parentPanel.name}. panel={typeof(T).Name} side={side}");
                return;
            }
            _panels[sideOfParent.Value].Dependent.Add(side);
        }

        public void UpdatePanel(Direction side, GameObject panel)
        {
            if (!_panels.ContainsKey(side)) {
                _logger.Warn($"No panel on side to update side={side} panel={panel.name}");
                return;
            }
            _panels[side].PanelObject = panel;
        }

        public bool IsStillNeed(int uid, Direction side)
        {
            if (!_panels.ContainsKey(side)) {
                return false;
            }
            return _panels[side].OperationIndex == uid;
        }

        public bool IsPanelVisible<T>(Direction side)
        {
            if (!_panels.ContainsKey(side)) {
                return false;
            }
            return _panels[side].PanelType == typeof(T);
        }
    }
}
