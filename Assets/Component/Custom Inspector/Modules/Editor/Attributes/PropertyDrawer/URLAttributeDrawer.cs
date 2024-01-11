using CustomInspector.Extensions;
using UnityEditor;
using UnityEngine;


namespace CustomInspector.Editor
{

    [CustomPropertyDrawer(typeof(URLAttribute))]
    public class URLAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            URLAttribute u = (URLAttribute)attribute;

            Rect iposition = EditorGUI.IndentedRect(position);

            using (new NewIndentLevel(0))
            {
                GUIContent linkContent = new(u.link, "click to follow link");
                Rect urlRect = new(iposition)
                {
                    width = GUI.skin.label.CalcSize(linkContent).x + 5,
                    height = EditorGUIUtility.singleLineHeight,
                };

                if (!string.IsNullOrEmpty(u.label))
                {
                    GUIContent linkLabel = new(u.label, u.tooltip);
                    Rect linkLabelRect = new(urlRect)
                    {
                        width = GUI.skin.label.CalcSize(linkLabel).x,
                    };
                    urlRect.x += linkLabelRect.width + 5;
                    EditorGUI.LabelField(linkLabelRect, linkLabel);
                }

                if (EditorGUI.LinkButton(urlRect, linkContent))
                {
                    Application.OpenURL(u.link);
                }
            }

            //draw property under
            Rect pR = new(position)
            {
                y = iposition.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing,
                height = DrawProperties.GetPropertyHeight(label, property),
            };
            DrawProperties.PropertyField(pR, label, property);
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing
                    + DrawProperties.GetPropertyHeight(label, property);
        }
    }
}