using System;
using JetBrains.Annotations;
using UnityEngine;

namespace LiteUI.Common.Utils
{
    [PublicAPI, Serializable]
    public class BitFieldEnum<T>
            where T : IComparable, IFormattable, IConvertible
    {
        [SerializeField]
        private int _bitField;

        public BitFieldEnum()
        {
            if (!typeof(T).IsEnum) {
                throw new ArgumentException("T must be an enumerated type");
            }
        }

        public bool IsSet(T index)
        {
            int enumIndex = index.ToInt32(null);
            return ((_bitField >> enumIndex) & 1) == 1;
        }

        public void SetBit(T index, bool value)
        {
            int enumIndex = index.ToInt32(null);
            if (value) {
                _bitField |= 1 << enumIndex;
            } else {
                _bitField &= ~(1 << enumIndex);
            }
        }

        public int RawValue
        {
            get => _bitField;
            set => _bitField = value;
        }
    }
}
