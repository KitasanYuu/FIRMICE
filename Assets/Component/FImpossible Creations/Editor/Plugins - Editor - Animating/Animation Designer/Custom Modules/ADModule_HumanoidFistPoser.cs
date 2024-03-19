using FIMSpace.FEditor;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.AnimationTools.CustomModules
{
    public class ADModule_HumanoidFistPoser : ADHumanoidMuclesModuleBase
    {

        public override string ModuleTitleName { get { return "Humanoid/Fist Poser"; } }


        public override void HumanoidChanges(Animator mecanim, float animationProgress, float deltaTime, AnimationDesignerSave s, ADClipSettings_Main anim_MainSet, ADClipSettings_CustomModules customModules, ADClipSettings_CustomModules.CustomModuleSet set)
        {
            var vAddit = GetVariable("Additive", set, false);
            bool additive = vAddit.GetBoolValue();

            var vL = GetVariable("Left", set, true);
            var vR = GetVariable("Right", set, true);
            var vClosed = GetVariable("Closed", set, 0f);
            var vThumb = GetVariable("Thumbs", set, 0f);

            //UnityEngine.Debug.Log(mList);
            float closeFactor = vClosed.Float;
            float thumbValue = vThumb.Float;

            float blendVal = GetEvaluatedBlend(set, animationProgress);
            closeFactor *= blendVal;

            var vThumbSpr = GetVariable("ThumbSpr", set, 0f);
            float thumbSpr = vThumbSpr.Float * blendVal;

            // Left hand
            if (vL.GetBoolValue())
            {
                MHandOpenClose(false, -closeFactor, additive, blendVal);

                if (thumbValue != 0f) MHandThumbOpenClose(false, thumbValue, additive, blendVal);

                if (thumbSpr != 0f)
                {
                    if (additive)
                        MLeftThumbSpread += thumbSpr;
                    else
                        MLeftThumbSpread = Mathf.LerpUnclamped(MLeftThumbSpread, vThumbSpr.Float, blendVal);
                }
            }

            if (vR.GetBoolValue())
            {
                MHandOpenClose(true, -closeFactor, additive, blendVal);

                if (thumbValue != 0f) MHandThumbOpenClose(true, thumbValue, additive, blendVal);

                if (thumbSpr != 0f)
                {
                    if (additive)
                        MRightThumbSpread += thumbSpr;
                    else
                        MRightThumbSpread = Mathf.LerpUnclamped(MRightThumbSpread, vThumbSpr.Float, blendVal);
                }
            }


        }


        #region Editor GUI Related Code

        public override void InspectorGUI_ModuleBody(float optionalBlendGhost, ADClipSettings_Main _anim_MainSet, AnimationDesignerSave s, ADClipSettings_CustomModules cModule, ADClipSettings_CustomModules.CustomModuleSet set)
        {
            if (GUI_CheckForHumanoidAnimator(s, _anim_MainSet) == false) return;

            var vAddit = GetVariable("Additive", set, false);
            vAddit.Tooltip = "Apply fist humanoid pose additive or override";
            vAddit.DrawGUI();

            var vL = GetVariable("Left", set, true);
            if (string.IsNullOrWhiteSpace(vL.DisplayName))
                vL.DisplayName = "Use Left Hand";
            var vR = GetVariable("Right", set, true);
            if (string.IsNullOrWhiteSpace(vR.DisplayName))
                vR.DisplayName = "Use Right Hand";

            var vClosed = GetVariable("Closed", set, 0f);
            vClosed.SetRangeHelperValue(new Vector2(-1f, 1f));
            vClosed.GUISpacing = new Vector2(6f, 0);

            EditorGUILayout.BeginHorizontal();
            vL.DrawGUI();
            GUILayout.FlexibleSpace();
            vR.DrawGUI();
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(4);
            FGUI_Inspector.DrawUILineCommon();

            vClosed.DrawGUI();

            var vThumb = GetVariable("Thumbs", set, 0f);
            vThumb.DisplayName = "Thumbs Blend";
            vThumb.Tooltip = "Thumbs Blend";
            vThumb.GUISpacing = new Vector2(0, 4);
            vThumb.SetRangeHelperValue(new Vector2(-1f, 1f));
            vThumb.DrawGUI();

            var vThumbSpr = GetVariable("ThumbSpr", set, 0f);
            vThumbSpr.DisplayName = "Thumbs Spread";
            vThumbSpr.SetRangeHelperValue(new Vector2(-1f, 1f));
            vThumbSpr.DrawGUI();
        }


        public override void SceneView_DrawSceneHandles(ADClipSettings_CustomModules.CustomModuleSet set, float alphaAnimation = 1, float progress = 0f)
        {
            base.SceneView_DrawSceneHandles(set, alphaAnimation);

            if (LastHumanoidAnimator == null) return;

            var vL = GetVariable("Left", set, true);
            var vR = GetVariable("Right", set, true);

            Handles.color = new Color(1f, 0.3f, 0.5f, 0.1f + alphaAnimation * 0.4f);

            if ( vL.GetBoolValue())
            {
                Transform hand = GetHand(false);
                Transform foreArm = GetLowerArm(false);
                if (hand && foreArm) AnimationDesignerWindow.DrawBoneHandle(hand.position, hand.position + (hand.position - foreArm.position) * 0.33f, 0.7f + alphaAnimation, false, 2f + alphaAnimation); 
            }

            if (vR.GetBoolValue())
            {
                Transform hand = GetHand(true);
                Transform foreArm = GetLowerArm(true);
                if (hand && foreArm) AnimationDesignerWindow.DrawBoneHandle(hand.position, hand.position + (hand.position - foreArm.position) * 0.33f, 0.7f + alphaAnimation, false, 2f + alphaAnimation);
            }

        }


        #endregion


    }
}
