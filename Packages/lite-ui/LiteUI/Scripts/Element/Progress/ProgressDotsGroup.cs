using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LiteUI.Element.Progress
{
    public class ProgressDotsGroup : MonoBehaviour
    {
        [SerializeField]
        private ProgressDotItem _dotPrefab = null!;
        [SerializeField]
        private Transform _container = null!;

        private readonly List<ProgressDotItem> _items = new();

        private int _maxCount;
        private int _count;

        public void SetMaxCount(int maxCount)
        {
            _maxCount = maxCount;
            DrawItemsCount(_maxCount);

            _count = Mathf.Min(_count, maxCount);
            RefreshItems();
        }

        public void SetCount(int count)
        {
            _maxCount = Mathf.Max(_maxCount, count);
            DrawItemsCount(_maxCount);
            
            _count = count;
            RefreshItems();
        }

        private void RefreshItems()
        {
            for (int i = 0; i < _maxCount; i++) {
                _items[i].On = i < _count;
            }
        }

        private void DrawItemsCount(int count)
        {
            while (_items.Count > count) {
                ProgressDotItem lastItem = _items.Last();
                Destroy(lastItem.gameObject);
                _items.Remove(lastItem);
            }
            while (_items.Count < count) {
                ProgressDotItem newItem = Instantiate(_dotPrefab, _container);
                newItem.gameObject.SetActive(true);
                _items.Add(newItem);
            }
        }
    }
}
