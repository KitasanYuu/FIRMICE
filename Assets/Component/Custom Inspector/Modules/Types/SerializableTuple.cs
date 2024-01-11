using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomInspector
{
    public class TupleAttribute : PropertyAttribute { }

    [System.Serializable]
    public class SerializableTuple<T1, T2>
    {
        [HorizontalGroup(true)]
        [LabelSettings(LabelStyle.NoLabel)]
        public T1 item1;

        [HorizontalGroup]
        [LabelSettings(LabelStyle.NoLabel)]
        public T2 item2;

        public SerializableTuple() { }
        public SerializableTuple(T1 item1, T2 item2)
        {
            this.item1 = item1;
            this.item2 = item2;
        }
    }
    [System.Serializable]
    public class SerializableTuple<T1, T2, T3>
    {
        [HorizontalGroup(true)]
        [LabelSettings(LabelStyle.NoLabel)]
        public T1 item1;

        [HorizontalGroup]
        [LabelSettings(LabelStyle.NoLabel)]
        public T2 item2;

        [HorizontalGroup]
        [LabelSettings(LabelStyle.NoLabel)]
        public T3 item3;

        public SerializableTuple() { }
        public SerializableTuple(T1 item1, T2 item2, T3 item3)
        {
            this.item1 = item1;
            this.item2 = item2;
            this.item3 = item3;
        }
    }
    [System.Serializable]
    public class SerializableTuple<T1, T2, T3, T4>
    {
        [HorizontalGroup(true)]
        [LabelSettings(LabelStyle.NoLabel)]
        public T1 item1;

        [HorizontalGroup]
        [LabelSettings(LabelStyle.NoLabel)]
        public T2 item2;

        [HorizontalGroup]
        [LabelSettings(LabelStyle.NoLabel)]
        public T3 item3;

        [HorizontalGroup]
        [LabelSettings(LabelStyle.NoLabel)]
        public T4 item4;

        public SerializableTuple() { }
        public SerializableTuple(T1 item1, T2 item2, T3 item3, T4 item4)
        {
            this.item1 = item1;
            this.item2 = item2;
            this.item3 = item3;
            this.item4 = item4;
        }
    }
    [System.Serializable]
    public class SerializableTuple<T1, T2, T3, T4, T5>
    {
        [HorizontalGroup(true)]
        [LabelSettings(LabelStyle.NoLabel)]
        public T1 item1;

        [HorizontalGroup]
        [LabelSettings(LabelStyle.NoLabel)]
        public T2 item2;

        [HorizontalGroup]
        [LabelSettings(LabelStyle.NoLabel)]
        public T3 item3;

        [HorizontalGroup]
        [LabelSettings(LabelStyle.NoLabel)]
        public T4 item4;

        [HorizontalGroup]
        [LabelSettings(LabelStyle.NoLabel)]
        public T5 item5;

        public SerializableTuple() { }
        public SerializableTuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5)
        {
            this.item1 = item1;
            this.item2 = item2;
            this.item3 = item3;
            this.item4 = item4;
            this.item5 = item5;
        }
    }
    [System.Serializable]
    public class SerializableTuple<T1, T2, T3, T4, T5, T6>
    {
        [HorizontalGroup(true)]
        [LabelSettings(LabelStyle.NoLabel)]
        public T1 item1;

        [HorizontalGroup]
        [LabelSettings(LabelStyle.NoLabel)]
        public T2 item2;

        [HorizontalGroup]
        [LabelSettings(LabelStyle.NoLabel)]
        public T3 item3;

        [HorizontalGroup]
        [LabelSettings(LabelStyle.NoLabel)]
        public T4 item4;

        [HorizontalGroup]
        [LabelSettings(LabelStyle.NoLabel)]
        public T5 item5;

        [HorizontalGroup]
        [LabelSettings(LabelStyle.NoLabel)]
        public T6 item6;

        public SerializableTuple() { }
        public SerializableTuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6)
        {
            this.item1 = item1;
            this.item2 = item2;
            this.item3 = item3;
            this.item4 = item4;
            this.item5 = item5;
            this.item6 = item6;
        }
    }
}
