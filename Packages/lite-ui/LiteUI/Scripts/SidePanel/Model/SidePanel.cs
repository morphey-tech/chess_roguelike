using System;
using System.Collections.Generic;
using LiteUI.Common.Model;
using UnityEngine;

namespace LiteUI.SidePanel.Model
{
    public class SidePanel
    {
        public Type PanelType { get; private set; }
        public int OperationIndex { get; private set; }
        public GameObject? PanelObject { get; set; }
        public List<Direction> Dependent { get; } = new();

        public SidePanel(Type panelType, int operationIndex)
        {
            PanelType = panelType;
            OperationIndex = operationIndex;
        }
    }
}
