using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ES.Common.Helpers
{
    public class ObservableMruCollection<T> : ObservableCollection<T>
    {
        #region Fields

        private readonly IEqualityComparer<T> _itemComparer;
        private readonly int _maxSize = -1;
        private int _originSize = -1;

        #endregion

        #region Constructors

        public ObservableMruCollection(int maxSize)
        {
            _maxSize = maxSize;
        }

        public ObservableMruCollection(IEnumerable<T> collection, int maxSize)
            : base(collection)
        {
            _maxSize = maxSize;
        }

        public ObservableMruCollection(List<T> list, int maxSize)
            : base(list)
        {
            _maxSize = maxSize;
        }

        #endregion

        #region Properties

        public int MaxSize
        {
            get { return _maxSize; }
        }

        #endregion

        #region Public Methods

        public new void Add(T item)
        {
            var indexOfMatch = IndexOf(item);
            if (indexOfMatch < 0)
            {
                Insert(0, item);
            }
            else if (indexOfMatch != 0) // ADDED CHECK TO AVOID INVALIDARGUMENT EXCEPTION FROM GRAPHEDITOR
            {
                Move(indexOfMatch, 0);
            }

            RemoveOverflow();
        }

        public void AddItem(T item, int originalListFirstIndex)
        {
            ++_originSize;
            var firstItem = item as IComparable;
            if (firstItem == null) return;
            for (int i = originalListFirstIndex; i < Count; ++i)
            {
                if (firstItem.CompareTo(Items[i]) >= 0) continue;
                Insert(i, item);
                break;
            }
        }

        public new bool Contains(T item)
        {
            return this.Contains(item, _itemComparer);
        }

        public new int IndexOf(T item)
        {
            int indexOfMatch = -1;

            if (_itemComparer != null)
            {
                for (int idx = 0; idx < Count - _originSize; idx++)
                {
                    if (_itemComparer.Equals(item, this[idx]))
                    {
                        indexOfMatch = idx;
                        break;
                    }
                }
            }
            else
            {
                indexOfMatch = base.IndexOf(item);
            }

            return indexOfMatch;
        }

        public new bool Remove(T item)
        {
            bool opResult = false;

            int targetIndex = IndexOf(item);
            if (targetIndex > -1)
            {
                RemoveAt(targetIndex);
                opResult = true;
            }

            return opResult;
        }

        #endregion

        #region Helper Methods

        private void RemoveOverflow()
        {
            if (MaxSize > 0)
            {
                var recentSize = Count - _originSize;
                while (recentSize-- > MaxSize)
                    RemoveAt(recentSize);
            }
        }

        #endregion
    }
}
