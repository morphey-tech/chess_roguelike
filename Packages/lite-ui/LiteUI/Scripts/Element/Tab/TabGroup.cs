using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LiteUI.Common.Extensions;
using UnityEngine;

namespace LiteUI.Element.Tab
{
    [PublicAPI]
    public class TabGroup : MonoBehaviour
    {
        [SerializeField]
        private TabElement? _selected;
        [SerializeField]
        private Transform? _container;
        
        private readonly List<TabElement> _tabItems = new();
        private readonly Dictionary<TabElement, bool> _deferredAddTabs = new();

        public event Action<TabElement>? OnTabSelected;

        private bool _inited;
        private TabElement? _deferredSelect;
        
        public bool Disabled { get; set; }

        private void Start()
        {
            TryToInit();
        }

        private void OnDestroy()
        {
            _tabItems.ForEach(i => i.OnTabClick -= OnTabClick);
        }

        public void AddTab(TabElement tabElement, bool selected = false)
        {
            if (tabElement == null) {
                throw new NullReferenceException("Incorrect tab element");
            }

            if (!_inited) {
                _deferredAddTabs.Add(tabElement, selected);
                return;
            }
            
            AttachTabToGroup(tabElement);

            if (selected) {
                ToggleTab(tabElement);
            }
        }

        public void AddTabByIndex(GameObject tab, int index)
        {
            TabElement tabElement = tab.GetComponent<TabElement>();
            if (tabElement == null) {
                throw new NullReferenceException("Incorrect tab element");
            }

            if (!_inited) {
                throw new Exception("Invalid state. Method call after init tab group.");
            }

            if (_tabItems.Contains(tabElement)) {
                _tabItems.Remove(tabElement);
            }
            _tabItems.Insert(index, tabElement);
            tabElement.TabGroup = this;
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
            _selected = tab;

            foreach (TabElement item in _tabItems) {
                if (item == tab) {
                    item.SetSelectionState(TabSelectionState.PRESSED);
                    item.SetContentActive(true);
                } else if (item.SelectionState != TabSelectionState.DISABLED) {
                    item.SetSelectionState(TabSelectionState.NORMAL);
                    item.SetContentActive(false);
                }
            }
            
            OnTabSelected?.Invoke(tab);
        }

        public void ToggleTabByName(string tabName)
        {
            if (!_inited) {
                _deferredSelect = _tabItems.FirstOrDefault(t => t.name == tabName);
                return;
            }

            TabElement? tabForSelect = _tabItems.FirstOrDefault(t => t.gameObject.name == tabName);
            if (tabForSelect != null) {
                ToggleTab(tabForSelect);
            }
        }

        public void RemoveTab(string tabName)
        {
            bool removed = false;

            foreach (TabElement tabItem in _tabItems) {
                if (tabItem.name != tabName) {
                    continue;
                }
                removed = _tabItems.Remove(tabItem);
                Destroy(tabItem.gameObject);
                break;
            }

            if (!removed || _tabItems.Count <= 0) {
                return;
            }
            _tabItems[0].TabGroup = this;
            ToggleTab(_tabItems[0]);
        }

        public int FindTabIndex(TabElement tab)
        {
            return _tabItems.IndexOf(tab);
        }

        public TabElement? FindTabByIndex(int index)
        {
            if (index < 0 || index >= _tabItems.Count) {
                return null;
            }
            return _tabItems[index];
        }

        public void RemoveAllTabs()
        {
            foreach (TabElement tabButtonComponent in _tabItems) {
                Destroy(tabButtonComponent.gameObject);
            }
            _selected = null;
            _tabItems.Clear();
        }
        
        private void OnTabClick(TabElement tab)
        {
            ToggleTab(tab);
        }

        private void AttachTabToGroup(TabElement tabElement)
        {
            _tabItems.Add(tabElement);
            tabElement.OnTabClick += OnTabClick;
        }


        private void TryToInit()
        {
            if (_inited) {
                return;
            }
            
            TabElement[] tabElements = Container.GetComponentsInChildren<TabElement>();

            foreach (TabElement tabButtonComponent in tabElements) {
                AttachTabToGroup(tabButtonComponent);
            }

            if (_deferredAddTabs.Count > 0) {
                foreach (KeyValuePair<TabElement, bool> pair in _deferredAddTabs) {
                    TabElement tabElement = pair.Key.GetComponent<TabElement>();
                    if (!_tabItems.Contains(tabElement)) {
                        AttachTabToGroup(tabElement);
                    }
                    if (pair.Value) {
                        _selected = pair.Key;
                    }
                }
                _deferredAddTabs.Clear();
            }

            _inited = true;

            if (_tabItems.Count == 0) {
                return;
            }

            ToggleTab(GetInitialTab());
        }
        
        private TabElement GetInitialTab()
        {
            return !_deferredSelect.IsDestroyed() ? _deferredSelect! : _tabItems[0];
        }

        private Transform Container => _container != null ? _container : transform;

        public TabElement? Selected => _selected;
        public List<TabElement> TabItems => new(_tabItems);
    }
}
