using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace CustomInspector
{
    [Conditional("UNITY_EDITOR")]
    public class Array2DAttribute : PropertyAttribute { }

    [Serializable]
    public class Array2D<T>
    {
        //-----Members----
        [MessageBox("Array2D is missing the [Array2D]-attribute to be correctly displayed.", MessageBoxType.Warning)]
        [SerializeField, Min(1)] int rowAmount = 3;
        public int RowAmount => rowAmount;
        [SerializeField, Min(1)] int columnAmount = 3;
        public int ColumnAmount => columnAmount;


        [SerializeField]
        Row[] rows;


        //----Methods----

        public T this[int rowIndex, int columnIndex]
        {
            get => GetElement(rowIndex: rowIndex, columnIndex: columnIndex);
            set => SetElement(value, rowIndex: rowIndex, columnIndex: columnIndex);
        }
        public T GetElement(int rowIndex, int columnIndex)
        {
            ThrowExceptionsIfInvalid(rowIndex: rowIndex, columnIndex: columnIndex);
            return rows[rowIndex].elements[columnIndex];
        }
        public void SetElement(T value, int rowIndex, int columnIndex)
        {
            ThrowExceptionsIfInvalid(rowIndex: rowIndex, columnIndex: columnIndex);
            rows[rowIndex].elements[columnIndex] = value;
        }
        void ThrowExceptionsIfInvalid(int rowIndex, int columnIndex)
        {
            if (rowIndex < 0)
                throw new ArgumentOutOfRangeException("rowIndex has to be positive");
            if (columnIndex < 0)
                throw new ArgumentOutOfRangeException("columnIndex has to be positive");
            if (rowIndex >= rowAmount)
                throw new ArgumentOutOfRangeException("rowIndex has to be lower than rowAmount");
            if (columnIndex >= columnAmount)
                throw new ArgumentOutOfRangeException("columnIndex has to be lower than columnAmount");
        }

        [Serializable]
        class Row
        {
            public T[] elements; //index chooses a column

            public Row(int elementAmount)
            {
                elements = new T[elementAmount];
            }
        }
        //---constructors----
        public Array2D() : this(rowAmount: 3, columnAmount: 3)
        { }
        public Array2D(int rowAmount, int columnAmount)
        {
            if (rowAmount < 1)
                Debug.LogError("Row-amount cannot be smaller than 1");
            else if (columnAmount < 1)
                Debug.LogError("Column-amount cannot be smaller than 1");
            else
            {
                this.rowAmount = rowAmount;
                this.columnAmount = columnAmount;
            }
            
            rows = new Row[rowAmount];
            for (int i = 0; i < rows.Length; i++)
            {
                rows[i] = new Row(columnAmount);
            }
        }
    }
}
