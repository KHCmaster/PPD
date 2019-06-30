using System;
using System.Collections.Generic;

namespace PPDMulti.Data
{
    delegate void ItemChangedEventHandler<T>(T[] addedItems, T[] removedItems);
    class ChangableList<T> : List<T>
    {
        public event ItemChangedEventHandler<T> ItemChanged;

        public ChangableList()
        {
        }

        public ChangableList(IEnumerable<T> collection)
            : base(collection)
        {

        }

        public new void Add(T item)
        {
            base.Add(item);
            OnItemChanged(new T[] { item }, new T[0]);
        }

        public new void AddRange(IEnumerable<T> collection)
        {
            var items = new List<T>(collection);
            base.AddRange(collection);

            if (items.Count > 0)
            {
                OnItemChanged(items.ToArray(), new T[0]);
            }
        }

        public new void Insert(int index, T item)
        {
            base.Insert(index, item);
            OnItemChanged(new T[] { item }, new T[0]);
        }

        public new void InsertRange(int index, IEnumerable<T> collection)
        {
            var items = new List<T>(collection);
            base.InsertRange(index, collection);

            if (items.Count > 0)
            {
                OnItemChanged(items.ToArray(), new T[0]);
            }
        }

        public new bool Remove(T item)
        {
            var ret = base.Remove(item);

            if (ret)
            {
                OnItemChanged(new T[0], new T[] { item });
            }

            return ret;
        }

        public new int RemoveAll(Predicate<T> match)
        {
            var before = this.ToArray();
            var ret = base.RemoveAll(match);

            if (ret > 0)
            {
                var removedList = new List<T>();
                int iter = 0;
                for (int i = 0; i < before.Length; i++)
                {
                    if (before[i].Equals(this[iter]))
                    {
                        iter++;
                    }
                    else
                    {
                        removedList.Add(before[i]);
                    }
                }

                if (removedList.Count > 0)
                {
                    OnItemChanged(new T[0], removedList.ToArray());
                }
            }

            return ret;
        }

        public new void RemoveAt(int index)
        {
            T item = this[index];
            base.RemoveAt(index);
            OnItemChanged(new T[0], new T[] { item });
        }

        public new void RemoveRange(int index, int count)
        {
            var items = GetRange(index, count).ToArray();
            base.RemoveRange(index, count);

            if (items.Length > 0)
            {
                OnItemChanged(new T[0], items);
            }
        }

        public new void Clear()
        {
            var items = this.ToArray();
            base.Clear();

            if (items.Length > 0)
            {
                OnItemChanged(new T[0], items);
            }
        }

        protected void OnItemChanged(T[] addedItems, T[] removedItems)
        {
            if (ItemChanged != null)
            {
                ItemChanged.Invoke(addedItems, removedItems);
            }
        }
    }
}
