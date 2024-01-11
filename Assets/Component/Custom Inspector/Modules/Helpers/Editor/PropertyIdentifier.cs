using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CustomInspector.Helpers
{
    /// <summary>
    /// Used to save errors on specific properties for performance through different OnGUI's
    /// </summary>
    public class PropertyIdentifier
    {
        public readonly Type targetObjectType;
        public readonly string propertyPath;

        public PropertyIdentifier(SerializedProperty property)
        {
            this.targetObjectType = property.serializedObject.targetObject.GetType();
            this.propertyPath = property.propertyPath;
        }
        public PropertyIdentifier(Type targetObject, string fullPath)
        {
            this.targetObjectType = targetObject;
            this.propertyPath = fullPath;
        }

        public bool Identifies(SerializedProperty property)
        {
            return targetObjectType == property.serializedObject.targetObject.GetType()
                    && propertyPath == property.propertyPath;
        }

        public override bool Equals(object obj)
        {
            if(obj is PropertyIdentifier identifier)
            {
                return EqualityComparer<Type>.Default.Equals(targetObjectType, identifier.targetObjectType) &&
                        propertyPath == identifier.propertyPath;
            }
            return false;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(targetObjectType, propertyPath);
        }
        public static bool operator ==(PropertyIdentifier i1, PropertyIdentifier i2)
        {
            if(i1 is null)
                return i2 is null;
            else if(i2 is null)
                return false;
            
            return i1.targetObjectType == i2.targetObjectType
                && i1.propertyPath == i2.propertyPath;
        }

        
        public static bool operator !=(PropertyIdentifier i1, PropertyIdentifier i2)
        {
            if (i1 is null)
                return i2 is not null;
            else if (i2 is null)
                return true;

            return i1.targetObjectType != i2.targetObjectType
                || i1.propertyPath == i2.propertyPath;
        }
    }
}
