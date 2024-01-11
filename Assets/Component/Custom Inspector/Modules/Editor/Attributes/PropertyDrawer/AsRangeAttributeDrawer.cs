using CustomInspector.Extensions;
using UnityEditor;
using UnityEngine;

namespace CustomInspector.Editor
{
    [CustomPropertyDrawer(typeof(AsRangeAttribute))]
    public class AsRangeAttributeDrawer : PropertyDrawer
    {
        const float numberToSliderDistance = 5; //distance on the right between the two values and the slider between them
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
#if UNITY_EDITOR
            position.height = EditorGUIUtility.singleLineHeight;
            AsRangeAttribute r = (AsRangeAttribute)attribute;

            if(property.propertyType != SerializedPropertyType.Vector2)
            {
                EditorGUI.LabelField(position, label, " ");
                Rect errorRect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y, position.width - EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
                EditorGUI.HelpBox(errorRect, "Use AsRange only on vector2's", MessageType.Error);
                return;
            }

            
            if (r.minLimit != r.maxLimit)
            {
                //Label
                EditorGUI.LabelField(position, label);

                Vector2 value = property.vector2Value;
                float minVal, maxVal;

                Rect number1Rect = new(position)
                {
                    x = position.x + EditorGUIUtility.labelWidth,
                    width = Mathf.Min(EditorGUIUtility.fieldWidth, ((position.width - EditorGUIUtility.labelWidth) - numberToSliderDistance) / 2),
                };
                Rect number2Rect = new(number1Rect)
                {
                    x = position.x + position.width - number1Rect.width,
                };
                Rect sliderRect = new Rect(x: number1Rect.x + number1Rect.width + numberToSliderDistance,
                                           y: position.y,
                                           width: (position.width - EditorGUIUtility.labelWidth) - (number1Rect.width + number2Rect.width) - 2 * numberToSliderDistance,
                                           height: EditorGUIUtility.singleLineHeight);

                EditorGUI.BeginChangeCheck();
                using (new NewIndentLevel(0))
                {
                    if (r.minLimit < r.maxLimit)
                    {
                        //1st number
                        minVal = Mathf.Max(r.minLimit, Mathf.Min(value.y, EditorGUI.FloatField(number1Rect, value.x)));

                        //2nd number
                        maxVal = Mathf.Min(r.maxLimit, Mathf.Max(value.x, EditorGUI.FloatField(number2Rect, value.y)));

                        //Slider
                        if (sliderRect.width > 0)
                            EditorGUI.MinMaxSlider(sliderRect, ref minVal, ref maxVal, r.minLimit, r.maxLimit);
                    }
                    else //minLimit > maxLimit
                    {
                        //1st number
                        minVal = Mathf.Min(r.minLimit, Mathf.Max(value.y, EditorGUI.FloatField(number1Rect, value.x)));

                        //2nd number
                        maxVal = Mathf.Max(r.maxLimit, Mathf.Min(value.x, EditorGUI.FloatField(number2Rect, value.y)));

                        //Slider
                        if (sliderRect.width > 0)
                        {
                            float invMin = -1 * minVal;
                            float invMax = -1 * maxVal;
                            EditorGUI.MinMaxSlider(sliderRect, ref invMin, ref invMax, -1 * r.minLimit, -1 * r.maxLimit);
                            minVal = -1 * invMin;
                            maxVal = -1 * invMax;
                        }
                    }
                }
                //Apply
                if (EditorGUI.EndChangeCheck())
                {
                    property.vector2Value = new Vector2(minVal, maxVal);
                    property.serializedObject.ApplyModifiedProperties();
                }
            }
            else
            {
                EditorGUI.LabelField(position, label);
                Rect errorRect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y, position.width - EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
                EditorGUI.HelpBox(errorRect, "Range is zero (maxLimit=minLimit).", MessageType.Error);
            }
#endif
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        }
    }
}