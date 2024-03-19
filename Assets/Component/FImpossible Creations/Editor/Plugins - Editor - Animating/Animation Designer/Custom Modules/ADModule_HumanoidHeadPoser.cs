using FIMSpace.FEditor;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.AnimationTools.CustomModules
{
    public class ADModule_HumanoidHeadPoser : ADHumanoidMuclesModuleBase
    {

        public override string ModuleTitleName { get { return "Humanoid/Head Poser"; } }
        public override bool GUIFoldable
        {
            get { return true; }
        }

        public override void HumanoidChanges(Animator mecanim, float animationProgress, float deltaTime, AnimationDesignerSave s, ADClipSettings_Main anim_MainSet, ADClipSettings_CustomModules customModules, ADClipSettings_CustomModules.CustomModuleSet set)
        {
            float blend = GetEvaluatedBlend(set, animationProgress);
            int startI;


            var vAddit = GetVariable("Additive", set, true);
            bool additive = vAddit.GetBoolValue();



            // -------------------------

            float tgtBlend = blend;

            var contr = GetVariable("Neck", set, Vector3.zero);
            startI = MID_NeckNodDownUp;
            ApplyMusclesV3(startI, contr.GetVector3Value(), tgtBlend * GetVariable("NeckB", set, 1f).Float, additive);

            tgtBlend = blend;
            contr = GetVariable("Head", set, Vector3.zero);
            startI = MID_HeadNodDownUp;
            ApplyMusclesV3(startI, contr.GetVector3Value(), tgtBlend * GetVariable("HeadB", set, 1f).Float, additive);

            tgtBlend = blend;
            contr = GetVariable("Jaw", set, Vector2.zero);
            startI = MID_JawClose;
            ApplyMusclesV2(startI, contr.GetVector2Value(), tgtBlend * GetVariable("JawB", set, 1f).Float, additive);


            contr = GetVariable("EyeL", set, Vector2.zero);

            if (contr.GetVector2Value() != Vector2.zero)
            {
                startI = MID_LeftEyeDownUp;
                ApplyMusclesV2(startI, contr.GetVector2Value(), tgtBlend, additive);
            }

            contr = GetVariable("EyeR", set, Vector2.zero);

            if (contr.GetVector2Value() != Vector2.zero)
            {
                startI = MID_RightEyeDownUp;
                ApplyMusclesV2(startI, contr.GetVector2Value(), tgtBlend, additive);
            }
        }


        public override void InspectorGUI_HeaderFoldown(ADClipSettings_CustomModules.CustomModuleSet customModuleSet)
        {
            base.InspectorGUI_HeaderFoldown(customModuleSet);

            GUILayout.Space(4);
            var contr = GetVariable("EyeL", customModuleSet, Vector2.zero);
            contr.DrawGUI();
            contr = GetVariable("EyeR", customModuleSet, Vector2.zero);
            contr.DrawGUI();
            GUILayout.Space(4);
        }


        #region Editor GUI Related Code

        public override void InspectorGUI_ModuleBody(float optionalBlendGhost, ADClipSettings_Main _anim_MainSet, AnimationDesignerSave s, ADClipSettings_CustomModules cModule, ADClipSettings_CustomModules.CustomModuleSet set)
        {
            if (GUI_CheckForHumanoidAnimator(s, _anim_MainSet) == false) return;

            var vAddit = GetVariable("Additive", set, true);
            vAddit.Tooltip = "Use additive blend or override muscle value with still value";
            vAddit.GUISpacing = new Vector2(4, 4);
            vAddit.HideFlag = true;

            vAddit.DrawGUI();

            FGUI_Inspector.DrawUILineCommon();

            var vNeck = GetVariable("Neck", set, Vector3.zero);
            vNeck.DisplayName = "Neck Controls";

            var vNeckBlend = GetVariable("NeckB", set, 1f);
            vNeckBlend.SetRangeHelperValue(new Vector2(0f, 1f));
            vNeckBlend.DisplayName = "Neck Blend";
            vNeckBlend.HideFlag = vAddit.GetBoolValue();


            var vHead = GetVariable("Head", set, Vector3.zero);
            vHead.DisplayName = "Head Controls";

            var vHeadBlend = GetVariable("HeadB", set, 1f);
            vHeadBlend.SetRangeHelperValue(new Vector2(0f, 1f));
            vHeadBlend.DisplayName = "Head Blend";
            vHeadBlend.HideFlag = vAddit.GetBoolValue();


            var vJaw = GetVariable("Jaw", set, Vector2.zero);
            vJaw.DisplayName = "Jaw Controls";

            var vJawBlend = GetVariable("JawB", set, 0f);
            vJawBlend.SetRangeHelperValue(new Vector2(0f, 1f));
            vJawBlend.DisplayName = "Jaw Blend";
            vJawBlend.HideFlag = vAddit.GetBoolValue();

            var vEyeL = GetVariable("EyeL", set, Vector2.zero);
            vEyeL.DisplayName = "Left Eye Controls";
            vEyeL.HideFlag = true;

            var vEyeR = GetVariable("EyeR", set, Vector2.zero);
            vEyeR.DisplayName = "Right Eye Controls";
            vEyeR.HideFlag = true;

            //base.InspectorGUI_ModuleBody(optionalBlendGhost, _anim_MainSet, s, cModule, set);
            Vector3 v3;


            vNeckBlend.Float = EditorGUILayout.Slider("Neck Blend:", vNeckBlend.Float, 0f, 1f);

            if (vNeckBlend.Float > 0f)
            {
                GUILayout.Space(4);
                EditorGUI.indentLevel++;
                v3 = vNeck.GetVector3Value();
                v3.x = EditorGUILayout.Slider("Neck Nod Down-Up:", v3.x, -1f, 1f);
                v3.y = EditorGUILayout.Slider("Neck Tilt Left-Right:", v3.y, -1f, 1f);
                v3.z = EditorGUILayout.Slider("Neck Turn Left-Right:", v3.z, -1f, 1f);
                vNeck.SetValue(v3);
                EditorGUI.indentLevel--;
            }

            FGUI_Inspector.DrawUILineCommon();

            vHeadBlend.Float = EditorGUILayout.Slider("Head Blend:", vHeadBlend.Float, 0f, 1f);

            if (vHeadBlend.Float > 0f)
            {
                GUILayout.Space(4);
                EditorGUI.indentLevel++;
                v3 = vHead.GetVector3Value();
                v3.x = EditorGUILayout.Slider("Head Nod Down-Up:", v3.x, -1f, 1f);
                v3.y = EditorGUILayout.Slider("Head Tilt Left-Right:", v3.y, -1f, 1f);
                v3.z = EditorGUILayout.Slider("Head Turn Left-Right:", v3.z, -1f, 1f);
                vHead.SetValue(v3);
                EditorGUI.indentLevel--;
            }

            FGUI_Inspector.DrawUILineCommon();

            vJawBlend.Float = EditorGUILayout.Slider("Jaw Blend:", vJawBlend.Float, 0f, 1f);

            if (vJawBlend.Float > 0f)
            {
                EditorGUI.indentLevel++;
                Vector2 v2 = vJaw.GetVector2Value();
                v2.x = EditorGUILayout.Slider("Jaw Close:", v2.x, -1f, 1f);
                v2.y = EditorGUILayout.Slider("Jaw Left-Right:", v2.y, -1f, 1f);
                vJaw.SetValue(v2);
                EditorGUI.indentLevel--;
            }

        }


        #endregion


    }
}
