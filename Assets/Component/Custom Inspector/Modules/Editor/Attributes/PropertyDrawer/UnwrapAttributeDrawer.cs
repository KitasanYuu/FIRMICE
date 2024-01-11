using CustomInspector.Extensions;
using CustomInspector.Helpers;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CustomInspector.Editor
{
    [CustomPropertyDrawer(typeof(UnwrapAttribute))]
    public class UnwrapAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            PropInfo info = GetInfo(property);

            if(info.errorMessage != null)
            {
                DrawProperties.DrawPropertyWithMessage(position, label, property, info.errorMessage, MessageType.Error);
                return;
            }

            IEnumerable<SerializedProperty> props = info.childrenPaths.Select(_ => property.serializedObject.FindProperty(_));

            UnwrapAttribute u = (UnwrapAttribute)attribute;
            string prefix = (u.applyName && label?.text != null) ? $"{label.text}: " : "";
            foreach (var prop in props)
            {
                position.height = DrawProperties.GetPropertyHeight(prop);
                DrawProperties.PropertyField(position, property: prop, label: new GUIContent(prefix + prop.name, prop.tooltip));
                position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            PropInfo info = GetInfo(property);

            if (info.errorMessage != null)
            {
                return DrawProperties.GetPropertyWithMessageHeight(label, property);
            }

            IEnumerable<SerializedProperty> props = info.childrenPaths.Select(_ => property.serializedObject.FindProperty(_));

            return props.Select(_ => DrawProperties.GetPropertyHeight(_) + EditorGUIUtility.standardVerticalSpacing).Sum() - EditorGUIUtility.standardVerticalSpacing;
        }

        /// <summary>
        /// We save them for performance reasons
        /// </summary>
        Dictionary<PropertyIdentifier, PropInfo> savedInfos = new();
        PropInfo GetInfo(SerializedProperty property)
        {
            PropertyIdentifier id = new(property);
            if(!savedInfos.TryGetValue(id, out PropInfo info))
            {
                info = new(property);
                savedInfos.Add(id, info);
            }
            return info;
        }
        class PropInfo
        {
            public readonly string errorMessage;
            public readonly string[] childrenPaths;

            public PropInfo(SerializedProperty property)
            {
                if (property.IsArrayElement()) //is list element
                {
                    errorMessage = "[Unwrap]-attribute not valid on list elements. Use the [Unfold] attribute if you simply always want to expand the property.";
                    return;
                }

                if (property.propertyType != SerializedPropertyType.Generic)
                {
                    errorMessage = $"{nameof(UnwrapAttribute)} only valid on Generic's (a serialized class)";
                    return;
                }

                errorMessage = null;
                childrenPaths = property.GetAllVisiblePropertys(true).Select(_ => _.propertyPath).ToArray();
            }
        }
    }
}

