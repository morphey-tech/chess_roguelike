using LiteUI.Common.Utils;
using UnityEngine;

namespace LiteUI.Element.PooledScroll
{
    public class PooledScrollViewModel : MonoBehaviour
    {
        [SerializeField]
        private int _columnCount = 0;
        [SerializeField]
        private int _rowCount = 0;
        [SerializeField]
        private bool _vertical = true;
        [SerializeField]
        private bool _dynamicItemSize = true;

        [SerializeField]
        private float _topPanelPadding;
        [SerializeField]
        private float _bottomPanelPadding;
        [SerializeField]
        private float _leftPanelPadding;
        [SerializeField]
        private float _rightPanelPadding;
        [SerializeField]
        private float _horizontalItemPadding = 4;
        [SerializeField]
        private float _verticalItemPadding = 4;

        [SerializeField]
        private float _initialWidth;
        [SerializeField]
        private float _initialHeight;

        public int ColumnCount => _columnCount;
        public int RowCount => _rowCount;
        public bool Vertical => _vertical;
        public bool DynamicItemSize => _dynamicItemSize;
        public float LeftPanelPadding => _leftPanelPadding;
        public float RightPanelPadding => _rightPanelPadding;
        public float TopPanelPadding => _topPanelPadding;
        public float BottomPanelPadding => _bottomPanelPadding;
        public float HorizontalItemPadding => _horizontalItemPadding;
        public float VerticalItemPadding => _verticalItemPadding;
        public float InitialWidth => _initialWidth;
        public float InitialHeight => _initialHeight;

        private void OnValidate()
        {
            if (MathUtils.IsFloatEquals(_initialWidth, 0f)) {
                if (!_dynamicItemSize || _vertical) {
                    Debug.LogWarning($"PooledScrollViewModel has zero value for {nameof(_initialWidth)}", transform);
                }
            }
            if (MathUtils.IsFloatEquals(_initialHeight, 0f)) {
                if (!_dynamicItemSize || !_vertical) {
                    Debug.LogWarning($"PooledScrollViewModel has zero value for {nameof(_initialHeight)}", transform);
                }
            }
        }
    }
}
