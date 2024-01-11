using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

namespace CustomInspector.Extensions
{
    public static class DebugTest
    {
        /// <summary>
        /// Prints an Error, if any field in Class with given Attribute is null (to check if everything got filled in the inspector)
        /// <para>It prints found fields' names and the transforms Hierarchies of the Monobehaviour</para>
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        public static void CheckFilled(this MonoBehaviour @object, Type attributeType)
        {
            foreach (System.Reflection.FieldInfo field in @object.GetType().GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic))
            {
                if (System.Attribute.IsDefined(field, attributeType) && field.GetValue(@object) == null)
                    UnityEngine.Debug.LogError($"{field.Name} on {@object.transform.GetPathString()} is null. Fill it via inspector for Skript {@object.GetType().Name}");
            }
        }

        /// <summary>
        /// Prints an Error, if any field in Class with given Attribute is null (to check if everything got filled in the inspector)
        /// <para>It prints found fields' names and the given owners Hierarchies of the Monobehaviour</para>
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        public static void CheckFilled(this object _object, Transform owner, Type attributeType)
        {
            foreach (System.Reflection.FieldInfo field in _object.GetType().GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic))
            {
                if (System.Attribute.IsDefined(field, attributeType) && field.GetValue(_object) == null)
                {
                    if (owner != null)
                        UnityEngine.Debug.LogError($"{field.Name} on {owner.GetPathString()} is null. Fill it via inspector for Skript {_object.GetType().Name}");
                    else
                        UnityEngine.Debug.LogError($"{field.Name} on unknown owner is null. Fill it via inspector for Skript {_object.GetType().Name}");
                }
            }
        }
        /// <summary>
        /// For example: "scenename/superParent/Parent/child/lowerChild/lowestChild/thisName"
        /// (if he is child of lowestchild)
        /// </summary>
        static string GetPathString(this Transform child)
        {
            return child.gameObject.scene.name + "/" + string.Join("/", GetHierarchie(child).Select(_ => _.name));


            ///<summary>All Parents and the child - the most Top comes first - parent of this, parent of that parent,...</summary>
            static List<Transform> GetHierarchie(Transform child)
            {
                if (child.parent == null)
                    return new List<Transform>() { child };
                else
                {
                    List<Transform> myPath = GetHierarchie(child.parent);
                    myPath.Add(child);
                    return myPath;
                }
            }
        }
    }
}
