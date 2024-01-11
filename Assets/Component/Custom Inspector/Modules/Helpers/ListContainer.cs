using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CustomInspector.Extensions
{
    /// <summary>
    /// This is a class that looks like a list, but the actual list is inside. This if for custom property drawers to get a list without attributes
    /// </summary>
    [System.Serializable]
    public class ListContainer<T> : IEnumerable, IEnumerable<T>, ICollection<T>, IList, IList<T>
    {
        [SerializeField]
        private List<T> list;

        public static implicit operator List<T>(ListContainer<T> container)
        {
            return container.list;
        }
        public static implicit operator ListContainer<T>(List<T> list)
        {
            return new ListContainer<T>(list);
        }

        public ListContainer(List<T> list)
        {
            this.list = list;
        }

        // ----------------------------------------------------------
        // -----------all code below here was generated--------------
        // ----------------------------------------------------------

        /*  
         *  Here are all list methods forwarded to the outside
         *  + 2 constructors
         *  + IEnumerable and IList interface implementation
         *  
         */

        public ListContainer()
        {
            list = new List<T>();
        }

        public ListContainer(IEnumerable<T> collection)
        {
            list = new List<T>(collection);
        }

        public T this[int index]
        {
            get => list[index];
            set => list[index] = value;
        }

        public int Count => list.Count;

        public int Capacity => list.Capacity;

        public bool IsReadOnly => false;

        public bool IsFixedSize => false;

        public bool IsSynchronized => false;

        public object SyncRoot
        {
            get
            {
                if (syncRoot == null)
                    syncRoot = new object();
                return syncRoot;
            }
            set => syncRoot = value; }

        private object syncRoot;

        object IList.this[int index] { get => this[index]; set => this[index] = (T)value; }

        public void Add(T item) => list.Add(item);

        public void Clear() => list.Clear();

        public bool Contains(T item) => list.Contains(item);

        public void CopyTo(T[] array, int arrayIndex) => list.CopyTo(array, arrayIndex);

        public IEnumerator<T> GetEnumerator() => list.GetEnumerator();

        public int IndexOf(T item) => list.IndexOf(item);

        public void Insert(int index, T item) => list.Insert(index, item);

        public bool Remove(T item) => list.Remove(item);

        public void RemoveAt(int index) => list.RemoveAt(index);

        IEnumerator IEnumerable.GetEnumerator() => list.GetEnumerator();

        public int BinarySearch(T item) => list.BinarySearch(item);

        public int BinarySearch(T item, IComparer<T> comparer) => list.BinarySearch(item, comparer);

        public int BinarySearch(int index, int count, T item, IComparer<T> comparer) => list.BinarySearch(index, count, item, comparer);

        public void CopyTo(int index, T[] array, int arrayIndex, int count) => list.CopyTo(index, array, arrayIndex, count);

        public bool Exists(Predicate<T> match) => list.Exists(match);

        public T Find(Predicate<T> match) => list.Find(match);

        public List<T> FindAll(Predicate<T> match) => list.FindAll(match);

        public int FindIndex(Predicate<T> match) => list.FindIndex(match);

        public int FindIndex(int startIndex, Predicate<T> match) => list.FindIndex(startIndex, match);

        public int FindIndex(int startIndex, int count, Predicate<T> match) => list.FindIndex(startIndex, count, match);

        public T FindLast(Predicate<T> match) => list.FindLast(match);

        public int FindLastIndex(Predicate<T> match) => list.FindLastIndex(match);

        public int FindLastIndex(int startIndex, Predicate<T> match) => list.FindLastIndex(startIndex, match);

        public int FindLastIndex(int startIndex, int count, Predicate<T> match) => list.FindLastIndex(startIndex, count, match);

        public void ForEach(Action<T> action) => list.ForEach(action);

        public List<T> GetRange(int index, int count) => list.GetRange(index, count);

        public int IndexOf(T item, int index) => list.IndexOf(item, index);

        public int IndexOf(T item, int index, int count) => list.IndexOf(item, index, count);

        public void InsertRange(int index, IEnumerable<T> collection) => list.InsertRange(index, collection);

        public int LastIndexOf(T item) => list.LastIndexOf(item);

        public void RemoveAll(Predicate<T> match) => list.RemoveAll(match);

        public void Reverse() => list.Reverse();

        public void Reverse(int index, int count) => list.Reverse(index, count);

        public void Sort() => list.Sort();

        public void Sort(IComparer<T> comparer) => list.Sort(comparer);

        public void Sort(Comparison<T> comparison) => list.Sort(comparison);

        public void Sort(int index, int count, IComparer<T> comparer) => list.Sort(index, count, comparer);

        public T[] ToArray() => list.ToArray();

        public void TrimExcess() => list.TrimExcess();

        public bool TrueForAll(Predicate<T> match) => list.TrueForAll(match);

        public int Add(object value)
        {
            Add((T)value);
            return Count - 1;
        }

        public bool Contains(object value) => list.Contains((T)value);

        public int IndexOf(object value) => list.IndexOf((T)value);

        public void Insert(int index, object value) => list.Insert(index, (T)value);

        public void Remove(object value) => Remove((T)value);

        public void CopyTo(Array array, int index) => list.CopyTo(array.Cast<T>().ToArray(), index);
    }
}