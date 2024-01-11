using CustomInspector.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace CustomInspector.Extensions
{
    public static class PropertyValues
    {
        public const BindingFlags defaultBindingFlags = BindingFlags.Instance
                                                      | BindingFlags.Static
                                                      | BindingFlags.Public
                                                      | BindingFlags.NonPublic
                                                      | BindingFlags.FlattenHierarchy;

        //Get values
        /// <summary>
        /// A generic way to get the serialized value of an existing serialized property
        /// </summary>
        /// <exception cref="NotSupportedException"></exception>
        public static object GetValue(this SerializedProperty property)
        {
            Debug.Assert(property != null, "property is null");
            return property.propertyType switch
            {
                SerializedPropertyType.AnimationCurve => property.animationCurveValue,
                SerializedPropertyType.Boolean => property.boolValue,
                SerializedPropertyType.BoundsInt => property.boundsIntValue,
                SerializedPropertyType.Bounds => property.boundsValue,
                SerializedPropertyType.Color => property.colorValue,
                //SerializedPropertyType.Double => serializedProperty.doubleValue,
                SerializedPropertyType.ExposedReference => property.exposedReferenceValue,
                SerializedPropertyType.Float /* or double */ => GetSingleOrDouble(),
                SerializedPropertyType.Integer => property.intValue,
                //SerializedPropertyType.Long => property.longValue,
                SerializedPropertyType.ManagedReference => property.managedReferenceValue,
                //SerializedPropertyType.ObjectReferenceInstanceID => property.objectReferenceInstanceIDValue,
                SerializedPropertyType.ObjectReference => property.objectReferenceValue,
                SerializedPropertyType.ArraySize => property.arraySize,
                SerializedPropertyType.Character => (char)property.intValue,
                //SerializedPropertyType.Gradient => property.gradientValue,
                SerializedPropertyType.Quaternion => property.quaternionValue,
                SerializedPropertyType.FixedBufferSize => property.fixedBufferSize,
                SerializedPropertyType.RectInt => property.rectIntValue,
                SerializedPropertyType.Rect => property.rectValue,
                SerializedPropertyType.LayerMask => (LayerMask)property.intValue,
                SerializedPropertyType.String => property.stringValue,
                SerializedPropertyType.Vector2Int => property.vector2IntValue,
                SerializedPropertyType.Vector2 => property.vector2Value,
                SerializedPropertyType.Vector3Int => property.vector3IntValue,
                SerializedPropertyType.Vector3 => property.vector3Value,
                SerializedPropertyType.Vector4 => property.vector4Value,
                SerializedPropertyType.Hash128 => property.hash128Value,


                SerializedPropertyType.Enum => Enum.ToObject(DirtyValue.GetType(property), property.intValue),
                SerializedPropertyType.Generic => GetGeneric(),

                _ => throw new NotSupportedException($"({property.propertyType} not supported)")
            };

            object GetSingleOrDouble()
            {
                Type type = DirtyValue.GetType(property);
                if (type == typeof(double))
                    return property.doubleValue;
                else
                    return property.floatValue;
            }
            object GetGeneric()
            {
                if (property.isArray)
                {
                    try
                    {
                        Type type = DirtyValue.GetType(property);
                        IList res = (IList)EditProperties.ForceCreateInstance(type);

                        if(!res.IsFixedSize) //list
                        {
                            foreach (object item in property.GetAllPropertys(false).Select(_ => _.GetValue()))
                            {
                                res.Add(item);
                            }
                        }
                        else //e.g. array
                        {
                            List<object> props = property.GetAllPropertys(false).Select(_ => _.GetValue()).ToList();
                            if (res.Count == props.Count)
                            {
                                for (int i = 0; i < props.Count; i++)
                                {
                                    res[i] = props[i];
                                }
                            }
                            else if(type.IsArray) //we can fix if is array
                            {
                                res = Array.CreateInstance(type.GetElementType(), props.Count);
                                for (int i = 0; i < props.Count; i++)
                                {
                                    res[i] = props[i];
                                }
                            }
                            else //sometimes res.Count is zero due to 'property is null when loading'
                                Debug.LogWarning($"{property.name}: {type} and property has different count: {res.Count} vs {props.Count}");
                        }
                        
                        return res;
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                        return null;
                    }
                }
                else // no array
                {
                    Type type = DirtyValue.GetType(property);
                    object res = EditProperties.ForceCreateInstance(type);
                    var props = property.GetAllPropertys(true);

                    foreach (var prop in props)
                    {
                        FieldInfo info = type.GetFieldWholeInheritance(prop.name, defaultBindingFlags);
                        info.SetValue(res, prop.GetValue());
                    }

                    return res;
                }
            }
        }

        public static IFindProperties GetOwnerAsFinder(this SerializedProperty property)
        {
            if(property.propertyPath[^1] == ']') // is array element: something.here.Array.data[i]
            {
                //last 2nd last dot
                int i = property.propertyPath.Length -1;
                while(property.propertyPath[i] != '.')
                    i--;
                i--;
                while (property.propertyPath[i] != '.')
                    i--;

                return new IFindProperties.ChildProp(property.serializedObject.FindProperty(property.propertyPath[..i]));
            }
            else //no array element
            {
                int lastDot = property.propertyPath.LastIndexOf('.');
                if(lastDot != -1)
                {
                    return new IFindProperties.ChildProp(property.serializedObject.FindProperty(property.propertyPath[..lastDot]));
                }
                else
                {
                    return new IFindProperties.PropertyRoot(property.serializedObject);
                }       
            }
        }

        public interface IFindProperties
        {
            public abstract SerializedProperty FindPropertyRelative(string name);
            public abstract Type GetPropertyType();
            public abstract bool IsArray();
            public abstract int GetArraySize();
            public abstract SerializedProperty GetArrayElementAtIndex(int index);
            public abstract string Name { get; }

            public class PropertyRoot : IFindProperties
            {
                public readonly SerializedObject obj;
                public PropertyRoot(SerializedObject obj)
                    => this.obj = obj;
                public SerializedProperty FindPropertyRelative(string propertyPath)
                    => obj.FindProperty(propertyPath: propertyPath);
                public Type GetPropertyType()
                    => obj.targetObject.GetType();
                public int GetArraySize()
                    => throw new ArgumentException("PropertyRoot cannot be an array");
                public SerializedProperty GetArrayElementAtIndex(int index)
                    => throw new ArgumentException("PropertyRoot cannot be an array");
                public bool IsArray()
                    => false;
                public string Name => obj.targetObject.name;
            }
            public class ChildProp : IFindProperties
            {
                public readonly SerializedProperty property;
                DirtyValue Dv
                {
                    get
                    {
                        if(dv == null)
                            dv = new DirtyValue(property);
                        return dv;
                    }
                }
                DirtyValue dv;
                public ChildProp(SerializedProperty property)
                {
                    if (property == null)
                        throw new NullReferenceException("property is null");
                    if (!property.isArray && property.propertyType != SerializedPropertyType.Generic)
                        throw new NotSupportedException($"{property.propertyType} not supported");

                    this.property = property;
                }
                public SerializedProperty FindPropertyRelative(string relativePropertyPath)
                    => property.FindPropertyRelative(relativePropertyPath: relativePropertyPath);
                public Type GetPropertyType()
                    => Dv.Type;
                public int GetArraySize()
                    => property.arraySize;//(Dv.GetValue() as IList).Count;
                public SerializedProperty GetArrayElementAtIndex(int index)
                    => property.GetArrayElementAtIndex(index);
                public bool IsArray() => property.isArray;
                public string Name => property.name;
            }
        }

        /// <summary>
        /// Searches a field in class and in all base classes of type. (it will prefer the least nested)
        /// </summary>
        /// <exception cref="MissingFieldException">If field was not found</exception>
        public static FieldInfo GetFieldWholeInheritance(this Type type, string name, BindingFlags bindingFlags = defaultBindingFlags)
        {
            Debug.Assert(!name.Contains('.'), "name instead of path expected");

            var res = type.GetField(name, bindingFlags);
            if (res == null)
                return GetFieldFromBaseClasses(type, name, bindingFlags);
            else
                return res;

            /// <summary>
            /// Search a field in all base classes of type
            /// </summary>
            /// <returns>The base class where field was found</returns>
            /// <exception cref="MissingFieldException">If field was not found</exception>
            static FieldInfo GetFieldFromBaseClasses(Type type, string name, BindingFlags bindingFlags = defaultBindingFlags)
            {
                FieldInfo fieldInfo;
                Type upperType = type.BaseType;
                while (upperType != null)
                {
                    fieldInfo = upperType.GetField(name, bindingFlags);
                    if (fieldInfo != null)
                        return fieldInfo;

                    upperType = upperType.BaseType;
                }
                if (name == "data" || name.Length >= 5 && name[..5] == "data[")
                    Debug.LogError("Maybe you meant an array/list element with 'data' instead of a field named 'data'.");
                throw new MissingFieldException($"Field '{name}' not found in '{type}' or in its base classes");
            }
        }

        public static bool IsArrayElement(this SerializedProperty property, out int index)
        {
            if (property.propertyPath[^1] != ']')
            {
                //property is not an array element
                index = -1;
                return false;
            }

            index = int.Parse(property.propertyPath[(property.propertyPath.LastIndexOf('[') + 1)..^1]);
            return true;
        }
        public static bool IsArrayElement(this SerializedProperty property)
        => property.IsArrayElement(out var _);

        public static GUIContent RepairLabel(GUIContent label, SerializedProperty property)
        {
            if (label.text == null) //even GUIContent.none has text = "" instead of null
                label = new(property.name, property.tooltip);
            Debug.Assert(label.text != null);
            label.text = PropertyConversions.NameFormat(label.text);
            return label;
        }
        public static IEnumerable<SerializedProperty> GetAllPropertys(this IFindProperties finder, bool alsoFromBase)
        {
            if (finder is null)
                throw new NullReferenceException("property is null");

            if (finder.IsArray())
            {
                return Enumerable.Range(0, finder.GetArraySize()).Select(_ => finder.GetArrayElementAtIndex(_));
            }
            else
            {
                Type type = finder.GetPropertyType(); //returned field can be owner array of current element (because element has no own field)

                IEnumerable<FieldInfo> fields;
                
                List<Type> baseClassesAndSelf = new() { type };
                if (alsoFromBase)
                {
                    Type t = type.BaseType;
                    if(t != null)
                        while (t != typeof(object)) //all classes are derived from Object in the end
                        {
                            baseClassesAndSelf.Add(t);
                            t = t.BaseType;
                        }
                    else Debug.LogWarning("Property has no specific type.");
                }

                fields = baseClassesAndSelf.Select(classType =>
                            classType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                                    .Where(field => !field.IsPublic &&  Attribute.IsDefined(field, typeof(SerializeField))
                                                    || field.IsPublic && !Attribute.IsDefined(field, typeof(NonSerializedAttribute))))
                            .SelectMany(x => x);

                //No null fields
                Debug.Assert(!fields.Any(x => x == null));
                //convert
                IEnumerable<SerializedProperty> props = fields.Select(_ => finder.FindPropertyRelative(_.Name));
                //return
                return props.Where(_ => _ != null);
            }
        }
        /// <summary>
        /// Returns all propertys in given object
        /// </summary>
        public static IEnumerable<SerializedProperty> GetAllPropertys(this SerializedObject obj, bool alsoFromBases)
        => GetAllPropertys(new IFindProperties.PropertyRoot(obj), alsoFromBases);

        /// <summary>
        /// Returns all propertys in given object that dont have HideInInspector defined
        /// </summary>
        /// <exception cref="NotSupportedException">Only on generic props</exception>
        public static IEnumerable<SerializedProperty> GetAllVisiblePropertys(this SerializedObject obj, bool alsoFromBases)
        => obj.GetAllPropertys(alsoFromBases).OnlyVisible();
        /// <summary>
        /// Returns all propertys in given property
        /// </summary>
        public static IEnumerable<SerializedProperty> GetAllPropertys(this SerializedProperty property, bool alsoFromBases)
        => GetAllPropertys(new IFindProperties.ChildProp(property), alsoFromBases);

        static IEnumerable<SerializedProperty> OnlyVisible(this IEnumerable<SerializedProperty> props)
            => props.Where(_ => !new DirtyValue(_).HasAttribute(typeof(HideInInspector)));
        /// <summary>
        /// Returns all propertys in given property that dont have HideInInspector defined
        /// </summary>
        /// <exception cref="NotSupportedException">Only on generic props</exception>
        public static IEnumerable<SerializedProperty> GetAllVisiblePropertys(this SerializedProperty property, bool alsoFromBases)
        => property.GetAllPropertys(alsoFromBases).OnlyVisible();


        /// <exception cref="ArgumentException">invalid path</exception>
        /// <exception cref="MissingFieldException">A field name on the path is not found</exception>
        /// <exception cref="MissingMethodException">the method name on the end of the path was not found</exception>
        /// <exception cref="Exceptions.WrongTypeException">If path has wrong format</exception>
        public static InvokableMethod GetMethodOnOwner(this SerializedProperty property, string methodPath,
                                    Type[] parameterTypes = null, BindingFlags bindingAttr = defaultBindingFlags)
        {
            return InvokableMethod.GetMethod(DirtyValue.GetOwner(property), methodPath, parameterTypes, bindingAttr);
        }

        /// <exception cref="ArgumentException">invalid path</exception>
        /// <exception cref="MissingFieldException">A field name on the path is not found</exception>
        /// <exception cref="MissingMethodException">the method name on the end of the path was not found</exception>
        /// <exception cref="Exceptions.WrongTypeException">If path has wrong format</exception>
        public static object CallMethodOnOwner(this SerializedProperty property, string methodPath,
                                    object[] parameters = null, BindingFlags bindingFlags = defaultBindingFlags)
        {
            var method = InvokableMethod.GetMethod(DirtyValue.GetOwner(property), methodPath, parameters?.Select(_ => _.GetType()).ToArray(), bindingFlags);
            return method.Invoke(parameters: parameters);
        }
        /// <exception cref="ArgumentException">invalid path</exception>
        /// <exception cref="MissingFieldException">A field name on the path is not found</exception>
        /// <exception cref="MissingMethodException">the method name on the end of the path was not found</exception>
        /// <exception cref="Exceptions.WrongTypeException">If path has wrong format</exception>
        public static InvokableMethod GetMethodInside(this SerializedProperty property, string methodPath,
                                    Type[] parameterTypes = null, BindingFlags bindingAttr = defaultBindingFlags)
        {
            return InvokableMethod.GetMethod(new DirtyValue(property), methodPath, parameterTypes, bindingAttr);
        }

        /// <exception cref="ArgumentException">invalid path</exception>
        /// <exception cref="MissingFieldException">A field name on the path is not found</exception>
        /// <exception cref="MissingMethodException">the method name on the end of the path was not found</exception>
        /// <exception cref="Exceptions.WrongTypeException">If path has wrong format</exception>
        public static object CallMethodInside(this SerializedProperty property, string methodPath,
                                    object[] parameters = null, BindingFlags bindingFlags = defaultBindingFlags)
        {
            var method = InvokableMethod.GetMethod(new DirtyValue(property), methodPath, parameters?.Select(_ => _?.GetType()).ToArray(), bindingFlags);
            return method.Invoke(parameters: parameters);
        }

    }
}