using FIMSpace.FEditor;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.AnimationTools.CustomModules
{
    public class ADModule_HumanoidSpinePoser : ADHumanoidMuclesModuleBase
    {

        public override string ModuleTitleName { get { return "Humanoid/Spine Poser"; } }
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

        public override void HumanoidChanges(Animator mecanim, float animationProgress, float deltaTime, AnimationDesignerSave s, ADClipSettings_Main anim_MainSet, ADClipSettings_CustomModules customModules, ADClipSettings_CustomModules.CustomModuleSet set)
        {
            float blend = GetEvaluatedBlend(set, animationProgress);
            int startI;

            var vAddit = GetVariable("Additive", set, true);
            bool additive = vAddit.GetBoolValue();

            float curveMul = 1f;
            var curveVar = GetVariable("VCurve", set, AnimationCurve.EaseInOut(0f, 1f, 1f, 1f));
            if (curveVar != null) curveMul = curveVar.GetCurve().Evaluate(animationProgress);
            
            // -------------------------

            float tgtBlend = blend;
            tgtBlend *= GetVariable("SpineB", set, 1f).Float;

            var contr = GetVariable("Spine", set, Vector3.zero);
            startI = MID_SpineFrontBack;
            ApplyMusclesV3(startI, contr.GetVector3Value() * curveMul, tgtBlend, additive);

            tgtBlend = blend;
            tgtBlend *= GetVariable("ChestB", set, 1f).Float;
            contr = GetVariable("Chest", set, Vector3.zero);
            startI = MID_ChestFrontBack;
            ApplyMusclesV3(startI, contr.GetVector3Value() * curveMul, tgtBlend, additive);

            tgtBlend = blend;
            tgtBlend *= GetVariable("UpChestB", set, 1f).Float;
            contr = GetVariable("UpChest", set, Vector3.zero);
            startI = MID_UpperChestFrontBack;
            ApplyMusclesV3(startI, contr.GetVector3Value() * curveMul, tgtBlend, additive);

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



            var vSpine = GetVariable("Spine", set, Vector3.zero);
            vSpine.DisplayName = "Lower Spine Controls";
            vSpine.Tooltip = "X: Front-Back\nY: Left-Right\nZ: Twist";

            var vSpineB = GetVariable("SpineB", set, 1f);
            vSpineB.SetRangeHelperValue(new Vector2(0f, 1f));
            vSpineB.DisplayName = "Lower Spine Blend";
            vSpineB.HideFlag = vAddit.GetBoolValue();


            Vector3 v3;
            vSpineB.Float = EditorGUILayout.Slider("Lower Spine Blend:", vSpineB.Float, 0f, 1f);

            if (vSpineB.Float > 0f)
            {
                GUILayout.Space(4);
                EditorGUI.indentLevel++;
                v3 = vSpine.GetVector3Value();
                v3.x = EditorGUILayout.Slider("Spine Front-Back:", v3.x, -1f, 1f);
                v3.y = EditorGUILayout.Slider("Spine Left-Right:", v3.y, -1f, 1f);
                v3.z = EditorGUILayout.Slider("Spine Twist:", v3.z, -1f, 1f);
                vSpine.SetValue(v3);
                EditorGUI.indentLevel--;
                GUILayout.Space(4);
            }

            FGUI_Inspector.DrawUILineCommon();


            var vChest = GetVariable("Chest", set, Vector3.zero);
            vChest.DisplayName = "Chest Controls";
            vChest.Tooltip = "X: Front-Back\nY: Left-Right\nZ: Twist";

            var vChestB = GetVariable("ChestB", set, 1f);
            vChestB.SetRangeHelperValue(new Vector2(0f, 1f));
            vChestB.DisplayName = "Chest Blend";
            vChestB.HideFlag = vAddit.GetBoolValue();


            vChestB.Float = EditorGUILayout.Slider("Chest Blend:", vChestB.Float, 0f, 1f);

            if (vChestB.Float > 0f)
            {
                GUILayout.Space(4);
                EditorGUI.indentLevel++;
                v3 = vChest.GetVector3Value();
                v3.x = EditorGUILayout.Slider("Chest Front-Back:", v3.x, -1f, 1f);
                v3.y = EditorGUILayout.Slider("Chest Left-Right:", v3.y, -1f, 1f);
                v3.z = EditorGUILayout.Slider("Chest Twist:", v3.z, -1f, 1f);
                vChest.SetValue(v3);
                EditorGUI.indentLevel--;
                GUILayout.Space(4);
            }

            FGUI_Inspector.DrawUILineCommon();



            var vUpChest = GetVariable("UpChest", set, Vector3.zero);
            vUpChest.DisplayName = "Upper Chest Controls";
            vUpChest.Tooltip = "X: Front-Back\nY: Left-Right\nZ: Twist";

            var vUpChestB = GetVariable("UpChestB", set, 1f);
            vUpChestB.SetRangeHelperValue(new Vector2(0f, 1f));
            vUpChestB.DisplayName = "Upper Chest Blend";
            vUpChestB.HideFlag = vAddit.GetBoolValue();


            vUpChestB.Float = EditorGUILayout.Slider("Up Chest Blend:", vUpChestB.Float, 0f, 1f);

            if (vUpChestB.Float > 0f)
            {
                GUILayout.Space(4);
                EditorGUI.indentLevel++;
                v3 = vUpChest.GetVector3Value();
                v3.x = EditorGUILayout.Slider("Up Chest Front-Back:", v3.x, -1f, 1f);
                v3.y = EditorGUILayout.Slider("Up Chest Left-Right:", v3.y, -1f, 1f);
                v3.z = EditorGUILayout.Slider("Up Chest Twist:", v3.z, -1f, 1f);
                vUpChest.SetValue(v3);
                EditorGUI.indentLevel--;
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
