using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomInspector.Extensions
{
    public static class Exceptions
    {
        /// <summary>
        /// If system.Reflections members have unexpected types
        /// </summary>
        public class WrongTypeException : Exception
        {
            public WrongTypeException() : base() { }
            public WrongTypeException(string message) : base(message) { }
            public WrongTypeException(string message, Exception inner) : base(message, inner) { }
        }
        ///<summary>if my custom unity parsing fails</summary>
        public class ParseException : Exception
        {
            public ParseException() : base() { }
            public ParseException(string message) : base(message) { }
            public ParseException(string message, Exception inner) : base(message, inner) { }
        }
    }
}