using FIMSpace.FEditor;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.AnimationTools.CustomModules
{
    public class ADModule_HumanoidArmPoser : ADHumanoidMuclesModuleBase
    {
        public float SlidersRanges = 1f;
        public override string ModuleTitleName { get { return "Humanoid/Arm Poser"; } }


        public override void HumanoidChanges(Animator mecanim, float animationProgress, float deltaTime, AnimationDesignerSave s, ADClipSettings_Main anim_MainSet, ADClipSettings_CustomModules customModules, ADClipSettings_CustomModules.CustomModuleSet set)
        {
            float blend = GetEvaluatedBlend(set, animationProgress);

            var vR = GetVariable("Right", set, true);
            bool right = vR.GetBoolValue();

            var vL = GetVariable("Left", set, false);
            bool left = vL.GetBoolValue();

            var vAddit = GetVariable("Additive", set, true);
            bool additive = vAddit.GetBoolValue();

            // -------------------------

            if (right) ApplyMusclesToArm(true, set, blend, animationProgress, additive);
            if (left) ApplyMusclesToArm(false, set, blend, animationProgress, additive);
        }


        void ApplyMusclesToArm(bool right, ADClipSettings_CustomModules.CustomModuleSet set, float blend, float animationProgress, bool additive)
        {
            var contr = GetVariable("UpArm", set, Vector3.zero);
            int startI = right ? MID_RightArmDownUp : MID_LeftArmDownUp;
            float tgtBlend = blend;

            var vBlend = GetVariable("UpArmB", set, 1f);
            tgtBlend *= vBlend.Float * vBlend.GetBlendEvaluation(animationProgress);
            ApplyMusclesV3(startI, contr.GetVector3Value(), tgtBlend, additive);

            tgtBlend = blend;
            vBlend = GetVariable("LowArmB", set, 1f);
            tgtBlend *= vBlend.Float * vBlend.GetBlendEvaluation(animationProgress);
            contr = GetVariable("LowArm", set, Vector2.zero);
            startI = right ? MID_RightForearmStretch : MID_LeftForearmStretch;
            ApplyMusclesV2(startI, contr.GetVector2Value(), tgtBlend, additive);

            tgtBlend = blend;
            vBlend = GetVariable("HandB", set, 1f);
            tgtBlend *= vBlend.Float * vBlend.GetBlendEvaluation(animationProgress);
            contr = GetVariable("Hand", set, Vector2.zero);
            startI = right ? MID_RightHandDownUp : MID_LeftHandDownUp;
            ApplyMusclesV2(startI, contr.GetVector2Value(), tgtBlend, additive);

            tgtBlend = blend;
            vBlend = GetVariable("ShldB", set, 1f);
            tgtBlend *= vBlend.Float * vBlend.GetBlendEvaluation(animationProgress);
            contr = GetVariable("Shld", set, Vector2.zero);
            startI = right ? MID_RightShoulderDownUp : MID_LeftShoulderDownUp;
            ApplyMusclesV2(startI, contr.GetVector2Value(), tgtBlend, additive);
        }


        public enum EDisplayMode
        {
            None, Shoulder, UpperArm, LowerArm, Hand
        }

        public EDisplayMode Display = EDisplayMode.UpperArm;
        protected bool rightEnabled = false;
        protected bool leftEnabled = false;





        #region Editor GUI Related Code

        public override void InspectorGUI_ModuleBody(float optionalBlendGhost, ADClipSettings_Main _anim_MainSet, AnimationDesignerSave s, ADClipSettings_CustomModules cModule, ADClipSettings_CustomModules.CustomModuleSet set)
        {
            if (GUI_CheckForHumanoidAnimator(s, _anim_MainSet) == false) return;




            EditorGUILayout.BeginHorizontal();

            var vR = GetVariable("Right", set, true);
            vR.DisplayName = "Right Arm";
            vR.Tooltip = "Enable to use Right Arm";
            vR.DrawGUI();
            rightEnabled = vR.GetBoolValue();


            var vL = GetVariable("Left", set, false);
            vL.DisplayName = "Left Arm";
            vL.Tooltip = "Enable to use Left Arm";
            vL.DrawGUI();
            leftEnabled = vL.GetBoolValue();


            var vAddit = GetVariable("Additive", set, true);
            vAddit.Tooltip = "Use additive blend or override muscle value with still value";
            vAddit.GUISpacing = new Vector2(0, 6);

            GUILayout.FlexibleSpace();
            vAddit.DrawGUI();

            EditorGUILayout.EndHorizontal();



            FGUI_Inspector.DrawUILineCommon(8);



            EditorGUILayout.BeginHorizontal();

            if (Display == EDisplayMode.Shoulder) GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Shoulder")) { Display = EDisplayMode.Shoulder; }
            if (Display == EDisplayMode.UpperArm) GUI.backgroundColor = Color.green; else GUI.backgroundColor = Color.white;
            if (GUILayout.Button("Upper arm")) { Display = EDisplayMode.UpperArm; }
            if (Display == EDisplayMode.LowerArm) GUI.backgroundColor = Color.green; else GUI.backgroundColor = Color.white;
            if (GUILayout.Button("Lower arm")) { Display = EDisplayMode.LowerArm; }
            if (Display == EDisplayMode.Hand) GUI.backgroundColor = Color.green; else GUI.backgroundColor = Color.white;
            if (GUILayout.Button("Hand")) { Display = EDisplayMode.Hand; }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndHorizontal();



            #region Define all variables

            // Controls
            var vUpArm = GetVariable("UpArm", set, Vector3.zero);
            vUpArm.DisplayName = "Upper Arm Controls";
            vUpArm.Tooltip = "X: UpperLeg FrontBack\nY: UpperLeg InOut\nZ: UpperLeg TwistInOut";

            // Blend
            var vUpArmB = GetVariable("UpArmB", set, 1f);
            vUpArmB.SetRangeHelperValue(new Vector2(0f, 1f));
            vUpArmB.DisplayName = "Upper Arm Blend";
            vUpArmB._BlendingCurveRanges = new Vector4(0f, 0f, 1f, 1f);
            vUpArmB.DisplayBlendingCurve = true;


            // Controls
            var vLowArm = GetVariable("LowArm", set, Vector2.zero);
            vLowArm.DisplayName = "Lower Arm Controls";

            // Blend
            var vLowArmB = GetVariable("LowArmB", set, 1f);
            vLowArmB.SetRangeHelperValue(new Vector2(0f, 1f));
            vLowArmB.DisplayName = "Lower Arm Blend";
            vLowArmB._BlendingCurveRanges = new Vector4(0f, 0f, 1f, 1f);
            vLowArmB.DisplayBlendingCurve = true;

            // Controls
            var vHand = GetVariable("Hand", set, Vector2.zero);
            vHand.DisplayName = "Hand Controls";

            // Blend
            var vHandB = GetVariable("HandB", set, 1f);
            vHandB.SetRangeHelperValue(new Vector2(0f, 1f));
            vHandB.DisplayName = "Hand Blend";
            vHandB._BlendingCurveRanges = new Vector4(0f, 0f, 1f, 1f);
            vHandB.DisplayBlendingCurve = true;

            // Controls
            var vShld = GetVariable("Shld", set, Vector2.zero);
            vShld.DisplayName = "Shoulder Controls";

            // Blend
            var vShldB = GetVariable("ShldB", set, 1f);
            vShldB.SetRangeHelperValue(new Vector2(0f, 1f));
            vShldB.DisplayName = "Shoulder Blend";
            vShldB._BlendingCurveRanges = new Vector4(0f, 0f, 1f, 1f);
            vShldB.DisplayBlendingCurve = true;

            #endregion

            bool drawBlend = true; // if (!vAddit.GetBoolValue())

            GUILayout.Space(6);
            EditorGUILayout.LabelField(Display.ToString(), FGUI_Resources.HeaderStyle);
            GUILayout.Space(4);

            if (Display == EDisplayMode.UpperArm)
            {
                Vector2 val = vUpArm.GetVector2Value();
                val.x = EditorGUILayout.Slider("Upper Arm Stretch", vUpArm.GetVector2Value().x, -SlidersRanges, SlidersRanges);
                val.y = EditorGUILayout.Slider("Upper Arm Twist In Out", vUpArm.GetVector2Value().y, -SlidersRanges, SlidersRanges);
                vUpArm.SetValue(val);
            }

            if (Display == EDisplayMode.UpperArm) if (drawBlend) vUpArmB.DrawGUI();



            if (Display == EDisplayMode.LowerArm)
            {
                Vector2 val = vLowArm.GetVector2Value();
                val.x = EditorGUILayout.Slider("Lower Arm Stretch", vLowArm.GetVector2Value().x, -SlidersRanges, SlidersRanges);
                val.y = EditorGUILayout.Slider("Lower Arm Twist In Out", vLowArm.GetVector2Value().y, -SlidersRanges, SlidersRanges);
                vLowArm.SetValue(val);
            }

            if (Display == EDisplayMode.LowerArm) if (drawBlend) vLowArmB.DrawGUI();

            if (Display == EDisplayMode.Hand)
            {
                Vector2 val = vHand.GetVector2Value();
                val.x = EditorGUILayout.Slider("Hand Up Down", vHand.GetVector2Value().x, -SlidersRanges, SlidersRanges);
                val.y = EditorGUILayout.Slider("Hand In Out", vHand.GetVector2Value().y, -SlidersRanges, SlidersRanges);
                vHand.SetValue(val);
            }


            if (Display == EDisplayMode.Hand) if (drawBlend) vHandB.DrawGUI();

            if (Display == EDisplayMode.Shoulder)
            {
                Vector2 val = vShld.GetVector2Value();
                val.x = EditorGUILayout.Slider("Shoulder Down-Up", vShld.GetVector2Value().x, -SlidersRanges, SlidersRanges);
                val.y = EditorGUILayout.Slider("Shoulder Front-Back", vShld.GetVector2Value().y, -SlidersRanges, SlidersRanges);
                vShld.SetValue(val);
            }

            if (Display == EDisplayMode.Shoulder) if (drawBlend) vShldB.DrawGUI();
            //base.InspectorGUI_ModuleBody(optionalBlendGhost, _anim_MainSet, s, cModule, set);

        }


        #endregion




        #region Gizmos



        public override void SceneView_DrawSceneHandles(ADClipSettings_CustomModules.CustomModuleSet customModuleSet, float alphaAnimation = 1, float progress = 0f )
        {
            if (S == null) return;
            if (LastHumanoidAnimator == null) return;

            if (rightEnabled) DrawArm(true, alphaAnimation);
            if (leftEnabled) DrawArm(false, alphaAnimation);
        }


        void DrawArm(bool right, float alphaAnimation = 1)
        {
            Transform pre = GetShoulder(right);
            Transform next = GetUpperArm(right);

            float alpha = 0.5f;

            if (pre && next)
            {
                if (Display == EDisplayMode.Shoulder) alpha = 0.9f;

                Handles.color = new Color(0.8f, 0.3f, 0.1f, alpha + alphaAnimation * 0.2f);
                AnimationDesignerWindow.DrawBoneHandle(pre.position, next.position, .5f + alpha * alphaAnimation, true, 4f * alpha + alphaAnimation);
            }

            pre = next;
            next = GetLowerArm(right);
            alpha = 0.5f;

            if (pre && next)
            {
                if (Display == EDisplayMode.UpperArm) alpha = 0.9f;
                Handles.color = new Color(0.8f, 0.3f, 0.1f, alpha + alphaAnimation * 0.2f);
                AnimationDesignerWindow.DrawBoneHandle(pre.position, next.position, .5f + alpha * alphaAnimation, true, 4f * alpha + alphaAnimation);
            }


            pre = next;
            next = GetHand(right);
            alpha = 0.5f;

            if (pre && next)
            {
                if (Display == EDisplayMode.LowerArm) alpha = 0.9f;
                Handles.color = new Color(0.8f, 0.3f, 0.1f, alpha + alphaAnimation * 0.2f);
                AnimationDesignerWindow.DrawBoneHandle(pre.position, next.position, .5f + alpha * alphaAnimation, true, 4f * alpha + alphaAnimation);
            }
            else
            {
                return;
            }

            Vector3 toHandDir = next.position - pre.position;
            alpha = 0.5f;

            if (pre && next)
            {
                if (Display == EDisplayMode.Hand) alpha = 0.9f;
                Handles.color = new Color(0.8f, 0.3f, 0.1f, alpha + alphaAnimation * 0.2f);
                AnimationDesignerWindow.DrawBoneHandle(next.position, next.position + toHandDir * 0.25f, .5f + alpha * alphaAnimation, true, 4f * alpha + alphaAnimation);
            }
        }

        #endregion



    }
}
