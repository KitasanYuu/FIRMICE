using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace CustomInspector
{
    [System.Serializable]
    public class SerializableDateTime
    {
        public enum InspectorFormat
        {
            Default,
            AddTextInput,
            TextInput,
            DateEnums,
        }

        [Min(1), Max(9999), Hook(nameof(ClampDayValue))]
        [SerializeField] public int year;
        [Min(1), Max(12), Hook(nameof(ClampDayValue))]
        [SerializeField] public int month;
        [Min(1), Max(31), Hook(nameof(ClampDayValue))]
        [SerializeField] public int day;
        [Min(0), Max(23)]
        [SerializeField] public int hour;
        [Min(0), Max(59)]
        [SerializeField] public int minute;
        [Min(0), Max(59)]
        [SerializeField] public int second;

        [SerializeField] public DateTimeKind kind;

        public int Year
        {
            get { return year; }
            set { year = Mathf.Clamp(value, 1, 9999); }
        }
        public int Month
        {
            get { return month; }
            set { month = Mathf.Clamp(value, 1, 12); }
        }
        public int Day
        {
            get { return day; }
            set { day = value; ClampDayValue(); }
        }
        void ClampDayValue() => day = Mathf.Clamp(day, 1, DateTime.DaysInMonth(Year, Month));
        public int Hour
        {
            get { return hour; }
            set { hour = Mathf.Clamp(value, 0, 23); }
        }
        public int Minute
        {
            get { return minute; }
            set { minute = Mathf.Clamp(value, 0, 59); }
        }
        public int Second
        {
            get { return second; }
            set { second = Mathf.Clamp(value, 0, 59); }
        }
        public DateTimeKind Kind
        {
            get { return kind; }
            set { kind = value; }
        }

        public SerializableDateTime() : this(DateTime.MinValue)
        { }
        public SerializableDateTime(DateTime dateTime)
        {
            year = dateTime.Year;
            month = dateTime.Month;
            day = dateTime.Day;
            hour = dateTime.Hour;
            minute = dateTime.Minute;
            second = dateTime.Second;
            //millisecond = dateTime.Millisecond;
            kind = dateTime.Kind;
        }

        public static implicit operator SerializableDateTime(DateTime dateTime) => new(dateTime);

        public static explicit operator DateTime(SerializableDateTime dateTime)
        => new DateTime(dateTime.year, dateTime.month, dateTime.day, dateTime.hour, dateTime.minute, dateTime.second, dateTime.kind);
    }

    [Conditional("UNITY_EDITOR")]
    public class SerializableDateTimeAttribute : PropertyAttribute
    {
        public readonly SerializableDateTime.InspectorFormat format;
        public SerializableDateTimeAttribute(SerializableDateTime.InspectorFormat format)
        {
            this.format = format;
        }
    }
}
