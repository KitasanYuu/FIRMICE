using FIMSpace.FEditor;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.AnimationTools.CustomModules
{
    public class ADModule_HumanoidSpinePoserAllParams : ADHumanoidMuclesModuleBase
    {

        public override string ModuleTitleName { get { return "Humanoid/All Params/Spine Poser-All Params"; } }
        public override bool GUIFoldable { get { return true; } }

        public override void OnInheritElasticnessUpdate(float animationProgress, float deltaTime, AnimationDesignerSave s, ADClipSettings_Main anim_MainSet, ADClipSettings_CustomModules customModules, ADClipSettings_CustomModules.CustomModuleSet set)
        {
            var vInh = GetVariable("Inh", set, true);
            InheritElasticness = vInh.GetBoolValue();
            base.OnInheritElasticnessUpdate(animationProgress, deltaTime, s, anim_MainSet, customModules, set);
        }

        public override void InspectorGUI_HeaderFoldown(ADClipSettings_CustomModules.CustomModuleSet customModuleSet)
        {
            base.InspectorGUI_HeaderFoldown(customModuleSet);

            var curveVar = GetVariable("VCurve", customModuleSet, AnimationCurve.EaseInOut(0f, 1f, 1f, 1f));
            curveVar.HideFlag = true;
            curveVar.DrawGUI();
        }

        public override void HumanoidChanges(Animator mecanim, float progr, float deltaTime, AnimationDesignerSave s, ADClipSettings_Main anim_MainSet, ADClipSettings_CustomModules customModules, ADClipSettings_CustomModules.CustomModuleSet set)
        {
            float blend = GetEvaluatedBlend(set, progr);
            int startI;

            var vAddit = GetVariable("Additive", set, true);
            bool additive = vAddit.GetBoolValue();

            float curveMul = 1f;
            var curveVar = GetVariable("VCurve", set, AnimationCurve.EaseInOut(0f, 1f, 1f, 1f));
            if (curveVar != null) curveMul = curveVar.GetCurve().Evaluate(progr);

            // -------------------------

            float tgtBlend = blend;
            tgtBlend *= GetVariable("SpineB", set, 1f).Float;

            //var contr = GetVariable("Spine", set, Vector3.zero);
            startI = MID_SpineFrontBack;

            if (tgtBlend > 0f)
                ApplyMusclesV3(startI, GetVariable("S_FB", set).GetBlendedFloat(progr) * curveMul, GetVariable("S_LR", set).GetBlendedFloat(progr) * curveMul, GetVariable("S_TW", set).GetBlendedFloat(progr) * curveMul,
                    tgtBlend, additive);

            tgtBlend = blend;
            tgtBlend *= GetVariable("ChestB", set, 1f).Float;
            //contr = GetVariable("Chest", set, Vector3.zero);
            startI = MID_ChestFrontBack;

            if (tgtBlend > 0f)
                ApplyMusclesV3(startI, GetVariable("CH_FB", set).GetBlendedFloat(progr) * curveMul, GetVariable("CH_LR", set).GetBlendedFloat(progr) * curveMul, GetVariable("CH_TW", set).GetBlendedFloat(progr) * curveMul,
                    tgtBlend, additive);

            tgtBlend = blend;
            tgtBlend *= GetVariable("UpChestB", set, 1f).Float;
            //contr = GetVariable("UpChest", set, Vector3.zero);
            startI = MID_UpperChestFrontBack;

            if (tgtBlend > 0f)
                ApplyMusclesV3(startI, GetVariable("UC_FB", set).GetBlendedFloat(progr) * curveMul, GetVariable("UC_LR", set).GetBlendedFloat(progr) * curveMul, GetVariable("UC_TW", set).GetBlendedFloat(progr) * curveMul,
                    tgtBlend, additive);

        }



        #region Editor GUI Related Code

        public override void InspectorGUI_ModuleBody(float optionalBlendGhost, ADClipSettings_Main _anim_MainSet, AnimationDesignerSave s, ADClipSettings_CustomModules cModule, ADClipSettings_CustomModules.CustomModuleSet set)
        {
            if (GUI_CheckForHumanoidAnimator(s, _anim_MainSet) == false) return;



            EditorGUILayout.BeginHorizontal();

            var vInh = GetVariable("Inh", set, true);
            vInh.DisplayName = "Inherit Elasticity";
            vInh.Tooltip = "Disable to achieve different effect when using leg IK!";
            vInh.DrawGUI();

            var vAddit = GetVariable("Additive", set, true);
            vAddit.Tooltip = "Use additive blend or override muscle value with still value";
            vAddit.GUISpacing = new Vector2(0, 6);

            GUILayout.FlexibleSpace();
            vAddit.DrawGUI();

            EditorGUILayout.EndHorizontal();

            FGUI_Inspector.DrawUILineCommon();



            //var vSpine = GetVariable("Spine", set, Vector3.zero);
            //vSpine.DisplayName = "Lower Spine Controls";
            //vSpine.Tooltip = "X: Front-Back\nY: Left-Right\nZ: Twist";

            var vSpineB = GetVariable("SpineB", set, 1f);
            vSpineB.SetRangeHelperValue(new Vector2(0f, 1f));
            vSpineB.DisplayName = "Lower Spine Blend";
            vSpineB.HideFlag = vAddit.GetBoolValue();

            var vLSpineFB = GetVariable("S_FB", set, 0f); vLSpineFB.DisplayName = "   Lower Spine Front-Back:"; vLSpineFB.SetRangeHelperValue(-1f, 1f);
            var vLSpineIO = GetVariable("S_LR", set, 0f); vLSpineIO.DisplayName = "   Lower Spine Left-Right:"; vLSpineIO.SetRangeHelperValue(-1f, 1f);
            var vLSpineTW = GetVariable("S_TW", set, 0f); vLSpineTW.DisplayName = "   Lower Spine Twist In-Out:"; vLSpineTW.SetRangeHelperValue(-1f, 1f);


            //Vector3 v3;
            vSpineB.Float = EditorGUILayout.Slider("Lower Spine Blend:", vSpineB.Float, 0f, 1f);

            if (vSpineB.Float > 0f)
            {
                vLSpineFB._BlendingCurveRanges = new Vector4(0f, 0f, 1f, 1f); vLSpineFB.DisplayBlendingCurve = true;
                vLSpineIO._BlendingCurveRanges = new Vector4(0f, 0f, 1f, 1f); vLSpineIO.DisplayBlendingCurve = true;
                vLSpineTW._BlendingCurveRanges = new Vector4(0f, 0f, 1f, 1f); vLSpineTW.DisplayBlendingCurve = true;


                GUILayout.Space(4);

                vLSpineFB.DrawGUI();
                vLSpineIO.DrawGUI();
                vLSpineTW.DrawGUI();
                //EditorGUI.indentLevel++;
                //v3 = vSpine.GetVector3Value();
                //v3.x = EditorGUILayout.Slider("Spine Front-Back:", v3.x, -1f, 1f);
                //v3.y = EditorGUILayout.Slider("Spine Left-Right:", v3.y, -1f, 1f);
                //v3.z = EditorGUILayout.Slider("Spine Twist:", v3.z, -1f, 1f);
                //vSpine.SetValue(v3);
                //EditorGUI.indentLevel--;
                GUILayout.Space(4);
            }

            FGUI_Inspector.DrawUILineCommon();


            //var vChest = GetVariable("Chest", set, Vector3.zero);
            //vChest.DisplayName = "Chest Controls";
            //vChest.Tooltip = "X: Front-Back\nY: Left-Right\nZ: Twist";

            var vChestB = GetVariable("ChestB", set, 1f);
            vChestB.SetRangeHelperValue(new Vector2(0f, 1f));
            vChestB.DisplayName = "Chest Blend";
            vChestB.HideFlag = vAddit.GetBoolValue();

            var vChestFB = GetVariable("CH_FB", set, 0f); vChestFB.DisplayName = "   Chest Front-Back:"; vChestFB.SetRangeHelperValue(-1f, 1f);
            var vChestLR = GetVariable("CH_LR", set, 0f); vChestLR.DisplayName = "   Chest Left-Right:"; vChestLR.SetRangeHelperValue(-1f, 1f);
            var vChestTW = GetVariable("CH_TW", set, 0f); vChestTW.DisplayName = "   Chest Twist In-Out:"; vChestTW.SetRangeHelperValue(-1f, 1f);



            vChestB.Float = EditorGUILayout.Slider("Chest Blend:", vChestB.Float, 0f, 1f);

            if (vChestB.Float > 0f)
            {
                vChestFB._BlendingCurveRanges = new Vector4(0f, 0f, 1f, 1f); vChestFB.DisplayBlendingCurve = true;
                vChestLR._BlendingCurveRanges = new Vector4(0f, 0f, 1f, 1f); vChestLR.DisplayBlendingCurve = true;
                vChestTW._BlendingCurveRanges = new Vector4(0f, 0f, 1f, 1f); vChestTW.DisplayBlendingCurve = true;

                GUILayout.Space(4);
                vChestFB.DrawGUI();
                vChestLR.DrawGUI();
                vChestTW.DrawGUI();
                //EditorGUI.indentLevel++;
                //v3 = vChest.GetVector3Value();
                //v3.x = EditorGUILayout.Slider("Chest Front-Back:", v3.x, -1f, 1f);
                //v3.y = EditorGUILayout.Slider("Chest Left-Right:", v3.y, -1f, 1f);
                //v3.z = EditorGUILayout.Slider("Chest Twist:", v3.z, -1f, 1f);
                //vChest.SetValue(v3);
                //EditorGUI.indentLevel--;
                GUILayout.Space(4);
            }

            FGUI_Inspector.DrawUILineCommon();



            //var vUpChest = GetVariable("UpChest", set, Vector3.zero);
            //vUpChest.DisplayName = "Upper Chest Controls";
            //vUpChest.Tooltip = "X: Front-Back\nY: Left-Right\nZ: Twist";

            var vUpChestB = GetVariable("UpChestB", set, 1f);
            vUpChestB.SetRangeHelperValue(new Vector2(0f, 1f));
            vUpChestB.DisplayName = "Upper Chest Blend";
            vUpChestB.HideFlag = vAddit.GetBoolValue();

            var vUChestFB = GetVariable("UC_FB", set, 0f); vUChestFB.DisplayName = "   Upper Chest Front-Back:"; vUChestFB.SetRangeHelperValue(-1f, 1f);
            var vUChestLR = GetVariable("UC_LR", set, 0f); vUChestLR.DisplayName = "   Upper Chest Left-Right:"; vUChestLR.SetRangeHelperValue(-1f, 1f);
            var vUChestTW = GetVariable("UC_TW", set, 0f); vUChestTW.DisplayName = "   Upper Chest Twist In-Out:"; vUChestTW.SetRangeHelperValue(-1f, 1f);



            vUpChestB.Float = EditorGUILayout.Slider("Up Chest Blend:", vUpChestB.Float, 0f, 1f);

            if (vUpChestB.Float > 0f)
            {
                vUChestFB._BlendingCurveRanges = new Vector4(0f, 0f, 1f, 1f); vUChestFB.DisplayBlendingCurve = true;
                vUChestLR._BlendingCurveRanges = new Vector4(0f, 0f, 1f, 1f); vUChestLR.DisplayBlendingCurve = true;
                vUChestTW._BlendingCurveRanges = new Vector4(0f, 0f, 1f, 1f); vUChestTW.DisplayBlendingCurve = true;

                GUILayout.Space(4);
                vUChestFB.DrawGUI();
                vUChestLR.DrawGUI();
                vUChestTW.DrawGUI();
                //EditorGUI.indentLevel++;
                //v3 = vUpChest.GetVector3Value();
                //v3.x = EditorGUILayout.Slider("Up Chest Front-Back:", v3.x, -1f, 1f);
                //v3.y = EditorGUILayout.Slider("Up Chest Left-Right:", v3.y, -1f, 1f);
                //v3.z = EditorGUILayout.Slider("Up Chest Twist:", v3.z, -1f, 1f);
                //vUpChest.SetValue(v3);
                //EditorGUI.indentLevel--;
                GUILayout.Space(4);
            }

            //base.InspectorGUI_ModuleBody(optionalBlendGhost, _anim_MainSet, s, cModule, set);

            var curveVar = GetVariable("VCurve", set, AnimationCurve.EaseInOut(0f, 1f, 1f, 1f));
            curveVar.SetRangeHelperValue(new Vector4(0f, -1f, 1f, 1f));
            curveVar.DisplayName = "Parameters Value Curve";
            curveVar.HideFlag = true;

        }


        #endregion


    }
}
