using System;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.AnimationTools
{
    public partial class ADVariable
    {

        public void DrawGUI()
        {
            DrawVariable(this);
        }

        public void DrawBlendCurveGUI(int width = 50, float animationProgress = 0f)
        {
            var r = _BlendingCurveRanges;
            AnimationDesignerWindow.DrawCurve(ref blendingCurve, "", width, r.x, r.y, r.z, r.w);
            //if (animationProgress > 0f) AnimationDesignerWindow.DrawCurveProgressOnR(animationProgress);
        }

        private bool _GUI_wasSliderExtended = false;

        public static float DrawSliderFor(float val, float min, float max, ref bool wasExtended, float extMin = 0f, float extMax = 0f)
        {
            if ((min > 0 && max == 0f) /*|| (min == 0 && max > 0)*/)
                val = EditorGUILayout.FloatField(" ", val);
            else
            {
                float dMin = min;
                float dMax = max;

                if (min < extMin) extMin = min;
                if (max > extMax) extMax = max;

                if (!wasExtended)
                {
                    if (val < dMin) if (extMin < min) { dMin = extMin; dMax = extMax; wasExtended = true; }
                    if (val > dMax) if (extMax > max) { dMin = extMin; dMax = extMax; wasExtended = true; }
                }
                else
                {
                    dMin = extMin;
                    dMax = extMax;
                }

                val = GUILayout.HorizontalSlider(val, dMin, dMax);
                val = EditorGUILayout.FloatField(val, GUILayout.Width(32));

                if (val > dMax || val < dMin) wasExtended = true;

                min = dMin;
                max = dMax;
            }

            if (min > 0 && max == 0f)
            { if (val < min) val = min; }

            return val;
        }


        public static float DrawSliderFor(float val, float min, float max)
        {
            if ((min > 0 && max == 0f) /*|| (min == 0 && max > 0)*/)
                val = EditorGUILayout.FloatField(" ", val);
            else
            {
                val = EditorGUILayout.Slider(" ", val, min, max);
            }

            if (min > 0 && max == 0f)
            { if (val < min) val = min; }

            return val;
        }


        public static int DrawSliderForInt(float valf, float minf, float maxf)
        {
            int val = Mathf.RoundToInt(valf);
            int min = Mathf.RoundToInt(minf);
            int max = Mathf.RoundToInt(maxf);

            if ((min > 0 && max == 0f) /*|| (min == 0 && max > 0)*/)
                val = EditorGUILayout.IntField(" ", val);
            else
                val = EditorGUILayout.IntSlider(" ", val, min, max);

            if (min > 0 && max == 0f)
            { if (val < min) val = min; }

            return val;
        }

        static GUIContent _gcDraw = null;
        public static void DrawVariable(ADVariable toDraw)
        {
            if (toDraw == null) return;

            var v = toDraw;

            if (_gcDraw == null) _gcDraw = new GUIContent();

            if (v.GUISpacing.x > 0) GUILayout.Space(v.GUISpacing.x);
            _gcDraw.text = toDraw.ID;
            _gcDraw.tooltip = "";

            if (!string.IsNullOrWhiteSpace(toDraw.DisplayName))
                _gcDraw.text = toDraw.DisplayName;

            if (!string.IsNullOrWhiteSpace(toDraw.Tooltip))
                _gcDraw.tooltip = toDraw.Tooltip;

            float width = EditorStyles.boldLabel.CalcSize(_gcDraw).x + 6;

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(3);

            if (width > 220) width = 220;

            EditorGUILayout.LabelField(_gcDraw, EditorStyles.boldLabel, GUILayout.Width(width));
            GUILayout.Space(6);

            if (v.ValueType == ADVariable.EVarType.Number)
            {
                EditorGUIUtility.labelWidth = 10;

                if (v.FloatSwitch == EVarFloatingSwitch.Float)
                {
                    if (v.rangeHelper == Vector4.zero) v.Float = EditorGUILayout.FloatField(" ", v.Float);
                    else
                    {
                        if (v.rangeHelper.z == 0f && v.rangeHelper.w == 0f)
                            v.Float = DrawSliderFor(v.Float, v.rangeHelper.x, v.rangeHelper.y);
                        else
                            v.Float = DrawSliderFor(v.Float, v.rangeHelper.x, v.rangeHelper.y, ref toDraw._GUI_wasSliderExtended, v.rangeHelper.z, v.rangeHelper.w);
                    }
                }
                else
                {
                    if (v.rangeHelper == Vector4.zero) v.IntV = EditorGUILayout.IntField(" ", v.GetIntValue());
                    else
                    {
                        v.IntV = DrawSliderForInt(v.Float, v.rangeHelper.x, v.rangeHelper.y);
                    }
                }
            }
            else if (v.ValueType == ADVariable.EVarType.Bool)
            {
                EditorGUIUtility.labelWidth = 10;
                v.SetValue(EditorGUILayout.Toggle(GUIContent.none, v.GetBoolValue()));
            }
            else if (v.ValueType == ADVariable.EVarType.Vector3)
            {
                Color preC = GUI.color;

                EditorGUIUtility.labelWidth = 10;
                v.SetValue(EditorGUILayout.Vector3Field("", v.GetVector3Value()));
                //else
                //    v.SetValue(EditorGUILayout.Vector3IntField("", v.GetVector3IntValue()));
            }
            else if (v.ValueType == ADVariable.EVarType.Vector2)
            {
                EditorGUIUtility.labelWidth = 10;

                //if (v.FloatSwitch == EVarFloatingSwitch.Float)
                v.SetValue(EditorGUILayout.Vector2Field("", v.GetVector2Value()));
                //else
                //    v.SetValue(EditorGUILayout.Vector2IntField("", v.GetVector2IntValue()));
            }
            else if (v.ValueType == EVarType.String)
            {
                EditorGUIUtility.labelWidth = 10;
                v.SetValue(EditorGUILayout.TextField("", v.GetStringValue()));
            }
            else if (v.ValueType == EVarType.ProjectObject)
            {
                EditorGUIUtility.labelWidth = 10;
                Type type = v.ForceType;
                if (type == null) type = typeof(UnityEngine.Object);
                v.SetValue(EditorGUILayout.ObjectField(v.GetUnityObjRef(), type, true));
            }
            else if (v.ValueType == EVarType.Curve)
            {
                EditorGUIUtility.labelWidth = 10;

                float startT = v.rangeHelper.x;
                float startV = v.rangeHelper.y;
                float endT = v.rangeHelper.z;
                float endV = v.rangeHelper.w;

                if (v.rangeHelper == Vector4.zero || (startT == 0f && endT == 0f) || (startV == 0f && endV == 0f))
                {
                    startT = 0f;
                    endT = 1f;
                    startV = 0f;
                    endV = 1f;
                }

                if (AnimationDesignerWindow.Get)
                    AnimationDesignerWindow.DrawCurve(ref v.animCurve, "", Mathf.RoundToInt(v.v4Val.x), startT, startV, endT, endV);
                else
                {
                    if (v.rangeHelper == Vector4.zero)
                        v.SetValue(EditorGUILayout.CurveField(v.GetCurve(), Color.cyan, Rect.zero));
                    else
                    {
                        var cRect = new Rect(startT, startV, endT - startT, endV - startV);
                        v.SetValue(EditorGUILayout.CurveField(v.GetCurve(), Color.cyan, cRect));
                    }
                }
            }

            EditorGUIUtility.labelWidth = 0;
            GUILayout.Space(3);

            if (v.DisplayBlendingCurve)
            {
                float animProgress = 0f;
                if (AnimationDesignerWindow.Get) animProgress = AnimationDesignerWindow.Get.LastAnimationProgress;
                v.DrawBlendCurveGUI(50, animProgress);
            }

            EditorGUILayout.EndHorizontal();
            if (v.GUISpacing.y > 0) GUILayout.Space(v.GUISpacing.y);
        }

    }
}
