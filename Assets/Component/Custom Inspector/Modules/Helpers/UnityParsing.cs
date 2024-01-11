using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CustomInspector.Extensions
{    
    public static class Conversions
    {
        public static Quaternion ToQuaternion(this Vector4 v4)
        {
            return new Quaternion(v4.x, v4.y, v4.z, v4.z);
        }
        public static bool IsUnityNull(this object obj)
        {
            try
            {
                if (obj?.Equals(null) ?? true) //'Equals()' because unity objects can be only checked for null in their types
                    return true;
            }
            catch (NullReferenceException) //this catches if the user makes their own equals method, but dont care for unitys null-notNull objects
            {
                return true;
            }
            catch (Exception e) //if user throws any other exceptions in their custom 'equals'-implementation
            {
                Debug.LogException(e);
                return true;
            }

            return false;
        }
    }
    public static class UnityParsing //Inverse of the ToString()
    {
        public static bool IsColor(string s, out Color parsedColor)
        {
            try
            {
                parsedColor = ParseColor(s);
                return true;
            }
            catch(Exception)
            {
                parsedColor = Color.red;
                return false;
            }
        }
        public static Color ParseColor(string s)
        {
            Vector4 v4 = ParseVector4(s);
            return new Color(v4.x, v4.y, v4.z, v4.w);
        }
        public static Vector2Int ParseVector2Int(string s) //Format: (1, 1)
        {
            List<int> components = GetNumbers(s).Select(_ => ParseIntRounded(_)).ToList();
            if (components.Count != 2)
                throw new ArgumentException($"string has {components.Count} instead of 2 components");

            return new Vector2Int(components[0], components[1]);
        }
        public static Vector2 ParseVector2(string s) //Format: (1.1, 1)
        {
            List<float> components = GetNumbers(s).Select(_ => float.Parse(_, NumberStyles.Float, CultureInfo.InvariantCulture)).ToList();
            if (components.Count != 2)
                throw new ArgumentException($"string has {components.Count} instead of 3 components");

            return new Vector2(components[0], components[1]);
        }
        public static Vector3Int ParseVector3Int(string s) //Format: (1, 1, 1)
        {
            List<int> components = GetNumbers(s).Select(_ => ParseIntRounded(_)).ToList();
            if (components.Count != 3)
                throw new ArgumentException($"string has {components.Count} instead of 3 components");

            return new Vector3Int(components[0], components[1], components[2]);
        }
        public static Vector3 ParseVector3(string s) //ToString format: (1.1, 1, 1) | unity clipboard format: Vector3(-0.5,-1.20000005,-0.0614999533)
        {
            List<float> components = GetNumbers(s).Select(_ => float.Parse(_, NumberStyles.Float, CultureInfo.InvariantCulture)).ToList();
            if (components.Count != 3)
                throw new ArgumentException($"string has {components.Count} instead of 3 components");

            return new Vector3(components[0], components[1], components[2]);
        }
        public static Vector4 ParseVector4(string s)  //Format: (1.1, 1, 0, 1)
        {
            List<float> components = GetNumbers(s).Select(_ => float.Parse(_, NumberStyles.Float, CultureInfo.InvariantCulture)).ToList();
            if (components.Count != 4)
                throw new ArgumentException($"string has {components.Count} instead of 4 components");

            return new Vector4(components[0], components[1], components[2], components[3]);
        }
        /// <summary>
        /// All numbers after first open bracket(if exists).
        /// Also accepts 'Vector3(4, -5f, 77)'
        /// <para>e.g.: 678(hallo123(7.5aa8 -> 123e-10, 7.5, 8</para>
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        static List<string> GetNumbers(string s)
        {
            List<string> nums = new();
            int start = s.IndexOf('(');
            if (start == -1) //dont care if no bracket exist
                start = 0;
            int i;
            for (i = start; i < s.Length; i++)
            {
                if (char.IsNumber(s[i]) || s[i] == '.' || s[i] == '-' || s[i] == 'e') // 10e-5 is valid
                {
                    continue;
                }

                if (start < i)
                    nums.Add(s[start..i]);
                start = i + 1;
            }
            if (start < s.Length) //no closing bracket
            {
                nums.Add(s[start..i]);
            }
            return nums;
        }
        static int ParseIntRounded(string floatStr)
        {
            for (int i = 0; i < floatStr.Length; i++)
            {
                if(!char.IsNumber(floatStr[i]))
                {
                    int @int = int.Parse(floatStr[..i]);
                    if(i + 1 < floatStr.Length && floatStr[i] == '.')
                    {
                        return (floatStr[i + 1] >= '5') ? @int + 1 : @int;
                    }
                    else return @int;
                }
            }
            return int.Parse(floatStr);
        }
        public static RectInt ParseRectInt(string s)  //Format: (x:0, y:0, width:0, height:0)
        {
            int[] iVal = new int[4];
            int start = 0;
            int end;
            for (int i = 0; i < 4; i++)
            {
                while (s[start] != ':') start++;
                start++;
                end = start;
                while (s[end] != ',') end++;
                iVal[i] = ParseIntRounded(s[start..end]);
            }
            return new RectInt(iVal[0], iVal[1], iVal[2], iVal[3]);
        }
        public static Rect ParseRect(string s)
        {
            float[] fVal = new float[4];
            int start = 0;
            int end;
            for (int i = 0; i < 4; i++)
            {
                while (s[start] != ':') start++;
                start++;
                end = start;
                while (s[end] != ',') end++;
                fVal[i] = float.Parse(s[start..end], NumberStyles.Float, CultureInfo.InvariantCulture);
            }
            return new Rect(fVal[0], fVal[1], fVal[2], fVal[3]);
        }
        public static BoundsInt ParseBoundsInt(string s) //Format: Position: (0, 0, 0), Size: (0, 0, 0)
        {
            string firstVector = s[(s.IndexOf('(') + 1)..s.IndexOf(')')];
            string secoundVector = s[(s.LastIndexOf('(') + 1)..s.LastIndexOf(')')];
            return new BoundsInt(ParseVector3Int(firstVector), ParseVector3Int(secoundVector));
        }
        public static Bounds ParseBounds(string s) //Format: Position: (0.00, 0.00, 0.00), Size: (0.00, 0.00, 0.00)
        {
            string firstVector = s[(s.IndexOf('(') + 1)..s.IndexOf(')')];
            string secoundVector = s[(s.LastIndexOf('(') + 1)..s.LastIndexOf(')')];
            return new Bounds(ParseVector3(firstVector), ParseVector3(secoundVector));
        }
        public static Quaternion ParseQuaternion(string s) //Format (0.50000, 0.50000, 0.50000, 0.50000)
        {
            Vector4 v4 = ParseVector4(s);
            return new Quaternion(v4.x, v4.y, v4.z, v4.w);
        }
    }
}