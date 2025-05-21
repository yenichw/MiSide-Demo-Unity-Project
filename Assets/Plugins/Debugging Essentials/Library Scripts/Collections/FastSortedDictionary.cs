using System;
using System.Collections.Generic;

namespace DebuggingEssentials
{
    public enum CompareMode { Key, Value };

    public struct ItemHolder<T, U> : IComparable<ItemHolder<T, U>> where T : IComparable<T> where U : IComparable<U>
    {
        public static CompareMode compareMode;
        public T key;
        public U value;

        public ItemHolder(T key, U value)
        {
            this.key = key;
            this.value = value;
        }

        public int CompareTo(ItemHolder<T, U> other)
        {
            if (compareMode == CompareMode.Key) return key.CompareTo(other.key);
            else return value.CompareTo(other.value);
        }
    }

    public class FastSortedDictionary<T, U> where T : IComparable<T> where U : IComparable<U>
    {
        public Dictionary<T, U> lookup;
        public FastList<ItemHolder<T, U>> list;

        public FastSortedDictionary(int capacity)
        {
            lookup = new Dictionary<T, U>();
            list = new FastList<ItemHolder<T, U>>(capacity);
        }

        public void Add(T key, U value)
        {
            lookup.Add(key, value);
            list.Add(new ItemHolder<T, U>(key, value));
        }

        public void Sort(CompareMode compareMode)
        {
            ItemHolder<T, U>.compareMode = compareMode;
            Array.Sort(list.items, 0, list.Count);
        }

        public void Clear()
        {
            lookup.Clear();
            list.Clear();
        }
    }
}