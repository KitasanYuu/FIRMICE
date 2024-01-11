using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using CustomInspector.Helpers;

namespace CustomInspector.Extensions
{
    public static class PropertyConversions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns>removes underscores, starts uppercase, adds spaces</returns>
        public static string NameFormat(string name)
        {
            if (name is null)
                throw new ArgumentException("Null-String cannot be formatted");
            if (name == "")
                return "";

            //remove underscore start
            if (name[0] == '_')
                name = name[1..];
            if (name == "") //contained only the underscore
                return " "; //space so that label is still shown but only empty

            //first character always uppercase
            string res = char.ToUpper(name[0]).ToString();
            //add remaining but insert space before uppercases
            for (int i = 1; i < name.Length; i++)
            {
                if (char.IsUpper(name[i]) && (i <= 0 || name[i-1] != ' ')) //if uppercase letter and not already inserted
                    res += " " + name[i];  //to lowercase would be: res += " " + (char)(name[i] + ('a' - 'A'));
                else
                    res += name[i];
            }
            return res;
        }
        public static System.Type ToSystemType(this SerializedPropertyType propertyType)
        {
            return propertyType switch
            {
                SerializedPropertyType.Integer => typeof(int),
                SerializedPropertyType.Boolean => typeof(bool),
                SerializedPropertyType.Float => typeof(float),
                SerializedPropertyType.String => typeof(string),
                SerializedPropertyType.Character => typeof(char),
                SerializedPropertyType.Color => typeof(Color),
                SerializedPropertyType.Enum => typeof(Enum),
                SerializedPropertyType.Vector2Int => typeof(Vector2Int),
                SerializedPropertyType.Vector2 => typeof(Vector2),
                SerializedPropertyType.Vector3Int => typeof(Vector3Int),
                SerializedPropertyType.Vector3 => typeof(Vector3),
                SerializedPropertyType.Vector4 => typeof(Vector4),
                SerializedPropertyType.RectInt => typeof(RectInt),
                SerializedPropertyType.Rect => typeof(Rect),
                SerializedPropertyType.LayerMask => typeof(LayerMask),
                SerializedPropertyType.AnimationCurve => typeof(AnimationCurve),
                SerializedPropertyType.BoundsInt => typeof(BoundsInt),
                SerializedPropertyType.Bounds => typeof(Bounds),
                SerializedPropertyType.Quaternion => typeof(Quaternion),
                SerializedPropertyType.ExposedReference => typeof(UnityEngine.Object),
                SerializedPropertyType.ObjectReference => typeof(UnityEngine.Object),
                _ => null
            };
        }
        public static SerializedPropertyType ToPropertyType(this System.Type type)
        {
            if (type == null)
                throw new NullReferenceException($"Type conversion failed");
            else if (type == typeof(int)) return SerializedPropertyType.Integer;
            else if (type == typeof(bool)) return SerializedPropertyType.Boolean;
            else if (type == typeof(float)) return SerializedPropertyType.Float;
            else if (type == typeof(double)) return SerializedPropertyType.Float;
            else if (type == typeof(string)) return SerializedPropertyType.String;
            else if (type == typeof(char)) return SerializedPropertyType.Character;
            else if (type == typeof(Color)) return SerializedPropertyType.Color;
            else if (type == typeof(Enum)) return SerializedPropertyType.Enum;
            else if (type == typeof(Vector2Int)) return SerializedPropertyType.Vector2Int;
            else if (type == typeof(Vector2)) return SerializedPropertyType.Vector2;
            else if (type == typeof(Vector3Int)) return SerializedPropertyType.Vector3Int;
            else if (type == typeof(Vector4)) return SerializedPropertyType.Vector4;
            else if (type == typeof(RectInt)) return SerializedPropertyType.RectInt;
            else if (type == typeof(Rect)) return SerializedPropertyType.Rect;
            else if (type == typeof(LayerMask)) return SerializedPropertyType.LayerMask;
            else if (type == typeof(AnimationCurve)) return SerializedPropertyType.AnimationCurve;
            else if (type == typeof(BoundsInt)) return SerializedPropertyType.BoundsInt;
            else if (type == typeof(Bounds)) return SerializedPropertyType.Bounds;
            else if (type == typeof(Quaternion)) return SerializedPropertyType.Quaternion;
            else if (type == typeof(UnityEngine.Object)) return SerializedPropertyType.ObjectReference;
            else return SerializedPropertyType.Generic;
        }

        /// <summary>
        /// Parses string to type of given property. Reverse of ToString()
        /// </summary>
        public static object ParseString(this SerializedProperty property, string value)
        {
            return property.propertyType switch
            {
                SerializedPropertyType.Integer => int.Parse(value),
                SerializedPropertyType.Boolean => bool.Parse(value),
                SerializedPropertyType.Float => ParseSingleOrDouble(),
                SerializedPropertyType.String => value,
                SerializedPropertyType.Color => UnityParsing.ParseColor(value),
                SerializedPropertyType.Enum => (int)Enum.Parse(property.GetValue().GetType(), value),
                SerializedPropertyType.Vector2Int => UnityParsing.ParseVector2Int(value),
                SerializedPropertyType.Vector2 => UnityParsing.ParseVector2(value),
                SerializedPropertyType.Vector3Int => UnityParsing.ParseVector3Int(value),
                SerializedPropertyType.Vector3 => UnityParsing.ParseVector3(value),
                SerializedPropertyType.Vector4 => UnityParsing.ParseVector4(value),
                SerializedPropertyType.RectInt => UnityParsing.ParseRectInt(value),
                SerializedPropertyType.Rect => UnityParsing.ParseRect(value),
                SerializedPropertyType.BoundsInt => UnityParsing.ParseBoundsInt(value),
                SerializedPropertyType.Bounds => UnityParsing.ParseBounds(value),
                SerializedPropertyType.Quaternion => UnityParsing.ParseQuaternion(value),
                _ => throw new ArgumentException($"Parse for '{property.propertyType}' not supported")

                /*  case SerializedPropertyType.ExposedReference:
                    case SerializedPropertyType.ObjectReference:*/
            };

            object ParseSingleOrDouble()
            {
                if (DirtyValue.GetType(property) == typeof(double))
                    return double.Parse(value);
                else
                    return float.Parse(value);
            }
        }
        //get path

        /// <returns>pre path without dot and end path</returns>
        public static (string pre, string end) DividePath(string path, bool includeDotOnPre)
        {
            if (path == "")
                throw new ArgumentException("cannot divide emtpy path");

            int div = PreToEndPathDividePosition(path);
            if (div > 0)
            {
                if (includeDotOnPre)
                    return (path[..(div + 1)], path[(div + 1)..]);
                else
                    return (path[..div], path[(div + 1)..]);
            }
            else
                return ("", path);
        }
        /// <summary>
        /// The path minus the name on end
        /// </summary>
        public static string PrePath(this string path, bool includeDot)
        {
            // Bei array elementen wollen wir nicht das array sondern den besitzer von dem array (because unitys thing was it that attributes on arrays get applied to the elements)
            // array elemente: MyClassName.myArrayName.Array.data[i]

            int div = PreToEndPathDividePosition(path);
            if (div > 0)
                return path[..div];
            else
                return "";
        }
        /// <summary>
        /// The last name on the path. On array-elements it will return arrayName.Array.data[i] plus everything more nested
        /// </summary>
        /// <param name="path"></param>
        public static string PathEnd(this string path)
        {
            return path[(PreToEndPathDividePosition(path) + 1)..];
        }
        /// <summary>
        /// The last name on the path, but on array-elements it will return Array.data[i]
        /// </summary>
        /// <param name="path"></param>
        public static string FollowName(this string path)
        {
            int split = path.LastIndexOf('.');
            if(path[^1] == ']') //if array
                split = path.LastIndexOf('.', split - 1);
            return path[(split + 1)..];
        }
        /// <returns>The index of the dot, that divides prepath and pathEnd. -1 if path==EndPath and no prepath</returns>
        static int PreToEndPathDividePosition(string path)
        {
            // Bei array elementen wollen wir nicht das array sondern den besitzer von dem array (because unitys thing was it that attributes on arrays get applied to the elements)
            // array elemente: MyClassName.myArrayName.Array.data[i]

            if (path[^1] == ']') //its an array element
            {
                //get in front of array element access
                int i = path.Length - 1;
                do
                {
                    for (; path[i] != '['; i--)
                    {
                        if (i <= 0)
                            throw new ArgumentException($"Path '{path}' doesnt contain a name.\n(Array and data[i] is not a name)");
                    }
                    string eSyntax = ".Array.data[";
                    Debug.Assert(path[(i - eSyntax.Length + 1)..(i + 1)] == eSyntax, $"Expected array-element syntax of '{eSyntax}' on part '{path[(i - eSyntax.Length + 1)..(i + 1)]}' of path: '{path}'");
                    i -= eSyntax.Length;
                }
                while (path[i] == ']');

                Debug.Assert(path[i] != '.', $"Path '{path}' contains two following dots");

                //get in front of name
                while (i > 0 && path[i] != '.')
                    i--;
                if(i == 0)
                {
                    if (path[0] == '.')
                        throw new ArgumentException($"Path '{path}' cannot begin with a dot");
                    else
                        return -1;
                }
                return i;
            }
            else
            {
                int res = path.LastIndexOf('.');
                if (res == 0)
                    throw new ArgumentException($"Path '{path}' cannot begin with a dot");
                else if (res >= path.Length - 1)
                    throw new ArgumentException($"Path '{path}' cannot end with a dot");
                return res;
            }
        }
        /// <summary>
        /// The last name on the path, but on array-elements it will return array name
        /// </summary>
        /// <param name="path"></param>
        public static string NameOfPath(this string path)
        {
            // returns: 'myArrayName' from 'MyClassName.myArrayName.Array.data[i]'

            int start = PreToEndPathDividePosition(path) + 1;
            int end = start + 1;
            while (end < path.Length
                    && path[end] != '.')
                end++;

            return path[start..end];
        }

        public static (int nl, List<int> indices) ArrayNestedInfos(string path)
        {
            //abc.xyz.Array.data[3].Array.data[0] -> (2, {3, 0})

            if (path == null)
                throw new NullReferenceException("Path is null");
            if (path == "")
                throw new ArgumentException("empty path contains no informations");

            if (path[^1] != ']')
                return (0, new());

            int start = path.Length - 1;
            int nl = 0;
            List<int> indices = new();
            do
            {
                int end = start; // pointing on ']'
                for (; path[start] != '['; start--)
                {
                    if (start <= 0)
                        throw new ArgumentException($"Path '{path}' doesnt contain a name.\n(Array and data[i] is not a name)");
                }
                // end pointing on '['

                indices.Add(int.Parse(path[(start + 1)..end]));

                start -= ".Array.data[".Length;
                nl++;
            }
            while (path[start] == ']');

            return (nl, indices);
        }

        /// <summary>
        /// Updates the changes made in reflection on specific object
        /// </summary>
        /// <returns>Returns true if any pending changes were applied to the SerializedProperty</returns>
        public static bool ApplyModifiedFields(this SerializedObject serializedObject, bool withUndoOperation)
        {
            bool hadChanges = false;
            foreach (var prop in PropertyValues.GetAllPropertys(serializedObject, true))
            {
                hadChanges |= ObjectToPropChanges(prop);
            }
            if(hadChanges)
            {
                if (withUndoOperation)
                    serializedObject.ApplyModifiedProperties();
                else
                    serializedObject.ApplyModifiedPropertiesWithoutUndo();
            }
            return hadChanges;
        }
        /// <summary>
        /// Updates the changes made in reflection on specific field
        /// </summary>
        /// <returns>Returns true if any pending changes were applied to the SerializedProperty</returns>
        public static bool ApplyModifiedField(this SerializedProperty property, bool withUndoOperation)
        {
            bool hadChanges = ObjectToPropChanges(property);
            if (hadChanges)
            {
                if (withUndoOperation)
                    property.serializedObject.ApplyModifiedProperties();
                else
                    property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
            }
            return hadChanges;
        }
        /// <summary>
        /// transforms object changes to prop changes
        /// </summary>
        /// <returns>Returns true if there were a pending change</returns>
        static bool ObjectToPropChanges(SerializedProperty property)
        {
            return SwitchValues(property, new DirtyValue(property));

            ///<remarks>dirtyValue has to be the instantiation of the property </remarks>
            ///<returns>If values where different</returns>
            static bool SwitchValues(SerializedProperty property, DirtyValue dirty)
            {
                //Debug.Log($"SwitchValues of {property.propertyPath}");
                Debug.Assert(dirty.FieldOwner != null, $"Could not get value out of {dirty.Type} {dirty.FieldName}.\nIt has no owner to get value from");
                object dirtyValue = dirty.GetValue();

                if (property.propertyType == SerializedPropertyType.Generic)
                {
                    if (dirtyValue.IsUnityNull())
                    {
                        //we cant go deeper (because dirtyValue is null) -> We have to switch here

                        object propValue = property.GetValue();
                        if (propValue == null)
                            return false;

                        dirty.SetValue(propValue);
                        property.SetValue(null);

                        return true;
                    }
                    else if (property.isArray /*&& ((IList)dirtyValue).Count != property.arraySize*/)
                    {
                        //list has to be threatened different, because you can add and remove elements, so you cant just switch elements

                        IList dirtyList = (IList)dirtyValue;
                        IList cleanList = (IList)property.GetValue();

                        //Debug.Log($"dirtyList {dirtyList.Count} | cleanList {cleanList.Count}");

                        if(dirtyList.Count != cleanList.Count || !dirtyList.SequenceEqual(cleanList))
                        {
                            try
                            {
                                property.SetValue(dirtyList);
                                dirty.SetValue(cleanList);
                            }
                            catch (Exception e)
                            {
                                Debug.LogException(e);
                                Debug.LogError("Changes on array could not be saved");
                            }
                            
                            return true;
                        }
                        else
                            return false;
                    }
                    else // dirtyValue != null && no list with missing elements
                    {
                        bool hadChanges = false;
                        foreach (var prop in PropertyValues.GetAllPropertys(property, true))
                        {
                            DirtyValue propDirty = dirty.FindRelative(prop.propertyPath.FollowName(), forceFind: true);
                            hadChanges |= SwitchValues(prop, propDirty);
                        }

                        return hadChanges;
                    }
                }
                else //normal field
                {
                    //read
                    object propValue = property.GetValue();

                    //Null checks
                    if (propValue.IsUnityNull())
                    {
                        if (dirtyValue.IsUnityNull())
                            return false;
                    }
                    //Check if they are the same
                    else if (propValue.Equals(dirtyValue))
                    {
                        return false;
                    }

                    //Change values
                    property.SetValue(dirtyValue);
                    dirty.SetValue(propValue);

                    return true;
                }
            }
        }
        /// <returns>The element type of arrays and lists</returns>
        public static Type GetIListElementType(Type type)
        {
            if (type.IsArray)
            {
                return type.GetElementType();
            }
            else //list
            {
                Type[] args = type.GetGenericArguments();
                Debug.Assert(args.Length <= 1, $"Multiple type arguments not supported yet.\nError on '{type}' with arguments '{string.Join(", ", args.Select(_ => _.Name))}'");
                if (args.Length == 0)
                    throw new FormatException($"No generic arguments found on {type.Name}");
                return args[0];
            }
        }
    }
}