using FIMSpace.FEditor;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.AnimationTools.CustomModules
{
    public class ADModule_HumanoidLegPoserAllParams : ADHumanoidMuclesModuleBase
    {

        public override string ModuleTitleName { get { return "Humanoid/All Params/Leg Poser-All Params"; } }


        public override void HumanoidChanges(Animator mecanim, float progr, float deltaTime, AnimationDesignerSave s, ADClipSettings_Main anim_MainSet, ADClipSettings_CustomModules customModules, ADClipSettings_CustomModules.CustomModuleSet set)
        {
            float blend = GetEvaluatedBlend(set, progr);
            int startI;

            var vR = GetVariable("Right", set, true);
            bool right = vR.GetBoolValue();


            var vAddit = GetVariable("Additive", set, true);
            bool additive = vAddit.GetBoolValue();


            // -------------------------

            float tgtBlend = blend;
            tgtBlend *= GetVariable("UpLegB", set, 1f).Float;

            //var contr = GetVariable("UpLeg", set, Vector3.zero);
            startI = right ? MID_RightUpperLegFrontBack : MID_LeftUpperLegFrontBack;

            if (tgtBlend > 0f)
                ApplyMusclesV3(startI, GetVariable("UL_FB", set).GetBlendedFloat(progr), GetVariable("UL_IO", set).GetBlendedFloat(progr), GetVariable("UL_TW", set).GetBlendedFloat(progr), tgtBlend, additive);

            tgtBlend = blend;
            tgtBlend *= GetVariable("LowLegB", set, 1f).Float;
            //contr = GetVariable("LowLeg", set, Vector2.zero);
            startI = right ? MID_RightLowerLegStretch : MID_LeftLowerLegStretch;

            if (tgtBlend > 0f)
                ApplyMusclesV2(startI, GetVariable("LL_ST", set).GetBlendedFloat(progr), GetVariable("LL_TW", set).GetBlendedFloat(progr), tgtBlend, additive);

            tgtBlend = blend;
            tgtBlend *= GetVariable("FootB", set, 1f).Float;
            //contr = GetVariable("Foot", set, Vector3.zero);
            startI = right ? MID_RightFootUpDown : MID_LeftFootUpDown;

            if (tgtBlend > 0f)
                ApplyMusclesV3(startI, GetVariable("F_UD", set).GetBlendedFloat(progr), GetVariable("F_TW", set).GetBlendedFloat(progr), GetVariable("F_TO", set).GetBlendedFloat(progr), tgtBlend, additive);

        }



        #region Editor GUI Related Code

        public override void InspectorGUI_ModuleBody(float optionalBlendGhost, ADClipSettings_Main _anim_MainSet, AnimationDesignerSave s, ADClipSettings_CustomModules cModule, ADClipSettings_CustomModules.CustomModuleSet set)
        {
            if (GUI_CheckForHumanoidAnimator(s, _anim_MainSet) == false) return;

            //var vR = GetVariable("Right", set, true);
            //vR.DisplayName = "Right Leg";
            //vR.Tooltip = "Enable to use Right Leg, disable to use Left Leg";

            //var vAddit = GetVariable("Additive", set, true);
            //vAddit.Tooltip = "Use additive blend or override muscle value with still value";
            //vAddit.GUISpacing = new Vector2(0, 6);


            EditorGUILayout.BeginHorizontal();

            var vR = GetVariable("Right", set, true);
            vR.DisplayName = "Right Leg";
            vR.Tooltip = "Enable to use Right Leg, disable to use Left Leg";

            vR.DrawGUI();

            var vAddit = GetVariable("Additive", set, true);
            vAddit.Tooltip = "Use additive blend or override muscle value with still value";
            vAddit.GUISpacing = new Vector2(0, 6);

            GUILayout.FlexibleSpace();
            vAddit.DrawGUI();

            EditorGUILayout.EndHorizontal();

            FGUI_Inspector.DrawUILineCommon();


            var vUpLegBlend = GetVariable("UpLegB", set, 1f);
            vUpLegBlend.SetRangeHelperValue(new Vector2(0f, 1f));
            vUpLegBlend.DisplayName = "Upper Leg Blend";
            vUpLegBlend.HideFlag = vAddit.GetBoolValue();

            var vUpLegFB = GetVariable("UL_FB", set, 0f); vUpLegFB.DisplayName = "   Upper Leg Front-Back:"; vUpLegFB.SetRangeHelperValue(-1f, 1f);
            var vUpLegIO = GetVariable("UL_IO", set, 0f); vUpLegIO.DisplayName = "   Upper Leg In-Out:"; vUpLegIO.SetRangeHelperValue(-1f, 1f);
            var vUpLegTW = GetVariable("UL_TW", set, 0f); vUpLegTW.DisplayName = "   Upper Leg Twist In-Out:"; vUpLegTW.SetRangeHelperValue(-1f, 1f);

            //var vUpLeg = GetVariable("UpLeg", set, Vector3.zero);
            //vUpLeg.DisplayName = "Upper Leg Controls";
            //vUpLeg.Tooltip = "X: UpperLeg FrontBack\nY: UpperLeg InOut\nZ: UpperLeg TwistInOut";


            var vLowLegBlend = GetVariable("LowLegB", set, 1f);
            vLowLegBlend.SetRangeHelperValue(new Vector2(0f, 1f));
            vLowLegBlend.DisplayName = "Lower Leg Blend";
            vLowLegBlend.HideFlag = vAddit.GetBoolValue();

            //var vLowLeg = GetVariable("LowLeg", set, Vector2.zero);
            //vLowLeg.DisplayName = "Lower Leg Controls";
            //vLowLeg.Tooltip = "X: LowerLeg Stretch\nY: LowerLeg TwistInOut";
            var vLowLegST = GetVariable("LL_ST", set, 0f); vLowLegST.DisplayName = "   Lower Leg Stretch:"; vLowLegST.SetRangeHelperValue(-1f, 1f);
            var vLowLegTW = GetVariable("LL_TW", set, 0f); vLowLegTW.DisplayName = "   Lower Leg Twist In-Out:"; vLowLegTW.SetRangeHelperValue(-1f, 1f);


            var vFootBlend = GetVariable("FootB", set, 1f);
            vFootBlend.SetRangeHelperValue(new Vector2(0f, 1f));
            vFootBlend.DisplayName = "Foot Blend";
            vFootBlend.HideFlag = vAddit.GetBoolValue();

            var vFootFB = GetVariable("F_UD", set, 0f); vFootFB.DisplayName = "   Foot Front-Back:"; vFootFB.SetRangeHelperValue(-1f, 1f);
            var vFootTW = GetVariable("F_TW", set, 0f); vFootTW.DisplayName = "   Foot Twist In-Out:"; vFootTW.SetRangeHelperValue(-1f, 1f);
            var vFootTO = GetVariable("F_TO", set, 0f); vFootTO.DisplayName = "   Toes Up-Down:"; vFootTO.SetRangeHelperValue(-1f, 1f);

            //var vFoot = GetVariable("Foot", set, Vector3.zero);
            //vFoot.DisplayName = "Foot Controls";
            //vFoot.Tooltip = "X: Foot UpDown\nY: Foot TwistInOut\nZ: Toes UpDown";

            //base.InspectorGUI_ModuleBody(optionalBlendGhost, _anim_MainSet, s, cModule, set);


            //Vector3 v3;

            vUpLegBlend.Float = EditorGUILayout.Slider("Upper Leg Blend:", vUpLegBlend.Float, 0f, 1f);

            if (vUpLegBlend.Float > 0f)
            {
                vUpLegFB._BlendingCurveRanges = new Vector4(0f, 0f, 1f, 1f); vUpLegFB.DisplayBlendingCurve = true;
                vUpLegIO._BlendingCurveRanges = new Vector4(0f, 0f, 1f, 1f); vUpLegIO.DisplayBlendingCurve = true;
                vUpLegTW._BlendingCurveRanges = new Vector4(0f, 0f, 1f, 1f); vUpLegTW.DisplayBlendingCurve = true;

                GUILayout.Space(4);
                vUpLegFB.DrawGUI();
                vUpLegIO.DrawGUI();
                vUpLegTW.DrawGUI();
                //EditorGUI.indentLevel++;
                //v3 = vUpLeg.GetVector3Value();
                //v3.x = EditorGUILayout.Slider("Upper Leg Front-Back:", v3.x, -1f, 1f);
                //v3.y = EditorGUILayout.Slider("Upper Leg In Out:", v3.y, -1f, 1f);
                //v3.z = EditorGUILayout.Slider("Upper Leg Twist In-Out:", v3.z, -1f, 1f);
                //vUpLeg.SetValue(v3);
                //EditorGUI.indentLevel--;
                GUILayout.Space(4);
            }

            FGUI_Inspector.DrawUILineCommon();


            vLowLegBlend.Float = EditorGUILayout.Slider("Lower Leg Blend:", vLowLegBlend.Float, 0f, 1f);

            if (vLowLegBlend.Float > 0f)
            {
                vLowLegST._BlendingCurveRanges = new Vector4(0f, 0f, 1f, 1f); vLowLegST.DisplayBlendingCurve = true;
                vLowLegTW._BlendingCurveRanges = new Vector4(0f, 0f, 1f, 1f); vLowLegTW.DisplayBlendingCurve = true;

                GUILayout.Space(4);
                vLowLegST.DrawGUI();
                vLowLegTW.DrawGUI();
                //EditorGUI.indentLevel++;
                //Vector2 v2 = vLowLeg.GetVector2Value();
                //v2.x = EditorGUILayout.Slider("Lower Leg Stretch:", v2.x, -1f, 1f);
                //v2.y = EditorGUILayout.Slider("Lower Leg Twist In-Out:", v2.y, -1f, 1f);
                //vLowLeg.SetValue(v2);
                //EditorGUI.indentLevel--;
                GUILayout.Space(4);
            }

            FGUI_Inspector.DrawUILineCommon();


            vFootBlend.Float = EditorGUILayout.Slider("Foot Blend:", vFootBlend.Float, 0f, 1f);

            if (vFootBlend.Float > 0f)
            {
                vFootFB._BlendingCurveRanges = new Vector4(0f, 0f, 1f, 1f); vFootFB.DisplayBlendingCurve = true;
                vFootTW._BlendingCurveRanges = new Vector4(0f, 0f, 1f, 1f); vFootTW.DisplayBlendingCurve = true;
                vFootTO._BlendingCurveRanges = new Vector4(0f, 0f, 1f, 1f); vFootTO.DisplayBlendingCurve = true;

                GUILayout.Space(4);
                vFootFB.DrawGUI();
                vFootTW.DrawGUI();
                vFootTO.DrawGUI();
                //EditorGUI.indentLevel++;
                //v3 = vFoot.GetVector3Value();
                //v3.x = EditorGUILayout.Slider("Foot Twist In-Out:", v3.x, -1f, 1f);
                //v3.y = EditorGUILayout.Slider("Foot Up-Down:", v3.y, -1f, 1f);
                //v3.z = EditorGUILayout.Slider("Toes Up-Down:", v3.z, -1f, 1f);
                //vFoot.SetValue(v3);
                //EditorGUI.indentLevel--;
                GUILayout.Space(4);
            }

            FGUI_Inspector.DrawUILineCommon();



        }


        #endregion


    }
}
