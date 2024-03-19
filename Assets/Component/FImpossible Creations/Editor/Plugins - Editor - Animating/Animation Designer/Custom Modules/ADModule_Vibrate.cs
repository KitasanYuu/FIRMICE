using System;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.AnimationTools.CustomModules
{
    public class ADModule_Vibrate : ADCustomModuleBase
    {
        public override string ModuleTitleName { get { return "Animation Helper/Vibrate"; } }
        public override bool GUIFoldable { get { return false; } }
        public override bool SupportBlending { get { return true; } }


        AnimationCurve _defautPattern = null;
        AnimationCurve defaultPattern
        {
            get
            {
                if (_defautPattern == null || _defautPattern.length < 2)
                {
                    Keyframe[] keys = new Keyframe[3];
                    keys[0] = new Keyframe(0f, 0f);
                    keys[1] = new Keyframe(0.5f, 1f);
                    keys[2] = new Keyframe(1f, 0f);
                    _defautPattern = new AnimationCurve(keys);
                    _defautPattern.SmoothTangents(0, 1f);
                    _defautPattern.SmoothTangents(1, 1f);
                    _defautPattern.SmoothTangents(2, 1f);
                    _defautPattern.preWrapMode = WrapMode.Loop;
                    _defautPattern.postWrapMode = WrapMode.Loop;
                    _defautPattern = new AnimationCurve(keys);
                }

                return _defautPattern;
            }
        }


        // Here GUI code which defines variables to tweak and displaying them
        public override void InspectorGUI_ModuleBody(float optionalBlendGhost, ADClipSettings_Main _anim_MainSet, AnimationDesignerSave s, ADClipSettings_CustomModules cModule, ADClipSettings_CustomModules.CustomModuleSet set)
        {
            GUILayout.Space(5);

            var patternCurve = GetVariable("Pattern", set, defaultPattern);
            patternCurve.SetRangeHelperValue(new Vector4(0, 0, 1, 1));

            var rate = GetVariable("Rate", set, 4);
            rate.FloatSwitch = ADVariable.EVarFloatingSwitch.Int;
            rate.DisplayName = "Frequency Rate:";
            rate.Tooltip = "How quickly the pattern proceeds over the animation clip time";

            var off = GetVariable("Off", set, 0f);
            off.SetRangeHelperValue(-1f, 1f);
            off.DisplayName = "Frequency Offset:";
            off.Tooltip = "Curve progress offset, helpful when adding this module to multiple bones.";

            var angles = GetVariable("Angl", set, new Vector3(25, 0, 0));
            angles.DisplayName = "Vibration Angles:";
            angles.GUISpacing = new Vector2(0, 4);

            // Display variables
            base.InspectorGUI_ModuleBody(optionalBlendGhost, _anim_MainSet, s, cModule, set);

            GUILayout.Space(4);

            var trVariable = GetVariable("T", set, "");
            trVariable.HideFlag = true;
            var trIDVariable = GetVariable("T_ID", set, "");
            trIDVariable.HideFlag = true;

            ArmatureTransformField(trVariable, trIDVariable, s, "To Vibrate:");

            GUILayout.Space(4);

            #region Update Order

            var updateOrder = GetVariable("Update Order:", set, 0);
            updateOrder.HideFlag = true;

            ADClipSettings_Modificators.ModificatorSet.EOrder order = (ADClipSettings_Modificators.ModificatorSet.EOrder)
                updateOrder.GetIntValue();

            order = (ADClipSettings_Modificators.ModificatorSet.EOrder)
                EditorGUILayout.EnumPopup(order);

            updateOrder.SetValue((int)order);

            #endregion

            _updateOrder = order;
        }

        ADClipSettings_Modificators.ModificatorSet.EOrder _updateOrder = ADClipSettings_Modificators.ModificatorSet.EOrder.InheritElasticity;

        public override void OnResetState(ADClipSettings_CustomModules.CustomModuleSet customModuleSet)
        {
            base.OnResetState(customModuleSet);

            var updateOrder = GetVariable("Update Order:", customModuleSet, 0);
            _updateOrder = (ADClipSettings_Modificators.ModificatorSet.EOrder)updateOrder.GetIntValue();
        }

        void UpdateVibrate(ADClipSettings_CustomModules.CustomModuleSet set, AnimationDesignerSave s, ADClipSettings_Main anim_MainSet, float animProgress)
        {
            var trVariable = GetVariable("T", set, "");

            if (set.HelpFlag == false || trVariable.GetValue() == null)
            {
                ReadTransformForIDVariables(trVariable, GetVariable("T_ID", set, ""), s);
                set.HelpFlag = true;
            }

            Transform t = trVariable.GetValue() as Transform;
            if (t == null) { return; }

            var patternCurve = GetVariable("Pattern", set, defaultPattern);
            var rate = GetVariable("Rate", set, 4f);

            var off = GetVariable("Off", set, 0f);

            float eval = patternCurve.GetCurve().Evaluate(((animProgress + off.Float) * rate.Float) % 1f);
            eval *= set.Blend * set.BlendEvaluation.Evaluate(animProgress);

            var angles = GetVariable("Angl", set, new Vector3(25, 0, 0));
            t.localRotation *= Quaternion.Euler(angles.GetVector3Value() * eval);
        }

        public override void OnBeforeIKUpdate(float animationProgress, float deltaTime, AnimationDesignerSave s, ADClipSettings_Main anim_MainSet, ADClipSettings_CustomModules customModules, ADClipSettings_CustomModules.CustomModuleSet set)
        {
            base.OnBeforeIKUpdate(animationProgress, deltaTime, s, anim_MainSet, customModules, set);
            if (_updateOrder != ADClipSettings_Modificators.ModificatorSet.EOrder.BeforeEverything) return;
            UpdateVibrate(set, s, anim_MainSet, animationProgress);
        }

        public override void OnInheritElasticnessUpdate(float animationProgress, float deltaTime, AnimationDesignerSave s, ADClipSettings_Main anim_MainSet, ADClipSettings_CustomModules customModules, ADClipSettings_CustomModules.CustomModuleSet set)
        {
            base.OnInheritElasticnessUpdate(animationProgress, deltaTime, s, anim_MainSet, customModules, set);
            if (_updateOrder != ADClipSettings_Modificators.ModificatorSet.EOrder.InheritElasticity) return;
            UpdateVibrate(set, s, anim_MainSet, animationProgress);
        }

        public override void OnInfluenceIKUpdate(float animationProgress, float deltaTime, AnimationDesignerSave s, ADClipSettings_Main anim_MainSet, ADClipSettings_CustomModules customModules, ADClipSettings_CustomModules.CustomModuleSet set)
        {
            base.OnInfluenceIKUpdate(animationProgress, deltaTime, s, anim_MainSet, customModules, set);
            if (_updateOrder != ADClipSettings_Modificators.ModificatorSet.EOrder.AffectIK) return;
            UpdateVibrate(set, s, anim_MainSet, animationProgress);
        }

        public override void OnLastUpdate(float animationProgress, float deltaTime, AnimationDesignerSave s, ADClipSettings_Main anim_MainSet, ADClipSettings_CustomModules customModules, ADClipSettings_CustomModules.CustomModuleSet set)
        {
            base.OnLastUpdate(animationProgress, deltaTime, s, anim_MainSet, customModules, set);
            if (_updateOrder != ADClipSettings_Modificators.ModificatorSet.EOrder.Last_Override) return;
            UpdateVibrate(set, s, anim_MainSet, animationProgress);
        }

    }
}