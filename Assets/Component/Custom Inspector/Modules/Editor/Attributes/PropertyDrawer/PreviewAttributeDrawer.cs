using CustomInspector.Extensions;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CustomInspector.Editor
{
    /// <summary>
    /// Draws label and field in 2 lines and on the right an icon
    /// </summary>
    [CustomPropertyDrawer(typeof(PreviewAttribute))]
    public class PreviewAttributeDrawer : PropertyDrawer
    {
        const float borderSize = 1;
        const float spaceFieldThumbnail = 5;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //Check is valid
            if (property.propertyType != SerializedPropertyType.ObjectReference)
            {
                DrawProperties.DrawPropertyWithMessage(position, label, property, $"Preview for {property.name} not supported (only object-references allowed)", MessageType.Error);
                return;
            }

            float thumbnailSize = ((PreviewAttribute)attribute).thumbnailSize;

            Rect labelRect;
            Rect fieldRect;
            Rect thumbnailRect;

            if (thumbnailSize <= 100)
            {
                labelRect = new Rect(position.x, position.y + thumbnailSize - 2 * EditorGUIUtility.singleLineHeight,
                                        EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
                fieldRect = new Rect(position.x, position.y + thumbnailSize - EditorGUIUtility.singleLineHeight,
                                        width: position.width - thumbnailSize - spaceFieldThumbnail - borderSize,
                                        height: EditorGUIUtility.singleLineHeight);
                thumbnailRect = new(position.x + position.width - thumbnailSize - borderSize - 1, position.y + borderSize,
                                        thumbnailSize, thumbnailSize);
            }
            else //make it next line
            {
                labelRect = new Rect(position.x, position.y,
                                    EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
                fieldRect = new Rect(position.x + EditorGUIUtility.fieldWidth, position.y,
                                    position.width, EditorGUIUtility.singleLineHeight);
                thumbnailRect = new(position.x + borderSize,
                                    position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing + borderSize,
                                    thumbnailSize, thumbnailSize);
            }
                
            //Label
            EditorGUI.LabelField(labelRect, label, EditorStyles.boldLabel);
            //Field
            DrawProperties.PropertyField(fieldRect, GUIContent.none, property);
            //Draw thumbnail
            Object m = (Object)property.GetValue();

            DrawProperties.DrawBorder(thumbnailRect, true, borderSize);
            Texture2D thumbnail = AssetPreview.GetAssetPreview(m); //this is prob bugged for hdrp?
            if (thumbnail == null)
                thumbnail = AssetPreview.GetMiniThumbnail(m);
            if (thumbnail != null)
                GUI.DrawTexture(thumbnailRect, thumbnail);
            
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.propertyType == SerializedPropertyType.ObjectReference)
            {
                float thumbnailSize = ((PreviewAttribute)attribute).thumbnailSize;
                thumbnailSize = Mathf.Min(thumbnailSize, EditorGUIUtility.currentViewWidth);

                if (thumbnailSize <= 100)
                    return thumbnailSize + EditorGUIUtility.standardVerticalSpacing + 2 * borderSize;
                else
                    return EditorGUIUtility.singleLineHeight + thumbnailSize + 2 * EditorGUIUtility.standardVerticalSpacing + 2 * borderSize;
            }
            else
            {
                return DrawProperties.GetPropertyWithMessageHeight(label, property);
            }
        }
    }
}