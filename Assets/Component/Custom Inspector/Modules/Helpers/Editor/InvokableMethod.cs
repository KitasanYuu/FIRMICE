using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace CustomInspector.Extensions
{
    public class InvokableMethod
    {
        object owner;
        MethodInfo info;

        public Type ReturnType => info.ReturnType;
        public string Name => info.Name;



        public InvokableMethod(object owner, MethodInfo info)
        {
            this.owner = owner;
            Debug.Assert(owner.GetType() != typeof(UnityEditor.SerializedProperty) && owner.GetType() != typeof(UnityEditor.SerializedObject), "Expected the dirty object");
            Debug.Assert(owner.GetType() != typeof(DirtyValue), "Expected the *actual* dirty object not the holder");
            this.info = info;
        }
        public InvokableMethod(object owner, string methodName, Type[] parameterTypes = null, BindingFlags bindingAttr = PropertyValues.defaultBindingFlags)
        {
            if (owner == null)
                throw new NullReferenceException("method owner is null");
            Debug.Assert(owner.GetType() != typeof(UnityEditor.SerializedProperty) && owner.GetType() != typeof(UnityEditor.SerializedObject), "Expected the dirty object");
            Debug.Assert(owner.GetType() != typeof(DirtyValue), "Expected the *actual* dirty object not the holder");
            Debug.Assert(!methodName.Contains('.'), "No path accepted. Name expected");

            this.owner = owner;
            info = GetMethod(owner, methodName, parameterTypes, bindingAttr);

            if (info == null)
                throw new MissingMethodException($"method '{methodName}' not found in '{owner.GetType()}'");
        }

        /// <exception cref="ArgumentException">invalid path</exception>
        /// <exception cref="MissingFieldException">A field name on the path is not found</exception>
        /// <exception cref="MissingMethodException">the method name on the end of the path was not found</exception>
        /// <exception cref="Exceptions.WrongTypeException">If path has wrong format</exception>
        public static InvokableMethod GetMethod(DirtyValue obj, string methodPath,
                                    Type[] parameterTypes = null, BindingFlags bindingAttr = PropertyValues.defaultBindingFlags)
        {
            int last = methodPath.LastIndexOf('.');
            if (last != -1)
            {
                obj = obj.FindRelative(methodPath[..last]);
            }

            string methodName = methodPath[(last + 1)..];
            object methodOwner = obj.GetValue();
            MethodInfo info = GetMethod(methodOwner, methodName, parameterTypes, bindingAttr);
            return new InvokableMethod(methodOwner, info);
        }

        static MethodInfo GetMethod(object owner, string methodName, Type[] parameterTypes = null, BindingFlags bindingAttr = PropertyValues.defaultBindingFlags)
        {
            Debug.Assert(!methodName.Contains('.'), "name instead of path expected");
            MethodInfo info;
            Type ownerType = owner.GetType();
            if ((parameterTypes?.Length ?? 0) == 0)
            {
                info = ownerType.GetMethod(name: methodName, bindingAttr: bindingAttr, null, CallingConventions.Any, Type.EmptyTypes, null);
                if (info == null)
                    throw new MissingMethodException($"'{methodName}' not found in '{ownerType}'");
            }
            else
            {
                if (!parameterTypes.Contains(null))
                {
                    info = ownerType.GetMethod(name: methodName, bindingAttr: bindingAttr, null, CallingConventions.Any, parameterTypes, null);
                    if (info == null)
                        throw new MissingMethodException($"'{methodName}' not found in '{ownerType}' with parameters '{string.Join(separator: ", ", parameterTypes.Select(_ => _.ToString()))}'");
                }
                else // some parameter types are not defined (null)
                {
                    var sameName = ownerType.GetMethods(bindingAttr)
                                    .Where(_ => _.Name == methodName); //namen filtern

                    Debug.Assert(sameName.Any(), "No methods with same name found");

                    var paramAmounts = sameName.Select(_ => (method: _, paramTypes: _.GetParameters().Select(_ => _.ParameterType).ToList())) //parameter lesen
                                    .Where(_ => _.paramTypes.Count == parameterTypes.Length); //anzahl an paramtern

                    Debug.Assert(sameName.Any(), "No methods with same parameter amount found");

                    var sameParams = paramAmounts.Select(_ => (method: _.method, paramTypes: _.paramTypes.Select((_, i) => (type: _, index: i))))// get indices
                                    .Where(_ => _.paramTypes.All(t => (parameterTypes[t.index] == null || t.type == parameterTypes[t.index]))); //selbe parameter, wenn nicht null

                    if (sameParams.Any())
                    {
                        if (sameParams.Take(2).Count() == 1)
                            info = sameParams.First().method;
                        else
                            throw new MissingMethodException($"There are several methods that fit on the parameters: {string.Join(", ", parameterTypes.Select(_ => _?.ToString() ?? "undefined"))}" +
                                                             $"\nMethods: {string.Join(", ", sameParams.Select(_ => $"{_.method.Name}({string.Join(", ", _.paramTypes)})"))}");
                    }
                    else
                    {
                        throw new MissingMethodException($"No method {methodName} found on {ownerType}");
                    }
                }
            }
            return info;
        }

        public T GetAttribute<T>() where T : Attribute
            => info.GetCustomAttribute<T>();
        public IEnumerable<Attribute> GetAttributes()
            => info.GetCustomAttributes();

        public int ParameterCount() => info.GetParameters().Length;

        public object Invoke()
        {
            return info.Invoke(owner, null);
        }
        public object Invoke(params object[] parameters)
        {
            return info.Invoke(owner, parameters);
        }
    }
}