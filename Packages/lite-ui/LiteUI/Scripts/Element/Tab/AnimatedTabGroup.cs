using System;
using System.Collections.Generic;
using System.Linq;
using LiteUI.Common.Logger;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using LiteUI.Common.Extensions;
using UnityEngine;

namespace LiteUI.Element.Tab
{
    [PublicAPI]
    public class AnimatedTabGroup : MonoBehaviour
    {
        private static readonly IUILogger _logger = LoggerFactory.GetLogger<AnimatedTabGroup>();

        [SerializeField]
        private TabElement? _selected;
        [SerializeField]
        private Transform? _container;

        private readonly List<TabElement> _tabItems = new();

        public event Func<UniTask>? OnTabSwitchStart;
        public event Action<TabElement>? OnTabSwitched;
        public event Action<TabElement, TabSelectionState>? OnSelectionChangedEvent;
        public event Action<TabElement[]>? OnTabGroupInited;

        private bool _inited;
        private bool _switching;
        private TabElement? _deferredSelect;

        private TabElement? _lastSelectedTab;

        public bool Disabled { get; set; }

        private void Start()
        {
            TryToInit();
        }

        private void OnDestroy()
        {
            _tabItems.ForEach(i => i.OnTabClick -= OnTabClick);
            _tabItems.ForEach(i => i.OnSelectionChanged -= OnSelectionChanged);
        }
        
        public void ToggleTab(TabElement tab)
        {
            if (!_inited) {
                _deferredSelect = tab;
                TryToInit();
                return;
            }

            if (Disabled) {
                return;
            }

            _lastSelectedTab = tab;



            ToggleTabAsync(tab).Forget();
        }

        private void OnSelectionChanged(TabElement tab, TabSelectionState state)
        {
            OnSelectionChangedEvent?.Invoke(tab, state);
        }

        private void OnTabClick(TabElement tab)
        {
            ToggleTab(tab);
        }

        private async UniTask ToggleTabAsync(TabElement tabToSelect)
        {
            List<TabElement> tabsToDeselect =
                    _tabItems.Select(t => t).Where(t => t != tabToSelect && t.SelectionState != TabSelectionState.DISABLED).ToList();

            if (_switching) {
                return;
            }
            _switching = true;
            await SelectTabs(tabToSelect, tabsToDeselect);
            if (this.IsDestroyed()) {
                return;
            }
            _switching = false;
            if (ValidateSelectedTab(tabToSelect)) {
                SetTabsContentActive(tabToSelect, tabsToDeselect);
            }
        }

        private async UniTask SelectTabs(TabElement tabToSelect, List<TabElement> tabsToDeselect)
        {
            tabToSelect.SetSelectionState(TabSelectionState.PRESSED);
            tabsToDeselect.ForEach(t => t.SetSelectionState(TabSelectionState.NORMAL));
            if (OnTabSwitchStart != null && _selected != tabToSelect) {
                try {
                    await OnTabSwitchStart.Invoke();
                } catch (OperationCanceledException) {
                    //Операция отменена, игнорируем
                } catch (Exception e) {
                    _logger.Error($"Exception during invoking TabSwitchStart event, exception ={e}");
                }
            }
            _selected = tabToSelect;

        }

        private void SetTabsContentActive(TabElement selectedTab, List<TabElement> deselectedTabs)
        {
            selectedTab.SetContentActive(true);
            deselectedTabs.ForEach(t => { t.SetContentActive(false); });
            OnTabSwitched?.Invoke(selectedTab);
        }

        private bool ValidateSelectedTab(TabElement tabToActivate)
        {
            if (_lastSelectedTab == null || _lastSelectedTab == tabToActivate) {
                return true;
            }
            ToggleTab(_lastSelectedTab);
            return false;
        }

        private void AttachTabToGroup(TabElement tabElement)
        {
            _tabItems.Add(tabElement);
            tabElement.OnTabClick += OnTabClick;
            tabElement.OnSelectionChanged += OnSelectionChanged;
        }

        private void TryToInit()
        {
            if (_inited) {
                return;
            }

            TabElement[] tabElements = Container.GetComponentsInChildren<TabElement>();

            if (tabElements.Length == 0) {
                return;
            }

            foreach (TabElement tabButtonComponent in tabElements) {
                AttachTabToGroup(tabButtonComponent);
            }

            _inited = true;
            OnTabGroupInited?.Invoke(tabElements);

            ToggleTab(GetInitialTab());
        }

        private TabElement GetInitialTab()
        {
            return !_deferredSelect.IsDestroyed() ? _deferredSelect! : _tabItems[0];
        }

        private Transform Container => _container != null ? _container : transform;
    }
}
