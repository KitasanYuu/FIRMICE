using CustomInspector.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace CustomInspector.Editor
{
    /// <summary>
    /// Draws a errorMessage if there are some in MessageDrawer
    /// </summary>
    [CustomPropertyDrawer(typeof(MessageDrawer))]
    [CustomPropertyDrawer(typeof(MessageDrawerAttribute))]
    public class MessageDrawerDrawer : PropertyDrawer
    {
#if UNITY_EDITOR
        public const int messageSize = 35;

        const float minSize = 350; //size at what the spacing disappears
        const float spacing = 0.2f; //proportion of helpbox start

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            DirtyValue messages = new DirtyValue(property).FindRelative("messages");

            if (messages.IsExisting)
            {
                var mList = (List<(string content, MessageBoxType type)>)messages.GetValue();
                if (mList.Count > 0)
                {
                    int savedIndentLevel = EditorGUI.indentLevel;
                    EditorGUI.indentLevel = 0;
                    {
                        Rect messageRect = new (position);
                        float space = Mathf.Min(Mathf.Max(position.width - minSize, 0), position.width * spacing);
                        messageRect.x += space;
                        messageRect.width -= space;
                        messageRect.height = messageSize;
                        for (int i = 0; i < mList.Count; i++)
                        {
                            EditorGUI.HelpBox(messageRect, mList[i].content, MessageBoxConvert.ToUnityMessageType(mList[i].type));

                            messageRect.y += messageSize;
                            messageRect.y += EditorGUIUtility.standardVerticalSpacing;
                        }

                    }
                    EditorGUI.indentLevel = savedIndentLevel;
                }
            }
            else
            {
                position.height = EditorGUIUtility.singleLineHeight;
                EditorGUI.HelpBox(position, $"MessageDrawer is null", MessageType.Error);
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {

            DirtyValue messages = new DirtyValue(property).FindRelative("messages");

            if (messages.IsExisting)
            {
                return (messageSize + EditorGUIUtility.standardVerticalSpacing) * ((IList)messages.GetValue()).Count;
            }
            else
            {
                return EditorGUIUtility.singleLineHeight; //error that messagedrawer is null
            }
        }
#endif
    }
}