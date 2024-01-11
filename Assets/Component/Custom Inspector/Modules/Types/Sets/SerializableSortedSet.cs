using System.Collections.Generic;
using System;
using UnityEngine.Assertions;
using UnityEngine;
using System.Collections;

namespace CustomInspector
{
    /// <summary>
    /// A serializable implementation of System.SortedSet.
    /// values are sorted ascending (starts with smallest value).
    /// Time complexity: access = O(log(n)) , add/remove = O(n)
    /// </summary>
    [System.Serializable]
    public class SerializableSortedSet<T> : SerializableSet<T>, ICollection, ICollection<T>, IEnumerable, IEnumerable<T> where T : IComparable
    {
        /// <returns>If item was added(true) or was already in set(false)</returns>
        public override bool TryAdd(T item)
            => AddAndGetIndex(item) != -1;

        /// <summary>
        /// Adds an item and then returns the index of insertion.
        /// </summary>
        /// <returns>-1 if not inserted</returns>
        public int AddAndGetIndex(T item)
        {
            if (item == null)
                return -1;

            (int index, bool isSame) = GetTheoreticalIndex(item);

            if (isSame) //already in set
                return -1;

            values.Insert(index, item);
            return index;
        }
        /// <summary>
        /// true if the item was successfully removed; otherwise false. This method also returns false if the item was not found in the List<T>.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override bool Remove(T item)
            => RemoveAndGetIndex(item) != -1;
        /// <summary>
        /// Removes an item and then returns the index where it was before.
        /// </summary>
        /// <returns>-1 if not found</returns>
        public int RemoveAndGetIndex(T item)
        {
            (int index, bool isSame) = GetTheoreticalIndex(item);
            if (isSame)
            {
                RemoveAt(index);
                return index;
            }
            else
                return -1;
        }

        /// <returns>
        /// The index of the item.
        /// <para> -1 if the item doesnt exist </para>
        /// </returns>
        public override int GetIndexOf(T item)
        {
            (int index, bool isSame) = GetTheoreticalIndex(item);

            if (isSame)
                return index;
            else
                return -1;
        }

        /// <returns>All values>=min && values<=max. Sorted by ascending distance to t1</returns>
        public T[] GetItemsBetween(T t1, T t2)
        {
            if (t2.CompareTo(t1) > 0)
            {
                (int minIndex, _) = GetTheoreticalIndex(t1);
                (int maxIndex, bool isSame) = GetTheoreticalIndex(t2);
                if (!isSame)
                    maxIndex--;

                T[] newArray = new T[maxIndex - minIndex + 1];
                for (int i = minIndex; i <= maxIndex; i++)
                {
                    newArray[i - minIndex] = values[i];
                }
                return newArray;
            }
            else
            {
                (int minIndex, _) = GetTheoreticalIndex(t2);
                (int maxIndex, bool isSame) = GetTheoreticalIndex(t1);
                if (!isSame)
                    maxIndex--;

                T[] newArray = new T[(maxIndex + 1) - minIndex];
                for (int i = minIndex; i <= maxIndex; i++) //aber andersrum von max nach min
                {
                    newArray[maxIndex - i] = values[i];
                }
                return newArray;
            }
        }
        public override bool Contains(T item)
        {
            (_, bool isSame) = GetTheoreticalIndex(item);
            return isSame;
        }

        public IEnumerable<T> GetIEnumerable()
        => values;

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

        /// <summary>
        /// Nearest item that is not bigger. isSame==true when item is same(actually exist)
        /// </summary>
        (int index, bool isSame) GetTheoreticalIndex(T item)
        {
            if(item is null)
            {
                throw new ArgumentNullException("Item is null");
            }

            if (values.Count > 1)
                return FindItemBetween(0, values.Count - 1);
            else if (values.Count == 1)
            {
                return values[0].CompareTo(item) switch
                {
                    //list[0] smaller
                    < 0 => (1, false),
                    //list[0] equal
                    0 => (0, true),
                    //list[0] greater
                    > 0 => (0, false)
                };
            }
            else return (0, false);

            (int index, bool isSame) FindItemBetween(int min, int max)
            {
                Assert.IsFalse((min > max), "Max should be always greater than min.");

                if (min == max) //Abbruch bedingung - ab hier ist die mitte von beiden ja eh nur das niedrigere
                {
                    return values[min].CompareTo(item) switch
                    {
                        < 0 => (min + 1, false), //min smaller
                        0 => (min, true), //min equal
                        > 0 => (min, false), //min greater
                    };
                }
                if (min + 1 == max) //Abbruch bedingung - ab hier ist die mitte von beiden ja eh nur das niedrigere
                {
                    switch (values[min].CompareTo(item))
                    {
                        case < 0: //list[min] smaller
                            return values[max].CompareTo(item) switch
                            {
                                < 0 => (max + 1, false), //max smaller
                                0 => (max, true), //max equal
                                > 0 => (max, false), //max greater
                            };
                        case 0: //list[min] equal
                            return (min, true);
                        case > 0: //list[min] greater
                            return (min, false);
                    }
                }

                int middle = (min + max) / 2;
                return values[middle].CompareTo(item) switch
                {
                    < 0 => FindItemBetween(middle + 1, max), // list[middle] < item
                    0 => (middle, true), // list[middle] == item
                    > 0 => FindItemBetween(min, middle - 1), // list[middle] > item
                };
            }
        }


#if UNITY_EDITOR

        /// <summary>
        /// This function should fix all inputs from the user in the inspector when the default property drawer will be overwritten
        /// </summary>
        protected void CheckUserInputs()
        {
            for (int i = 0; i < values.Count - 1; i++)
            {
                if (values[i].CompareTo(values[i + 1]) >= 0)
                {
                    Debug.LogException(new DataMisalignedException("values in SerializableSortedSet have to be ascending. You have to press Clear()"));
                }
            }
        }

        void ICollection.CopyTo(Array array, int index)
        {
            if (index + Count > array.Length)
                throw new ArgumentException("The number of elements in the source Dictionary is greater than the available space from index to the end of the destination array");

            for (int i = index + Count - 1; i >= index; i--)
            {
                array.SetValue(values[i], i);
            }
        }

        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
        {
            if (arrayIndex + Count > array.Length)
                throw new ArgumentException("The number of elements in the source Dictionary is greater than the available space from index to the end of the destination array");

            for (int i = arrayIndex + Count - 1; i >= arrayIndex; i--)
            {
                array[i] = values[i];
            }
        }
#endif

    }
}