using UnityEditor;
using UnityEngine;

namespace FIMSpace.AnimationTools.CustomModules
{
    public class ADModule_SuddenStepRotations : ADCustomModuleBase
    {
        public override string ModuleTitleName { get { return "Animation Helper/Sudden Step Rotations"; } }
        public override bool GUIFoldable { get { return false; } }
        public override bool SupportBlending { get { return true; } }

        AnimationCurve _defautPattern = null;
        AnimationCurve defaultPatter
        {
            get
            {
                if (_defautPattern == null)
                {
                    Keyframe[] keys = new Keyframe[6];
                    keys[0] = new Keyframe(0f, 0f);
                    keys[0].weightedMode = WeightedMode.None;
                    keys[1] = new Keyframe(0.2f, .6f);
                    keys[1].weightedMode = WeightedMode.None;
                    keys[2] = new Keyframe(.4f, .3f);
                    keys[2].weightedMode = WeightedMode.None;
                    keys[3] = new Keyframe(.6f, -.5f);
                    keys[3].weightedMode = WeightedMode.None;
                    keys[4] = new Keyframe(.85f, -.3f);
                    keys[4].weightedMode = WeightedMode.None;
                    keys[5] = new Keyframe(1f, 0f);
                    keys[5].weightedMode = WeightedMode.None;
                    _defautPattern = new AnimationCurve(keys);
                }

                return _defautPattern;
            }
        }


        // Here GUI code which defines variables to tweak and displaying them
        public override void InspectorGUI_ModuleBody(float optionalBlendGhost, ADClipSettings_Main _anim_MainSet, AnimationDesignerSave s, ADClipSettings_CustomModules cModule, ADClipSettings_CustomModules.CustomModuleSet set)
        {
            GUILayout.Space(5);

            var patternCurve = GetVariable("Pattern", set, defaultPatter);
            patternCurve.DisplayName = "Points Pattern:";
            patternCurve.Tooltip = "Curve does not matter, just the points and its values";
            patternCurve.SetRangeHelperValue(new Vector4(0, -1, 1, 1));

            //var rate = GetVariable("Seed", set, 0);
            //rate.FloatSwitch = ADVariable.EVarFloatingSwitch.Int;
            //rate.DisplayName = "Seed:";
            //rate.Tooltip = "Random seed for rotations turbulence";

            var off = GetVariable("Spd", set, .5f);
            off.SetRangeHelperValue(0f, 1f);
            off.DisplayName = "Transition Speed:";
            off.Tooltip = "How quick the rotation steps should be.";

            var angles = GetVariable("Angl", set, new Vector3(25, 0, 0));
            angles.DisplayName = "Vibration Angles:";
            angles.GUISpacing = new Vector2(0, 4);

            // Display variables

            EditorGUI.BeginChangeCheck();
            base.InspectorGUI_ModuleBody(optionalBlendGhost, _anim_MainSet, s, cModule, set);
            if (EditorGUI.EndChangeCheck()) set.HelpFlag = false;

            GUILayout.Space(4);

            var trVariable = GetVariable("T", set, "");
            trVariable.HideFlag = true;
            var trIDVariable = GetVariable("T_ID", set, "");
            trIDVariable.HideFlag = true;

            ArmatureTransformField(trVariable, trIDVariable, s, "To Vibrate:");

            GUILayout.Space(4);

            var updateOrder = GetVariable("Update Order:", set, 0);
            updateOrder.HideFlag = true;

            ADClipSettings_Modificators.ModificatorSet.EOrder order = (ADClipSettings_Modificators.ModificatorSet.EOrder)
                updateOrder.GetIntValue();

            order = (ADClipSettings_Modificators.ModificatorSet.EOrder)
                EditorGUILayout.EnumPopup(order);

            updateOrder.SetValue((int)order);

            _updateOrder = order;
        }

        ADClipSettings_Modificators.ModificatorSet.EOrder _updateOrder = ADClipSettings_Modificators.ModificatorSet.EOrder.InheritElasticity;

        public override void OnResetState(ADClipSettings_CustomModules.CustomModuleSet customModuleSet)
        {
            base.OnResetState(customModuleSet);

            var updateOrder = GetVariable("Update Order:", customModuleSet, 0);
            _updateOrder = (ADClipSettings_Modificators.ModificatorSet.EOrder)updateOrder.GetIntValue();

            customModuleSet.HelpFlag = false;
        }


        Keyframe[] _keys = null;
        void UpdateVibrate(ADClipSettings_CustomModules.CustomModuleSet set, AnimationDesignerSave s, ADClipSettings_Main anim_MainSet, float animProgress)
        {

            if (_keys == null || set.HelpFlag == false)
            {
                ReadTransformForIDVariables(GetVariable("T", set, ""), GetVariable("T_ID", set, ""), s);
                var patternCurve = GetVariable("Pattern", set, defaultPatter);
                _keys = patternCurve.GetCurve().keys;
                set.HelpFlag = true;
            }

            var trVariable = GetVariable("T", set, "");
            Transform t = trVariable.GetValue() as Transform;

            if (t == null) return;

            if (_keys.Length < 3) return;

            int preIndex = GetIndexAtProgress(animProgress);
            if (preIndex >= _keys.Length - 1) preIndex = _keys.Length - 2;

            float progDiff = Mathf.Abs(_keys[preIndex + 1].time - animProgress);
            float AtoB = Mathf.InverseLerp(0.05f * (1f - GetVariable("Spd", set, .5f).Float), 0f, progDiff);
            var angles = GetVariable("Angl", set, new Vector3(25, 0, 0));
            float blend = set.Blend * set.BlendEvaluation.Evaluate(animProgress);
            t.localRotation *= Quaternion.Euler(angles.GetVector3Value() * Mathf.Lerp(_keys[preIndex].value, _keys[preIndex + 1].value, AtoB) * blend);
        }

        int GetIndexAtProgress(float dist)
        {
            if (dist >= 1f) return _keys.Length;
            for (int p = 0; p < _keys.Length - 1; p++)
            {
                if (dist >= _keys[p].time && dist < _keys[p + 1].time) return p;
            }

            return 0;
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