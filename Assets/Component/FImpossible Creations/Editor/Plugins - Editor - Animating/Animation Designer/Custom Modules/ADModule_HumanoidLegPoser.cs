using FIMSpace.FEditor;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.AnimationTools.CustomModules
{
    public class ADModule_HumanoidLegPoser : ADHumanoidMuclesModuleBase
    {

        public override string ModuleTitleName { get { return "Humanoid/Leg Poser"; } }


        public override void HumanoidChanges(Animator mecanim, float animationProgress, float deltaTime, AnimationDesignerSave s, ADClipSettings_Main anim_MainSet, ADClipSettings_CustomModules customModules, ADClipSettings_CustomModules.CustomModuleSet set)
        {
            float blend = GetEvaluatedBlend(set, animationProgress);
            int startI;

            var vR = GetVariable("Right", set, true);
            bool right = vR.GetBoolValue();


            var vAddit = GetVariable("Additive", set, true);
            bool additive = vAddit.GetBoolValue();


            // -------------------------

            float tgtBlend = blend;
            tgtBlend *= GetVariable("UpLegB", set, 1f).Float;

            var contr = GetVariable("UpLeg", set, Vector3.zero);
            startI = right ? MID_RightUpperLegFrontBack : MID_LeftUpperLegFrontBack;
            ApplyMusclesV3(startI, contr.GetVector3Value(), tgtBlend, additive);

            tgtBlend = blend;
            tgtBlend *= GetVariable("LowLegB", set, 1f).Float;
            contr = GetVariable("LowLeg", set, Vector2.zero);
            startI = right ? MID_RightLowerLegStretch : MID_LeftLowerLegStretch;
            ApplyMusclesV2(startI, contr.GetVector2Value(), tgtBlend, additive);

            tgtBlend = blend;
            tgtBlend *= GetVariable("FootB", set, 1f).Float;
            contr = GetVariable("Foot", set, Vector3.zero);
            startI = right ? MID_RightFootUpDown : MID_LeftFootUpDown;
            ApplyMusclesV3(startI, contr.GetVector3Value(), tgtBlend, additive);

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


            var vUpLeg = GetVariable("UpLeg", set, Vector3.zero);
            vUpLeg.DisplayName = "Upper Leg Controls";
            vUpLeg.Tooltip = "X: UpperLeg FrontBack\nY: UpperLeg InOut\nZ: UpperLeg TwistInOut";

            var vUpLegBlend = GetVariable("UpLegB", set, 1f);
            vUpLegBlend.SetRangeHelperValue(new Vector2(0f, 1f));
            vUpLegBlend.DisplayName = "Upper Leg Blend";
            vUpLegBlend.HideFlag = vAddit.GetBoolValue();


            var vLowLeg = GetVariable("LowLeg", set, Vector2.zero);
            vLowLeg.DisplayName = "Lower Leg Controls";
            vLowLeg.Tooltip = "X: LowerLeg Stretch\nY: LowerLeg TwistInOut";

            var vLowLegBlend = GetVariable("LowLegB", set, 1f);
            vLowLegBlend.SetRangeHelperValue(new Vector2(0f, 1f));
            vLowLegBlend.DisplayName = "Lower Leg Blend";
            vLowLegBlend.HideFlag = vAddit.GetBoolValue();



            var vFoot = GetVariable("Foot", set, Vector3.zero);
            vFoot.DisplayName = "Foot Controls";
            vFoot.Tooltip = "X: Foot UpDown\nY: Foot TwistInOut\nZ: Toes UpDown";

            var vFootBlend = GetVariable("FootB", set, 1f);
            vFootBlend.SetRangeHelperValue(new Vector2(0f, 1f));
            vFootBlend.DisplayName = "Foot Blend";
            vFootBlend.HideFlag = vAddit.GetBoolValue();

            //base.InspectorGUI_ModuleBody(optionalBlendGhost, _anim_MainSet, s, cModule, set);


            Vector3 v3;

            vUpLegBlend.Float = EditorGUILayout.Slider("Upper Leg Blend:", vUpLegBlend.Float, 0f, 1f);

            if (vUpLegBlend.Float > 0f)
            {
                GUILayout.Space(4);
                EditorGUI.indentLevel++;
                v3 = vUpLeg.GetVector3Value();
                v3.x = EditorGUILayout.Slider("Upper Leg Front-Back:", v3.x, -1f, 1f);
                v3.y = EditorGUILayout.Slider("Upper Leg In Out:", v3.y, -1f, 1f);
                v3.z = EditorGUILayout.Slider("Upper Leg Twist In-Out:", v3.z, -1f, 1f);
                vUpLeg.SetValue(v3);
                EditorGUI.indentLevel--;
                GUILayout.Space(4);
            }

            FGUI_Inspector.DrawUILineCommon();


            vLowLegBlend.Float = EditorGUILayout.Slider("Lower Leg Blend:", vLowLegBlend.Float, 0f, 1f);

            if (vLowLegBlend.Float > 0f)
            {
                GUILayout.Space(4);
                EditorGUI.indentLevel++;
                Vector2 v2 = vLowLeg.GetVector2Value();
                v2.x = EditorGUILayout.Slider("Lower Leg Stretch:", v2.x, -1f, 1f);
                v2.y = EditorGUILayout.Slider("Lower Leg Twist In-Out:", v2.y, -1f, 1f);
                vLowLeg.SetValue(v2);
                EditorGUI.indentLevel--;
                GUILayout.Space(4);
            }

            FGUI_Inspector.DrawUILineCommon();


            vFootBlend.Float = EditorGUILayout.Slider("Foot Blend:", vFootBlend.Float, 0f, 1f);

            if (vFootBlend.Float > 0f)
            {
                GUILayout.Space(4);
                EditorGUI.indentLevel++;
                v3 = vFoot.GetVector3Value();
                v3.x = EditorGUILayout.Slider("Foot Twist In-Out:", v3.x, -1f, 1f);
                v3.y = EditorGUILayout.Slider("Foot Up-Down:", v3.y, -1f, 1f);
                v3.z = EditorGUILayout.Slider("Toes Up-Down:", v3.z, -1f, 1f);
                vFoot.SetValue(v3);
                EditorGUI.indentLevel--;
                GUILayout.Space(4);
            }

            FGUI_Inspector.DrawUILineCommon();



        }


        #endregion


    }
}
