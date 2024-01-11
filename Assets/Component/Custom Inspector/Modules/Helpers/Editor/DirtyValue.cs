using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace CustomInspector.Extensions
{
    /// <summary> Contains the dirty value of the current unity instantiation (this can be an array element) </summary>
    public class DirtyValue
    {
        public string FieldName => fieldInfo.Name;
        public Type Type { get; private set; } //is always filled
        public object FieldOwner => fieldOwner;
        /// <summary> If its possible to get value or fieldOwner is null </summary>
        public bool IsExisting => fieldOwner != null;

        (int level, List<int> indices) nested = (0, new List<int>()); //is filled
        object fieldOwner; //can be null
        FieldInfo fieldInfo; //is always filled


        // Reminder that in unity null-checks on casted obejcts have to been made with .IsUnityNull()
        // We can not directly convert it to null, bec we have cases, where we wamt to access inside the unity null object
        public virtual object GetValue()
        {
            if (!TryGetValue(out object value))
            {
                if (nested.level <= 0) //not in a list
                    throw new NotExistingException($"{fieldInfo.Name} in {fieldInfo.DeclaringType.FullName}");
                else //some list element (... of element, of element ...)
                    throw new NotExistingException($"{nested.level}-time(s) nested in {fieldInfo?.Name ?? "null"} in {fieldInfo.DeclaringType.FullName}");
            }
            else
                return value;
        }
        public bool TryGetValue(out object value)
        {
            if (fieldOwner == null)
            {
                value = null;
                return false;
            }

            if (nested.level <= 0)
                value = fieldInfo.GetValue(fieldOwner);
            else
            {
                value = fieldInfo.GetValue(fieldOwner);
                for (int i = 0; i < nested.level; i++)
                {
                    try
                    {
                        value = (value as IList)[nested.indices[i]];
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        value = null;
                        return false;
                    }
                }
            }

            return true;
        }
        public virtual void SetValue(object value)
        {
            if (fieldOwner == null)
                throw new NotExistingException();

            if (nested.level <= 0)
                fieldInfo.SetValue(fieldOwner, value);
            else
            {
                object obj = fieldInfo.GetValue(fieldOwner);
                for (int i = 0; i < nested.level - 1; i++)
                {
                    obj = (obj as IList)[nested.indices[i]];
                }
                (obj as IList)[nested.indices[^1]] = value;
            }
        }

        protected virtual FieldInfo GetFieldInfo() => fieldInfo;
        public bool HasAttribute(Type attr)
            => GetFieldInfo().IsDefined(attr);
        public T GetAttribute<T>() where T : Attribute
        {
            return (T)Attribute.GetCustomAttribute(GetFieldInfo(), typeof(T));
        }
        public Attribute[] GetAttributes()
        {
            return Attribute.GetCustomAttributes(GetFieldInfo());
        }

        public bool IsIList() => typeof(IList).IsAssignableFrom(Type);


        public static Type GetType(SerializedProperty property)
        {
            if (property == null)
                throw new NullReferenceException("serializedProperty is null");
            return new DirtyValue(property).Type;
        }

        /// <exception cref="NullReferenceException"></exception>
        public DirtyValue(SerializedProperty property)
        {
            if (property == null)
                throw new NullReferenceException("serializedProperty is null");
            if (string.IsNullOrEmpty(property.propertyPath))
                throw new ArgumentException("property has no path");
            if (property.serializedObject.targetObject == null)
                throw new NullReferenceException("targetObject is null");

            nested = PropertyConversions.ArrayNestedInfos(property.propertyPath);
            (string pre, string end) path = PropertyConversions.DividePath(property.propertyPath, false);
            Type fieldOwnerType;
            if (string.IsNullOrEmpty(path.pre)) //no pre
            {
                fieldOwner = property.serializedObject.targetObject;
                fieldOwnerType = fieldOwner.GetType();
            }
            else // has pre
            {
                DirtyValue owner = new(property.serializedObject.targetObject, path.pre);
                owner.TryGetValue(out fieldOwner);
                fieldOwnerType = owner.Type;
            }

            if (nested.level <= 0) //end has no .Array.data[i]
            {
                fieldInfo = fieldOwnerType.GetFieldWholeInheritance(path.end);
                Debug.Assert(fieldInfo != null, $"field '{path.end}' not found on '{fieldOwnerType}'");
                Type = fieldInfo.FieldType;
            }
            else
            {
                int ind = path.end.IndexOf('.');
                Debug.Assert(ind != -1, "Dot not found in nested path");
                string fieldName = path.end[..ind];
                fieldInfo = fieldOwnerType.GetFieldWholeInheritance(fieldName);
                Debug.Assert(fieldInfo != null, $"field '{fieldName}' not found on '{fieldOwnerType}'");
                Type = fieldInfo.FieldType;
                for (int i = 0; i < nested.level; i++)
                {
                    Type = PropertyConversions.GetIListElementType(Type);
                }
            }
        }
        /// <exception cref="NullReferenceException"></exception>
        public static DirtyValue GetOwner(SerializedProperty property)
        {
            if (property == null)
                throw new NullReferenceException("serializedProperty is null");
            if (property.serializedObject.targetObject == null)
                throw new NullReferenceException("targetObject is null");

            string prePath = property.propertyPath.PrePath(false);
            if (!string.IsNullOrEmpty(prePath))
                return new DirtyValue(property.serializedObject.targetObject, prePath);
            else
                return new TopLevel(property);
        }

        /// <exception cref="ArgumentException">invalid path</exception>
        /// <exception cref="MissingFieldException">A field name on the path is not found</exception>
        /// <exception cref="Exceptions.WrongTypeException">If path has wrong format</exception>
        public DirtyValue(object targetObject, string propertyPath, BindingFlags bindingAttr = PropertyValues.defaultBindingFlags)
        {
            if (targetObject == null)
                throw new NullReferenceException("targetObject is null");
            if (string.IsNullOrEmpty(propertyPath))
            {
                Debug.LogWarning("propertyPath is empty");
                throw new NullReferenceException("propertyPath is empty");
            }
            if (propertyPath[0] == '.')
                throw new ArgumentException("propertyPath cannot begin with '.'");

            int f = propertyPath.IndexOf('.');
            if (f == -1)
                f = propertyPath.Length;

            string first = propertyPath[..f];

            fieldOwner = targetObject;
            fieldInfo = targetObject.GetType().GetFieldWholeInheritance(first, bindingAttr);
            Type = fieldInfo.FieldType;

            if (f < propertyPath.Length)
            {
                FollowPath(propertyPath[(f + 1)..], bindingAttr);
            }
        }

        private DirtyValue((int level, List<int> indices) nested, object fieldOwner, FieldInfo field, Type type)
        {
            this.nested = nested;
            this.fieldOwner = fieldOwner;
            this.fieldInfo = field;
            this.Type = type;
        }

        /// <exception cref="ArgumentException">invalid path</exception>
        /// <exception cref="MissingFieldException">A field name on the path is not found</exception>
        /// <exception cref="Exceptions.WrongTypeException">If path has wrong format</exception>
        public virtual DirtyValue FindRelative(string propertyPath, BindingFlags bindingAttr = PropertyValues.defaultBindingFlags, bool forceFind = false)
        {
            DirtyValue res = new(nested, fieldOwner, fieldInfo, Type);
            res.FollowPath(propertyPath, bindingAttr, forceFind);
            
            return res;
        }

        /// <exception cref="ArgumentException">invalid path</exception>
        /// <exception cref="MissingFieldException">A field name on the path is not found</exception>
        /// <exception cref="Exceptions.WrongTypeException">If path has wrong format</exception>
        void FollowPath(string propertyPath, BindingFlags bindingAttr = PropertyValues.defaultBindingFlags, bool forceFind = false)
        {
            if (propertyPath[0] == '.')
                throw new ArgumentException("Path cannot begin with '.'");
            string[] propertys = propertyPath.Split('.');

            if (propertys.Length <= 0)
                throw new ArgumentException($"path '{propertyPath}' has no content");

            object targetObject;
            if (forceFind)
            {
                targetObject = GetValue();
                Debug.Assert(targetObject != null, $"{nameof(targetObject)} ({FieldName}) must not be null to forceFind members inside");
            }
            else
                TryGetValue(out targetObject);

            if (propertys.Length > 0)
            {
                for (int i = 0; i < propertys.Length; i++)
                {
                    if (propertys[i] != "Array") // last was no array
                    {
                        nested.level = 0;
                        nested.indices = new();

                        fieldInfo = Type.GetFieldWholeInheritance(propertys[i], bindingAttr);
                        Type = fieldInfo.FieldType;

                        fieldOwner = targetObject;
                        if (forceFind || fieldOwner != null)
                            targetObject = fieldInfo.GetValue(fieldOwner);
                    }
                    else //last was array
                    {
                        if (++i < propertys.Length) //wenn es ein element der liste ist : MyClassName.myArrayName.Array.data[i]
                        {
                            if (IsIList())
                            {
                                int.TryParse(propertys[i][5..^1], out int dataIndex); //Its naming is always "data[i]"

                                nested.level++;
                                nested.indices.Add(dataIndex);

                                Type = PropertyConversions.GetIListElementType(Type);

                                if(forceFind)
                                    targetObject = (targetObject as IList)[dataIndex];
                                else if (targetObject != null)
                                {
                                    try
                                    {
                                        targetObject = (targetObject as IList)[dataIndex];
                                    }
                                    catch (ArgumentOutOfRangeException)
                                    {
                                        targetObject = null;
                                    }
                                }
                            }
                            else
                            {
                                throw new Exceptions.WrongTypeException($"Expected collection instead of '{targetObject.GetType()}'");
                            }
                            continue;
                        }
                        else throw new ArgumentException($"Path '{propertyPath}' cannot end on 'Array'. Its missing the data index"); //always Array.data[i] on array elements
                    }
                }
            }
        }

        /// <summary>
        /// The most top value: the class itself
        /// </summary>
        public class TopLevel : DirtyValue
        {
            readonly object value;
            public TopLevel(SerializedProperty property)
            : base((0, null), null, null, null)
            {
                value = property.serializedObject.targetObject;
                Type = value.GetType();
            }
            public override object GetValue()
            {
                return value;
            }
            /// <param name="acceptUnityNull"> if false, any unity-null object will return c#'s null </param>
            public override DirtyValue FindRelative(string propertyPath, BindingFlags bindingAttr = PropertyValues.defaultBindingFlags, bool forceFind = false)
            {
                return new DirtyValue(GetValue(), propertyPath);
            }
            public override void SetValue(object value)
            {
                throw new NotSupportedException("Top-level dirty-value (serialized-targetObject) cannot be changed by value");
            }
            protected override FieldInfo GetFieldInfo()
            {
                throw new NotSupportedException("Top-level dirty-value (serialized-targetObject) does not own a field");
            }
        }

        public class NotExistingException : Exception
        {
            const string message = "The value you are accessing is not existing.\n" +
                                   "You are accessing a member inside a null-class";
            public NotExistingException()
            : base(message)
            { }
            public NotExistingException(string m)
            : base(m + "\n" + message)
            {

            }
        }
    }
}