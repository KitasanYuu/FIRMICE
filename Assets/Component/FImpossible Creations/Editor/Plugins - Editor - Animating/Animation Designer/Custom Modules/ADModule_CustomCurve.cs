using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.AnimationTools.CustomModules
{

    public class ADModule_CustomCurve : ADCustomModuleBase
    {
        public override string ModuleTitleName { get { return "Utilities/Custom Animation Curve"; } }
        public override bool GUIFoldable { get { return false; } }
        public override bool SupportBlending { get { return false; } }

        enum EGizmosDraw
        {
            DontDrawGizmos, Height, Depth, Tresholds
        }

        EGizmosDraw gizmosStyle = EGizmosDraw.Height;
        float gizmosTreshold = 0.5f;

        public override void InspectorGUI_ModuleBody(float clipProgress, ADClipSettings_Main _anim_MainSet, AnimationDesignerSave s, ADClipSettings_CustomModules cModule, ADClipSettings_CustomModules.CustomModuleSet set)
        {
            EditorGUILayout.HelpBox("Module for Including Animation Curve in the exported animation clip", MessageType.None);

            // !!! The variables defined with 'GetVariable' are saved for each animation clip you modify
            // The variables you set inside module class (like public int...) are global for the setup file instance you will generate

            GUILayout.Space(4);
            var targetName = GetVariable("CurveName", set, "New Curve");
            targetName.GUISpacing = new Vector2(0, 6); // Spacing
            targetName.HideFlag = false;

            targetName.DrawGUI();
            GUILayout.Space(1);

            var minMax = GetVariable("MinMax", set, new Vector2(0f, 1f));
            Vector2 minMaxRange = minMax.GetVector2Value();

            // Define curve value parameter
            var curveVar = GetCurveVariable("Value1");
            AnimationDesignerWindow.DrawCurve(ref curveVar, "Value:", 0, 0, minMaxRange.x, 1f, minMaxRange.y);
            AnimationDesignerWindow.DrawCurveProgress(clipProgress, 150, 58);


            GUILayout.Space(14);
            EditorGUILayout.LabelField("Optional Helper Options Below", EditorStyles.centeredGreyMiniLabel);
            GUILayout.Space(1);

            EditorGUILayout.LabelField("Current Value: " + curveVar.Evaluate(clipProgress));
            GUILayout.Space(2);

            minMax.DrawGUI();

            gizmosStyle = (EGizmosDraw)EditorGUILayout.EnumPopup("Helper Gizmos Style:", gizmosStyle);
            if ( gizmosStyle == EGizmosDraw.Tresholds)
            {
                gizmosTreshold = EditorGUILayout.FloatField("Change Gizmo Color Below:", gizmosTreshold);
            }
        }


        public override void OnExportFinalizing(AnimationClip originalClip, AnimationClip newGeneratedClip, AnimationDesignerSave s, ADClipSettings_Main anim_MainSet, ADClipSettings_CustomModules customModules, ADClipSettings_CustomModules.CustomModuleSet set, List<AnimationEvent> addingEvents)
        {
            base.OnExportFinalizing(originalClip, newGeneratedClip, s, anim_MainSet, customModules, set, addingEvents);

            var targetName = GetVariable("CurveName", set, "New Curve");
            var curveVar = GetCurveVariable("Value1");

            AnimationCurve curve = AnimationDesignerWindow.CopyCurve(curveVar);
            AnimationGenerateUtils.DistrubuteCurveOnTime(ref curve, 0f, newGeneratedClip.length);
            newGeneratedClip.SetCurve("", typeof(Animator), targetName.GetStringValue(), curve);
        }


        public override void SceneView_DrawSceneHandles(ADClipSettings_CustomModules.CustomModuleSet customModuleSet, float alphaAnimation = 1, float progress = 0f)
        {
            var animD = AnimationDesignerWindow.Get;
            if (animD == null) return;
            Transform animT = animD.AnimatorTransform;
            if (animT == null) return;
            if (gizmosStyle == EGizmosDraw.DontDrawGizmos) return;

            Bounds b = animD.S.InitialBounds;
            b.center = animT.TransformPoint(b.center);

            Vector3 frontPos = b.center;

            var targetName = GetVariable("CurveName", customModuleSet, "New Curve");
            var curveVar = GetCurveVariable("Value1");
            float val = curveVar.Evaluate(progress);

            var minMax = GetVariable("MinMax", customModuleSet, new Vector2(0f, 1f));
            Vector2 mmax = minMax.GetVector2Value();

            string valueR = System.Math.Round(val, 2).ToString();
            Vector3 labelOff = animT.right * b.size.x * 0.3f + animT.forward * b.size.z * 0.3f;

            if (gizmosStyle == EGizmosDraw.Height)
            {
                frontPos += animT.forward * b.size.z * 0.7f;
                frontPos += animT.right * b.size.x * 0.7f;

                Vector3 botPos = frontPos;
                botPos.y = b.min.y;
                Vector3 uppPos = frontPos;
                uppPos.y = b.max.y;

                Handles.color = Color.white * 0.65f;
                Handles.DrawAAPolyLine(2, botPos, uppPos);

                Handles.color = Color.green * 0.75f;

                val = Mathf.InverseLerp(mmax.x, mmax.y, val);

                Vector3 curvePos = Vector3.Lerp(botPos, uppPos, val);
                Handles.SphereHandleCap(0, curvePos, Quaternion.identity, b.extents.magnitude * 0.125f, EventType.Repaint);

                Handles.Label(curvePos + labelOff, targetName.GetStringValue() + " = " + valueR);
            }
            else if (gizmosStyle == EGizmosDraw.Depth)
            {
                frontPos += animT.right * b.size.x * 0.7f;

                Vector3 botPos = frontPos;
                botPos.z = b.min.y;
                botPos -= animT.forward * b.size.y * 0.5f;
                Vector3 uppPos = frontPos;
                uppPos.z = b.max.y;
                uppPos -= animT.forward * b.size.y * 0.5f;

                Handles.color = Color.white * 0.65f;
                Handles.DrawAAPolyLine(2, botPos, uppPos);

                Handles.color = Color.green * 0.75f;

                val = Mathf.InverseLerp(mmax.x, mmax.y, val);

                Vector3 curvePos = Vector3.Lerp(botPos, uppPos, val);
                Handles.SphereHandleCap(0, curvePos, Quaternion.identity, b.extents.magnitude * 0.125f, EventType.Repaint);

                Handles.Label(curvePos + labelOff, targetName.GetStringValue() + " = " + valueR);
            }
            else if (gizmosStyle == EGizmosDraw.Tresholds)
            {
                frontPos += animT.right * b.size.x * 0.7f;

                if ( val > gizmosTreshold)
                    Handles.color = Color.green * 0.75f;
                else
                    Handles.color = Color.yellow * 0.75f;

                Handles.SphereHandleCap(0, frontPos, Quaternion.identity, b.extents.magnitude * 0.125f, EventType.Repaint);
                Handles.Label(frontPos + labelOff, targetName.GetStringValue() + " = " + valueR);
            }
        }

    }
}