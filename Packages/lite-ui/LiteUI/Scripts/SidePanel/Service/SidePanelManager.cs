using System;
using System.Collections.Generic;
using System.Reflection;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using JetBrains.Annotations;
using LiteUI.Common.Model;
using LiteUI.SidePanel.Attribute;
using LiteUI.SidePanel.Model;
using LiteUI.UI.Model;
using LiteUI.UI.Service;
using LiteUI.UI.Tween;
using UnityEngine;
using VContainer;
using static LiteUI.Common.Preconditions;

namespace LiteUI.SidePanel.Service
{
    [PublicAPI]
    public class SidePanelManager
    {
        private const float DEFAULT_SHOW_HIDE_DURATION = 0.4f;

        private readonly UIService _uiService;
        private readonly ScreenLayout _screenLayout;

        private readonly SidePanels _sidePanels = new();

        private GameObject _panelContainer = null!;
        private int _operationIndex;

        private float _showHideDuration;

        [Inject]
        public SidePanelManager(UIService uiService, ScreenLayout screenLayout)
        {
            _uiService = uiService;
            _screenLayout = screenLayout;
        }

        public void AttachRootContainer(GameObject root, float showHideDuration = DEFAULT_SHOW_HIDE_DURATION)
        {
            _panelContainer = CheckNotNull(root);
            _showHideDuration = showHideDuration;
        }

        [MustUseReturnValue]
        public UniTask<T> ShowPanel<T>(params object[] initParams)
                where T : MonoBehaviour
        {
            return ShowPanelAsync<T>(null, initParams);
        }

        [MustUseReturnValue]
        public UniTask<T> ShowChildPanel<T>(GameObject? parentPanel, params object[] initParams)
                where T : MonoBehaviour
        {
            return ShowPanelAsync<T>(parentPanel, initParams);
        }

        [MustUseReturnValue]
        public UniTask HidePanel(Direction side)
        {
            List<Direction>? panelsToHide = _sidePanels.GetChildPanels(side);
            List<UniTask> hideTasks = new();
            if (panelsToHide != null) {
                hideTasks.AddRange(panelsToHide.Select(HidePanel));
            }
            hideTasks.Add(HidePanelAsync(side));
            return UniTask.WhenAll(hideTasks);
        }

        [MustUseReturnValue]
        public UniTask HidePanel(GameObject panel)
        {
            Direction? side = _sidePanels.FindPanelSide(panel);
            if (side == null) {
                return UniTask.CompletedTask;
            }
            return HidePanel(side.Value);
        }

        [MustUseReturnValue]
        public UniTask HidePanel(MonoBehaviour panel)
        {
            return HidePanel(panel.gameObject);
        }

        [MustUseReturnValue]
        public UniTask HideAll()
        {
            List<UniTask> hideTasks = new() {
                    HidePanel(Direction.UP),
                    HidePanel(Direction.DOWN),
                    HidePanel(Direction.LEFT),
                    HidePanel(Direction.RIGHT)
            };
            return UniTask.WhenAll(hideTasks);
        }

        public bool IsPanelVisible<T>()
                where T : MonoBehaviour
        {
            Direction side = GetPanelSide<T>();
            return _sidePanels.IsPanelVisible<T>(side);
        }
        
        private async UniTask<T> ShowPanelAsync<T>(GameObject? parentPanel, object[] initParams)
                where T : MonoBehaviour
        {
            Direction side = GetPanelSide<T>();
            float showOffset = GetPanelShowOffset<T>();
            
            List<UniTask> moveTasks = new();
            moveTasks.Add(HidePanel(side));
            T result = null!;
            UniTask showTask = DoShowPanelAsync<T>(side, showOffset, parentPanel, initParams).ContinueWith(p => result = p);
            moveTasks.Add(showTask);
            await UniTask.WhenAll(moveTasks);
            return result;
        }
        
        private async UniTask<T> DoShowPanelAsync<T>(Direction side, float showOffset, GameObject? parentPanel, object[] initParams)
                where T : MonoBehaviour
        {
            int currentOperation = _operationIndex++;
            _sidePanels.Add<T>(currentOperation, side, parentPanel);
            T panel = await _uiService.CreateAsync<T>(UIModel.Create<T>(initParams).Container(_panelContainer));
            GameObject panelObject = panel.gameObject;
            RectTransform panelRect = panelObject.GetComponent<RectTransform>(); 
            if (!_sidePanels.IsStillNeed(currentOperation, side)) {
                _uiService.Release(panelObject);
                throw new OperationCanceledException();
            }

            _sidePanels.UpdatePanel(side, panelObject);
            panelObject.SetActive(true);
            await DirectionPanelTween.DoHide(_screenLayout, panelObject, side, 0f);
            await DirectionPanelTween.DoShow(_screenLayout, panelObject, side, _showHideDuration, -(panelRect.sizeDelta.x + showOffset));
            return panelObject.GetComponent<T>();
        }

        private async UniTask HidePanelAsync(Direction side)
        {
            GameObject? panel = _sidePanels.GetPanel(side);
            _sidePanels.Remove(side);
            if (panel == null) {
                return;
            }

            await DirectionPanelTween.DoHide(_screenLayout, panel, side, _showHideDuration);
            panel.SetActive(false);
            _uiService.Release(panel);
        }

        private Direction GetPanelSide<T>()
                where T : MonoBehaviour
        {
            Type panelType = typeof(T);
            UISidePanelAttribute uiSidePanelAttribute = CheckNotNull(panelType.GetCustomAttribute<UISidePanelAttribute>());
            return uiSidePanelAttribute.Side;
        }
        
        private float GetPanelShowOffset<T>()
                where T : MonoBehaviour
        {
            Type panelType = typeof(T);
            UISidePanelAttribute uiSidePanelAttribute = CheckNotNull(panelType.GetCustomAttribute<UISidePanelAttribute>());
            return uiSidePanelAttribute.ShowOffset;
        }

        public GameObject PanelContainer => _panelContainer;
    }
}
