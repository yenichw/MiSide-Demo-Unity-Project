using System;
using System.Collections.Generic;

namespace DebuggingEssentials
{
    public class FastSortedHashSet<T> where T : IComparable<T>
    {
        public HashSet<T> lookup;
        public FastList<T> list;

        public FastSortedHashSet(int capacity)
        {
            lookup = new HashSet<T>();
            list = new FastList<T>(capacity);
        }

        public void Add(T item)
        {
            lookup.Add(item);
            list.Add(item);
        }

        public void Sort()
        {
            Array.Sort(list.items, 0, list.Count);
        }

        public void Clear()
        {
            lookup.Clear();
            list.Clear();
        }
    }

    public class FastSortedHashSet<T, U> where U : IComparable<U>
    {
        public HashSet<T> lookup;
        public FastList<U> list;

        public FastSortedHashSet(int capacity)
        {
            lookup = new HashSet<T>();
            list = new FastList<U>(capacity);
        }

        public void Add(T lookupItem, U listItem)
        {
            lookup.Add(lookupItem);
            list.Add(listItem);
        }

        public void Sort()
        {
            Array.Sort(list.items, 0, list.Count);
        }

        public void Clear()
        {
            lookup.Clear();
            list.Clear();
        }
    }
}