using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace DebuggingEssentials
{
    public class FastQueue<T> : FastListBase
    {
        protected int head; // First element in the queue
        protected int tail; // Last element in the queue

        protected T[] items;

        public FastQueue(int capacity)
        {
            if (capacity < 1) capacity = 1;

            items = new T[capacity];
        }

        protected void DoubleCapacity()
        {
            int arraySize = items.Length;
            T[] newItems = new T[arraySize * 2];

            if (_count > 0)
            {
                if (head < tail)
                {
                    Array.Copy(items, head, newItems, 0, _count);
                }
                else
                {
                    Array.Copy(items, head, newItems, 0, arraySize - head);
                    Array.Copy(items, 0, newItems, arraySize - head, tail);
                }
            }

            items = newItems;
            head = 0;
            tail = _count;
        }

        public int ArrayLength() { return items.Length; }

        public virtual void Clear()
        {
            if (head < tail)
            {
                Array.Clear(items, head, _count);
            }
            else
            {
                Array.Clear(items, head, items.Length - head);
                Array.Clear(items, 0, tail);
            }

            tail = head = 0;
            Count = _count = 0;
        }

        public virtual void FastClear()
        {
            tail = head = 0;
            Count = _count = 0;
        }

        public virtual void Enqueue(T item)
        {
            if (_count == items.Length)
            {
                DoubleCapacity();
                // Debug.LogError("FastQueue is full! Count " + _count + " capacity " + arraySize);
                // return;
            }

            items[tail] = item;
            tail = (tail + 1) % items.Length;
            Count = ++_count;
        }

        public void EnqueueRange(T[] items, int startIndex = 0, int count = -1)
        {
            if (count == -1) count = items.Length;
            int endIndex = startIndex + count;

            for (int i = startIndex; i < endIndex; i++)
            {
                Enqueue(items[i]);
            }
        }

        public T Peek(int index)
        {
            if (index >= _count)
            {
                Debug.LogError("index " + index + " is bigger than Count " + _count);
                return default(T);
            }

            return items[(head + index) % items.Length];
        }

        public virtual T Dequeue()
        {
            if (_count == 0) { Debug.LogError("FastQueue is empty!"); return default(T); }

            T item = items[head];
            items[head] = default(T);
            head = (head + 1) % items.Length;
            Count = --_count;

            return item;
        }

        public virtual void Dequeue(T item)
        {
            if (_count == 0) { Debug.LogError("FastQueue is empty!"); return; }

            int index = -1;
            int i;

            for (i = 0; i < _count; i++)
            {
                index = (i + head) % items.Length;
                if (items[index].Equals(item)) break;
            }

            if (i == _count) { Debug.LogError("Item not found in FastQueue"); return; }

            // Debug.Log("Found " + i + " -> " + items[index]);

            Count = --_count;

            for (int j = i; j >= 1; j--)
            {
                index = (j + head) % items.Length;
                int previousIndex = (j + (head - 1)) % items.Length;
                items[index] = items[previousIndex];
            }

            items[head] = default(T);
            // Debug.Log(startIndex + " " + items[startIndex]);
            head = (head + 1) % items.Length;
        }

        public void Report()
        {
            string text = "";
            for (int i = 0; i < _count; i++)
            {
                int index = (i + head) % items.Length;
                text += items[index].ToString() + ", ";
            }

            string text2 = "";
            for (int i = 0; i < items.Length; i++)
            {
                text2 += items[i].ToString() + ", ";
            }

            Debug.Log("==============================================");
            Debug.Log(text);
            Debug.Log(text2);
            Debug.Log("Tail " + tail + " Head " + head + " Count " + Count + "(" + _count + ")");
            Debug.Log("==============================================");
        }
    }

    public class FastIntQueue : FastQueue<int>
    {
        public FastIntQueue(int capacity) : base(capacity) { }

        public void EnqueueRange(int startIndex, int endIndex)
        {
            for (int i = startIndex; i < endIndex; i++) Enqueue(i);
        }

        public void ClearAndEnqueueRange(int startIndex, int endIndex)
        {
            FastClear();
            EnqueueRange(startIndex, endIndex);
        }

        public override void Dequeue(int item)
        {
            // if (arraySize == 128) Debug.Log("Dequeue " + item + " count " + _count);

            if (_count == 0) { Debug.LogError("FastQueue is empty!"); return; }

            int index = -1;
            int i;

            for (i = 0; i < _count; i++)
            {
                index = (i + head) % items.Length;
                if (items[index] == item) break;
            }

            if (i == _count) { Debug.LogError("Item not found in FastQueue"); return; }

            // Debug.Log("Found " + i + " -> " + items[index]);

            Count = --_count;

            for (int j = i; j >= 1; j--)
            {
                index = (j + head) % items.Length;
                int previousIndex = (j + (head - 1)) % items.Length;
                items[index] = items[previousIndex];
            }

            items[head] = 0;
            // Debug.Log(startIndex + " " + items[startIndex]);
            head = (head + 1) % items.Length;
        }
    }
}