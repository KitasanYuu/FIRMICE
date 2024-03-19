using FIMSpace.FEditor;
using FIMSpace.FTools;
using FIMSpace.Generating;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static FIMSpace.AnimationTools.ADClipSettings_CustomModules;

namespace FIMSpace.AnimationTools.CustomModules
{

    public class ADModule_BasicHipsStabilizer : ADCustomModuleBase
    {
        public override string ModuleTitleName { get { return "Inverse Kinematics (IK)/Basic Hips Stabilizer"; } }
        public override bool GUIFoldable { get { return false; } }
        public override bool SupportBlending { get { return true; } }

        public override void OnInheritElasticnessUpdate(float animationProgress, float deltaTime, AnimationDesignerSave s, ADClipSettings_Main anim_MainSet, ADClipSettings_CustomModules customModules, CustomModuleSet set)
        {
            PrepareReferences(s, anim_MainSet);

            base.OnInheritElasticnessUpdate(animationProgress, deltaTime, s, anim_MainSet, customModules, set);

            if (legStability == null) return;
            for (int i = 0; i < legStability.Count; i++)
            {
                legStability[i].endBone = s.Limbs[legs[i].Index].LastBone.T;
                legStability[i].keyPosition = legStability[i].endBone.position;
            }
        }

        class StablilityInfo
        {
            public Transform endBone;
            public Vector3 keyPosition;

            public struct Stability
            {
                public Vector3 toPelvis;
                public float toPelvisMagn;
                public Vector3 toPelvisNoY;
                public float toPelvisNoYMagn;

                public void ComputePelvisRelation(Vector3 refPos, Transform root, Transform pelvis)
                {
                    Vector3 cPos = refPos;
                    cPos = root.InverseTransformPoint(cPos);
                    Vector3 cPosNoY = cPos; cPosNoY.y = 0f;

                    Vector3 pelvisPos = root.InverseTransformPoint(pelvis.position);
                    Vector3 pelvisPosNoY = pelvisPos; pelvisPosNoY.y = 0f;

                    toPelvis = pelvisPos - cPos;
                    toPelvisMagn = toPelvis.magnitude;

                    toPelvisNoY = pelvisPosNoY - cPosNoY;
                    toPelvisNoYMagn = toPelvisNoY.magnitude;
                }
            }

            public Stability OriginalStability;
            public Stability CurrentStability;

            public void ComputePelvisRelation(ADClipSettings_IK.IKSet leg, Transform root, Transform pelvis)
            {
                OriginalStability = new Stability();
                OriginalStability.ComputePelvisRelation(keyPosition, root, pelvis);

                CurrentStability = new Stability();
                CurrentStability.ComputePelvisRelation(leg.LastTargetIKPosition, root, pelvis);
            }

        }

        Vector3 _sd_hipsSmooth = Vector3.zero;
        Vector3 _hipsSmooth = Vector3.zero;

        bool resetHips = true;
        public override void OnResetState(CustomModuleSet customModuleSet)
        {
            base.OnResetState(customModuleSet);
            resetHips = true;
        }

        public override void OnInfluenceIKUpdate(float animationProgress, float deltaTime, AnimationDesignerSave s, ADClipSettings_Main anim_MainSet, ADClipSettings_CustomModules customModules, ADClipSettings_CustomModules.CustomModuleSet set)
        {
            if (anim_MainSet.Pelvis == null) return;

            Vector3 pelvisRefPos = anim_MainSet.Pelvis.position;

            float countDiv = (float)legs.Count;

            var hipsPush = GVar("HP", "Hips Push Blend:", 1f);
            hipsPush.SetRangeHelperValue(0.0f, 1f);
            hipsPush.GUISpacing = new Vector2(0, 7);
            float hipsPushBlend = hipsPush.Float;

            Vector3 stabilityDiff = Vector3.zero;

            for (int l = 0; l < legs.Count; l++)
            {
                legStability[l].ComputePelvisRelation(legs[l], s.LatestAnimator, anim_MainSet.Pelvis);
                stabilityDiff += (legStability[l].OriginalStability.toPelvisNoY - legStability[l].CurrentStability.toPelvisNoY);
            }

            stabilityDiff /= countDiv;
            stabilityDiff *= hipsPushBlend;

            var stretchLimit = GVar("SL", "Max Stretching:", 1f);
            stretchLimit.SetRangeHelperValue(0.5f, 1f);
            float limitStretch = stretchLimit.Float;

            Vector3 stretchDiff = Vector3.zero;
            
            if (limitStretch < 1f)
            {
                for (int l = 0; l < legs.Count; l++)
                {
                    var legL = legs[l];
                    if (legL.LastUsedProcessor == null) continue;
                    FIK_IKProcessor legIK = legL.LastUsedProcessor as FIK_IKProcessor;
                    if (legIK == null) continue;

                    float stretch = legIK.GetStretchValue(((legIK.StartIKBone.srcPosition + stabilityDiff) - legL.LastTargetIKPosition).magnitude);
                    if (stretch > limitStretch)
                    {
                        //float transition = Mathf.InverseLerp(1f, 0.985f, 1f-(stretch - limitStretch));

                        Vector3 pelvisToIK = legL.LastTargetIKPosition - pelvisRefPos;
                        float len = (stretch - limitStretch) * legIK.GetLimbLength();
                        stretchDiff += pelvisToIK.normalized * len;
                    }
                }
            }

            var strechLimitBlend = GVar("SLB", "Stretch Limit Blend:", 0.9f);
            strechLimitBlend.GUISpacing = new Vector2(0, 6);
            strechLimitBlend.SetRangeHelperValue(0f, 1f);

            stabilityDiff += (stretchDiff / countDiv) * strechLimitBlend.Float;

            if (resetHips)
            {
                _hipsSmooth = stabilityDiff;
                resetHips = false;
            }

            var smoothingMot = GVar("SM", "Smoothing Motion:", 0.3f);
            smoothingMot.SetRangeHelperValue(0f, 1f);

            float smoothingOffset = smoothingMot.Float;

            if (smoothingOffset <= 0f)
                _hipsSmooth = stabilityDiff;
            else
                _hipsSmooth = Vector3.SmoothDamp(_hipsSmooth, stabilityDiff, ref _sd_hipsSmooth, 0.001f + 0.25f * smoothingOffset, float.MaxValue, deltaTime);

            anim_MainSet.PelvisFrameCustomPositionOffset += _hipsSmooth * GetEvaluatedBlend(set, animationProgress);
        }


        #region Editor GUI Related Code

        [HideInInspector] public bool _InitialInfoClicked = false;

        public override void InspectorGUI_Header(float animProgress, ADClipSettings_CustomModules.CustomModuleSet customModuleSet)
        {
            //OnRefreshVariables(customModuleSet);
            base.InspectorGUI_Header(animProgress, customModuleSet);
        }

        List<ADClipSettings_IK.IKSet> legs = new List<ADClipSettings_IK.IKSet>();

        List<StablilityInfo> legStability = new List<StablilityInfo>();

        void PrepareReferences(AnimationDesignerSave s, ADClipSettings_Main _anim_MainSet)
        {
            var ikSet = s.GetSetupForClip<ADClipSettings_IK>(s.IKSetupsForClips, _anim_MainSet.settingsForClip, _anim_MainSet.SetIDHash);
            if (ikSet == null) return;

            if (ikSet.LimbIKSetups.Count == 0)
            {
                return;
            }

            if (legs == null) legs = new List<ADClipSettings_IK.IKSet>();
            if (legs.Count > 0) if (legs[0] != ikSet.LimbIKSetups[0]) legs.Clear();
            if (legs.Count <= 0 || legs[0] == null || legs[0].LastUsedProcessor == null)
            {
                legs.Clear();
                for (int i = 0; i < ikSet.LimbIKSetups.Count; i++)
                {
                    if (ikSet.LimbIKSetups[i].IKType == ADClipSettings_IK.IKSet.EIKType.FootIK)
                    {
                        legs.Add(ikSet.LimbIKSetups[i]);
                    }
                }
            }

            if (legStability == null) legStability = new List<StablilityInfo>();
            FGenerators.AdjustCount(legStability, legs.Count);
        }

        public override void InspectorGUI_ModuleBody(float optionalBlendGhost, ADClipSettings_Main _anim_MainSet, AnimationDesignerSave s, ADClipSettings_CustomModules cModule, CustomModuleSet set)
        {
            #region Prepare Resources

            var ikSet = s.GetSetupForClip<ADClipSettings_IK>(s.IKSetupsForClips, _anim_MainSet.settingsForClip, AnimationDesignerWindow._toSet_SetSwitchToHash);

            if (ikSet == null)
            {
                EditorGUILayout.HelpBox("Can't Find IK Set!", MessageType.Warning);
                return;
            }

            #endregion

            #region Legs IK Search

            if (ikSet.LimbIKSetups.Count == 0)
            {
                EditorGUILayout.HelpBox("Can't Find Limb IK Setups!", MessageType.Warning);
                return;
            }

            PrepareReferences(s, _anim_MainSet);

            if (legs.Count <= 0)
            {
                EditorGUILayout.HelpBox("Can't Find Limb IK Legs!", MessageType.Warning);
                return;
            }

            if (_anim_MainSet.TurnOnIK == false)
            {
                EditorGUILayout.HelpBox("Go to IK Tab and enable using IK", MessageType.Warning);
                return;
            }

            if (legs[0].Enabled == false)
            {
                EditorGUILayout.HelpBox("Some legs IK seems to be disabled! Hips Stabilizer will work only with enabled foot IKs!", MessageType.Info);
                if (GUILayout.Button("Enable Foot IKs"))
                {
                    for (int i = 0; i < legs.Count; i++) legs[i].Enabled = true;
                    S._SetDirty();
                }
            }
            else
            {
                if (!_InitialInfoClicked)
                {
                    EditorGUILayout.HelpBox("Hips Stabilizer can help you achieving stable pose after doing changes in the original clip.\nIt's dedicated for standing animations!", MessageType.None);
                    var r = GUILayoutUtility.GetLastRect();

                    if (GUI.Button(r, GUIContent.none, EditorStyles.label))
                    {
                        _InitialInfoClicked = true;
                    }

                    GUILayout.Space(6);
                }
            }

            #endregion

            base.InspectorGUI_ModuleBody(optionalBlendGhost, _anim_MainSet, s, cModule, set);
        }

        ADVariable GVar(string id, string displayName, object defValue, string tooltip = "")
        {
            var v = GetVariable(id, null, defValue);
            v.DisplayName = displayName;
            v.Tooltip = tooltip;
            return v;
        }

        #endregion


    }
}