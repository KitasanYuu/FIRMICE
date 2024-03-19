using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating
{
    [System.Serializable]
    public partial struct MinMax
    {
        public int Min;
        public int Max;

        public bool IsZero { get { return Min == 0 && Max == 0; } }
        public static MinMax zero { get { return new MinMax(0,0); } }
        public Vector2 ToVector2 { get { return new Vector2(Min,Max); } }
        public Vector2Int ToVector2Int { get { return new Vector2Int(Min,Max); } }

        public MinMax(int min, int max)
        {
            Min = min;
            Max = max;
        }

        public int GetRandom()
        {
            return (int)(Min + (float)FGenerators.GetRandom() * ((Max + 1) - Min));
        }

#if UNITY_EDITOR

        public static MinMax DrawGUI(MinMax target, GUIContent label, bool clamp = true)
        {
            EditorGUILayout.BeginHorizontal();
            float width = EditorStyles.label.CalcSize(label).x;
            EditorGUIUtility.labelWidth = width + 4;
            EditorGUILayout.LabelField(label, GUILayout.Width(width));

            GUILayout.Space(28);
            EditorGUIUtility.labelWidth = 28;
            target.Min = EditorGUILayout.IntField("Min", target.Min, GUILayout.Width(70));

            //GUILayout.FlexibleSpace();
            GUILayout.Space(24);
            EditorGUIUtility.labelWidth = 32;
            target.Max = EditorGUILayout.IntField("Max", target.Max, GUILayout.Width(74));
            EditorGUILayout.EndHorizontal();
            EditorGUIUtility.labelWidth = 0;

            if (clamp)
            {
                if (target.Min < 0) target.Min = 0;
                if (target.Max < 0) target.Max = 0;
            }

            if (target.Min > target.Max) target.Max = target.Min;
            if (target.Max < target.Min) target.Min = target.Max;

            return target;
        }

#endif

    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(MinMax))]
    public class MinMaxDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            Rect srcPos = position;
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            EditorGUIUtility.labelWidth = 30;
            float labelW = EditorStyles.label.CalcSize(label).x;
            var vRect = new Rect(srcPos.width - 60 * 2f - 10, position.y, 60, position.height);

            int preInd = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            SerializedProperty sp_min = property.FindPropertyRelative("Min");
            EditorGUI.PropertyField(vRect, sp_min, new GUIContent(sp_min.displayName));

            vRect = new Rect(srcPos.width - 60, position.y, 60, position.height);
            SerializedProperty sp_max = property.FindPropertyRelative("Max");
            EditorGUI.PropertyField(vRect, sp_max, new GUIContent(sp_max.displayName));

            if (sp_min.intValue < 0) sp_min.intValue = 0;
            if (sp_max.intValue < 0) sp_max.intValue = 0;
            if (sp_min.intValue > sp_max.intValue) sp_max.intValue = sp_min.intValue;
            if (sp_max.intValue < sp_min.intValue) sp_min.intValue = sp_max.intValue;
            EditorGUI.indentLevel = preInd;

            EditorGUIUtility.labelWidth = 0;
            EditorGUI.EndProperty();
        }
    }
#endif


    [System.Serializable]
    public struct MinMaxF
    {
        public float Min;
        public float Max;

        public MinMaxF(float min, float max)
        {
            Min = min;
            Max = max;
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(MinMaxF))]
    public class MinMaxFDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            Rect srcPos = position;
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            EditorGUIUtility.labelWidth = 30;
            float labelW = EditorStyles.label.CalcSize(label).x;
            var vRect = new Rect(srcPos.width - 60 * 2f - 10, position.y, 60, position.height);

            SerializedProperty sp_min = property.FindPropertyRelative("Min");
            EditorGUI.PropertyField(vRect, sp_min, new GUIContent(sp_min.displayName));

            vRect = new Rect(srcPos.width - 60, position.y, 60, position.height);
            SerializedProperty sp_max = property.FindPropertyRelative("Max");
            EditorGUI.PropertyField(vRect, sp_max, new GUIContent(sp_max.displayName));

            if (sp_min.floatValue < 0) sp_min.floatValue = 0;
            if (sp_max.floatValue < 0) sp_max.floatValue = 0;
            if (sp_min.floatValue > sp_max.floatValue) sp_max.floatValue = sp_min.floatValue;
            if (sp_max.floatValue < sp_min.floatValue) sp_min.floatValue = sp_max.floatValue;

            EditorGUIUtility.labelWidth = 0;
            EditorGUI.EndProperty();
        }
    }
#endif

}
