using FIMSpace.FEditor;
using FIMSpace.FTools;
using FIMSpace.Generating;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static FIMSpace.AnimationTools.ADClipSettings_CustomModules;

namespace FIMSpace.AnimationTools.CustomModules
{

    public class ADModule_ReachHelper : ADCustomModuleBase
    {
        public override string ModuleTitleName { get { return "Inverse Kinematics (IK)/Hand Reach Helper"; } }
        public override bool GUIFoldable { get { return false; } }
        public override bool SupportBlending { get { return true; } }

        public override void OnInheritElasticnessUpdate(float animationProgress, float deltaTime, AnimationDesignerSave s, ADClipSettings_Main anim_MainSet, ADClipSettings_CustomModules customModules, CustomModuleSet set)
        {
            PrepareReferences(s, anim_MainSet);

            base.OnInheritElasticnessUpdate(animationProgress, deltaTime, s, anim_MainSet, customModules, set);

            if (armsInfo == null) return;
            for (int i = 0; i < armsInfo.Count; i++)
            {
                armsInfo[i].startBone = s.Limbs[arms[i].Index].FirstBone.T;
                armsInfo[i].endBone = s.Limbs[arms[i].Index].LastBone.T;
                armsInfo[i].keyPosition = armsInfo[i].endBone.position;
            }
        }

        class ArmInfo
        {
            public Transform startBone;
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


        // Here 'Update()' execution
        public override void OnInfluenceIKUpdate(float animationProgress, float deltaTime, AnimationDesignerSave s, ADClipSettings_Main anim_MainSet, ADClipSettings_CustomModules customModules, ADClipSettings_CustomModules.CustomModuleSet set)
        {
            Transform root = s.LatestAnimator;

            var selectedLimb = GetVariable("Spine Limb", null, 0);

            int limbId = selectedLimb.GetIntValue();
            var spineLimb = GetLimbByID(s, limbId);
            if (spineLimb == null) return;

            var spineRotB = GVar("srb", "Spine Rotation Blend", 0.4f);
            spineRotB.SetRangeHelperValue(0f, 1f);

            var spineForwB = GVar("sfb", "Spine Forward Blend", 0.1f);
            spineForwB.SetRangeHelperValue(0f, 1f);
            spineForwB.GUISpacing = new Vector2(0, 6);

            var sensitivityBlend = GVar("sns", "Sensitivity", 1.5f);
            sensitivityBlend.SetRangeHelperValue(0f, 3f);

            var boostBlend = GVar("bst", "Boost", 1f);
            boostBlend.SetRangeHelperValue(0f, 2f);

            //Vector3 pushFactor = Vector3.zero;
            //float countDiv = (float)arms.Count;
            //Vector3 stabilityDiff = Vector3.zero;
            var spineStart = spineLimb.FirstBone;

            xAngle = 0f;
            yAngle = 0f;
            float stretchSensitivity = sensitivityBlend.Float;

            for (int l = 0; l < arms.Count; l++)
            {
                armsInfo[l].ComputePelvisRelation(arms[l], s.LatestAnimator, anim_MainSet.Pelvis);

                if (arms[l].Enabled == false) continue;

                var armIK = arms[l].LastUsedProcessor as FimpIK_Arm;
                if (armIK == null) continue;
                if (armIK.ShoulderIKBone == null) continue;
                if (armIK.UpperArmIKBone == null) continue;

                Vector3 chestToUpperArm = armIK.UpperArmIKBone.transform.position - armIK.ShoulderIKBone.transform.parent.position;

                Vector3 spineRight = spineStart.T.rotation * spineStart.RightInRoot;
                Vector3 spineUp = spineStart.T.rotation * spineStart.UpInRoot;
                Vector3 spineForward = spineStart.T.rotation * spineStart.RightInRoot;

                float dot = Vector3.Dot(-spineRight, chestToUpperArm.normalized);
                if (dot > 0f) dot = 1f;
                if (dot < 0f) dot = -1f;


                Vector3 armToTarget = arms[l].LastTargetIKPosition - armsInfo[l].startBone.position;
                armToTarget.Normalize();

                float forwardDot = Vector3.Dot(Vector3.Cross(spineForward, armToTarget), -spineUp);
                float updownDot = Vector3.Dot(spineUp, armToTarget);
                //float leftRightDot = Vector3.Dot(spineRight, armToTarget);

                float stretch = armIK.GetStretchValue(arms[l].LastTargetIKPosition);
                if (stretch > 0.8f)
                {
                    yAngle += dot * forwardDot * (stretch * stretchSensitivity - 0.8f * stretchSensitivity) * (1f - Mathf.Abs(updownDot));
                    xAngle += -updownDot * (stretch * stretchSensitivity - 0.8f * stretchSensitivity);
                }
            }

            yAngle *= 50f * boostBlend.Float * spineRotB.Float;
            yAngle = Mathf.Clamp(yAngle, -75f, 75f);

            xAngle *= 50f * boostBlend.Float * spineForwB.Float;
            xAngle = Mathf.Clamp(xAngle, -32f, 60f);

            int spineBones = Mathf.Min(3, spineLimb.Bones.Count);
            float spineCountDiv = (float)spineBones;

            xAngle /= spineCountDiv;
            yAngle /= spineCountDiv;

            float blend = GetEvaluatedBlend(set, animationProgress);

            float ff = 0.4f;
            for (int i = 0; i < spineBones; i++)
            {
                ff += 0.3f;
                Quaternion targetRot = Quaternion.AngleAxis(xAngle * ff * blend, spineLimb.Bones[i].T.rotation * spineLimb.Bones[i].RightInRoot);
                targetRot *= Quaternion.AngleAxis(yAngle * ff * blend, spineLimb.Bones[i].T.rotation * spineLimb.Bones[i].UpInRoot);
                targetRot *= spineLimb.Bones[i].T.rotation;

                spineLimb.Bones[i].T.rotation = targetRot;
            }

            //anim_MainSet.PelvisFrameCustomPositionOffset += new Vector3(Mathf.Sin(animationProgress * Mathf.PI * 2f) * 0.1f, 0f, 0f);
        }

        float xAngle = 0f;
        float yAngle = 0f;

        #region Editor GUI Related Code

        [HideInInspector] public bool _InitialInfoClicked = false;

        List<ADClipSettings_IK.IKSet> arms = new List<ADClipSettings_IK.IKSet>();
        List<ArmInfo> armsInfo = new List<ArmInfo>();

        void PrepareReferences(AnimationDesignerSave s, ADClipSettings_Main _anim_MainSet)
        {
            var ikSet = s.GetSetupForClip<ADClipSettings_IK>(s.IKSetupsForClips, _anim_MainSet.settingsForClip, _anim_MainSet.SetIDHash);
            if (ikSet == null) return;

            if (ikSet.LimbIKSetups.Count == 0)
            {
                return;
            }

            if (arms == null) arms = new List<ADClipSettings_IK.IKSet>();
            if (arms.Count > 0) if (arms[0] != ikSet.LimbIKSetups[0]) arms.Clear();
            if (arms.Count <= 0 || arms[0] == null || arms[0].LastUsedProcessor == null)
            {
                arms.Clear();
                for (int i = 0; i < ikSet.LimbIKSetups.Count; i++)
                {
                    if (ikSet.LimbIKSetups[i].IKType == ADClipSettings_IK.IKSet.EIKType.ArmIK)
                    {
                        arms.Add(ikSet.LimbIKSetups[i]);
                    }
                }
            }

            if (armsInfo == null) armsInfo = new List<ArmInfo>();
            FGenerators.AdjustCount(armsInfo, arms.Count);
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

            #region Arm IK Search

            if (ikSet.LimbIKSetups.Count == 0)
            {
                EditorGUILayout.HelpBox("Can't Find Limb IK Setups!", MessageType.Warning);
                return;
            }

            PrepareReferences(s, _anim_MainSet);

            if (arms.Count <= 0)
            {
                EditorGUILayout.HelpBox("Can't Find Limb IK Arms!", MessageType.Warning);
                return;
            }

            if (_anim_MainSet.TurnOnIK == false)
            {
                EditorGUILayout.HelpBox("Go to IK Tab and enable using IK", MessageType.Warning);
                return;
            }

            bool anyOn = false;
            for (int i = 0; i < arms.Count; i++)
            {
                if (arms[i].Enabled == true)
                {
                    anyOn = true;
                    break;
                }
            }

            if (!anyOn)
            {
                EditorGUILayout.HelpBox("Arms IK seems to be disabled! Hand Reach Helper will work only with enabled at least one arm IK!", MessageType.Info);
                if (GUILayout.Button("Enable Arms IKs"))
                {
                    for (int i = 0; i < arms.Count; i++) arms[i].Enabled = true;
                    S._SetDirty();
                }
            }
            else
            {
                if (!_InitialInfoClicked)
                {
                    EditorGUILayout.HelpBox("Hand Reach Helper will automatically rotate spine bones to make arm reach target ik point more easily.\nModule dedicated for standing animations!", MessageType.None);
                    var r = GUILayoutUtility.GetLastRect();

                    if (GUI.Button(r, GUIContent.none, EditorStyles.label))
                    {
                        _InitialInfoClicked = true;
                    }

                    GUILayout.Space(6);
                }
            }

            #endregion


            // Read variables defined inside 'InspectorGUI_ModuleBody()' above
            var selectedLimb = GetVariable("Spine Limb", null, 0);
            selectedLimb.Tooltip = "You need to tell module, which limb is spine limb in order to rotate it properly";
            selectedLimb.GUISpacing = new Vector2(0, 7);

            var wasAutoSpine = GetVariable("w", null, false);
            wasAutoSpine.HideFlag = true;

            if (wasAutoSpine.GetBoolValue() == false)
            {
                for (int i = 0; i < s.Limbs.Count; i++)
                {
                    if (s.Limbs[i].GetName.ToLower().StartsWith("spin"))
                    {
                        selectedLimb.IntV = i;
                        break;
                    }
                }

                wasAutoSpine.SetValue(true);
            }

            selectedLimb.SetRangeHelperValue(new Vector2(0, s.Limbs.Count - 1)); // Limbs count slider
            int limbId = selectedLimb.GetIntValue();

            // Access selected limb IK to modify it
            var limb = GetLimbByID(s, limbId);
            if (limb == null) return; // Limb not exists! Don't do anything then to prevent errors

            EditorGUILayout.LabelField("! Spine Limb = " + limb.GetName, EditorStyles.centeredGreyMiniLabel);

            base.InspectorGUI_ModuleBody(optionalBlendGhost, _anim_MainSet, s, cModule, set);

            GUILayout.Space(5);
            EditorGUILayout.HelpBox("Debug: X Angle = " + xAngle + " Y Angle = " + yAngle, MessageType.None);
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
