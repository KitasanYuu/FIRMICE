using CustomInspector.Extensions;
using UnityEditor;
using UnityEngine;

namespace CustomInspector.Editor
{
    [CustomPropertyDrawer(typeof(AssetsOnlyAttribute))]
    public class AssetsOnlyAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.ObjectReference)
            {
                DrawProperties.DrawPropertyWithMessage(position, label, property,
                                "SceneObjectsOnlyAttribute only supports ObjectReferences", MessageType.Error);
                return;
            }

            string infoMessage = "AssetsOnly:\nyou cannot fill sceneObjects in here";
            GUIContent newLabel = new(label.text, string.IsNullOrEmpty(label.tooltip) ? infoMessage : $"{label.tooltip}\n{infoMessage}");

            EditorGUI.BeginChangeCheck();
            var res = EditorGUI.ObjectField(position: position, label: newLabel, obj: property.objectReferenceValue, objType: fieldInfo.FieldType, allowSceneObjects: false);
            if (EditorGUI.EndChangeCheck())
            {
                property.objectReferenceValue = res;
                property.serializedObject.ApplyModifiedProperties();
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.propertyType == SerializedPropertyType.ObjectReference)
                return DrawProperties.GetPropertyHeight(label, property);
            else
                return DrawProperties.GetPropertyWithMessageHeight(label, property);
        }
    }
}