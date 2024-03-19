using FIMSpace.FEditor;
using FIMSpace.FTools;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.AnimationTools
{
    public partial class ADArmatureLimb
    {
        public FimpIK_Arm IKArmProcessor { get; private set; }
        public FIK_IKProcessor IKLegProcessor { get; private set; }
        public FIK_CCDProcessor IKCCDProcessor { get; private set; }
        bool ikCCDOk = false;
        bool ikError = false;

        public void CheckForIKInitialization(AnimationDesignerSave save, ADClipSettings_IK.IKSet ikSet, ADClipSettings_Main main, float progr, float dt, float boneBlend, bool reInitialize)
        {
            ikError = false;
            ikSet.RefreshMod(save, main);

            for (int i = 0; i < Bones.Count; i++)
            {
                Bones[i].RefreshTransformReference(save.Armature, false);
            }

            if (ikSet.IKType == ADClipSettings_IK.IKSet.EIKType.ArmIK)
            {
                if (IKArmProcessor == null || reInitialize || IKArmProcessor.IsCorrect == false || ikSet.requestReinitialize)
                {
                    if (Bones.Count == 4)
                    {
                        for (int i = 0; i < Bones.Count; i++)
                        {
                            if (Bones[i].T == null) { ikError = true; return; }
                        }

                        AnimationDesignerWindow.ForceTPose();
                        IKArmProcessor = new FimpIK_Arm();
                        IKArmProcessor.SetBones(Bones[0].T, Bones[1].T, Bones[2].T, Bones[3].T);
                        IKArmProcessor.Init(save.LatestAnimator.transform);
                        ikSet.requestReinitialize = false;
                    }
                }
            }

            if (ikSet.IKType == ADClipSettings_IK.IKSet.EIKType.FootIK)
                if (IKLegProcessor == null || reInitialize || ikSet.requestReinitialize || IKLegProcessor.Initialized == false || IKLegProcessor.Bones == null || IKLegProcessor.Bones.Length == 0 || IKLegProcessor.StartBone == null)
                    if (Bones.Count == 3)
                    {
                        if (Bones[0].T == null) { ikError = true; return; }
                        AnimationDesignerWindow.ForceTPose();
                        IKLegProcessor = new FIK_IKProcessor(Bones[0].T, Bones[1].T, Bones[2].T);
                        IKLegProcessor.AllowEditModeInit = true;
                        //IKLegProcessor.UseEnsuredRotation = true;
                        IKLegProcessor.Init(LatestAnimator.transform);
                        ikSet.requestReinitialize = false;
                    }

            ikCCDOk = false;
            if (ikSet.IKType == ADClipSettings_IK.IKSet.EIKType.ChainIK)
                if (IKCCDProcessor == null || reInitialize || ikSet.requestReinitialize || IKCCDProcessor.Initialized == false || IKCCDProcessor.Bones == null || IKCCDProcessor.Bones.Length == 0 || IKCCDProcessor.StartBone == null)
                    if (Bones.Count > 2)
                    {
                        ikCCDOk = true;
                        Transform[] chain = new Transform[Bones.Count];
                        for (int i = 0; i < Bones.Count; i++) { if (Bones[i].T == null) { ikError = true; return; } chain[i] = Bones[i].T; }
                        AnimationDesignerWindow.ForceTPose();
                        IKCCDProcessor = new FIK_CCDProcessor(chain);
                        IKCCDProcessor.Init(save.LatestAnimator.transform);
                        ikSet.requestReinitialize = false;
                    }

            if (reInitialize)
            {
                UpdateIKParams(ikSet, GetIKBlending(ikSet, progr, boneBlend));
                IKLateUpdateSimulation(ikSet, dt, progr, boneBlend, main);
            }
        }



        public float GetIKBlending(ADClipSettings_IK.IKSet ik, float progr, float boneBlend)
        {
            return AnimationBlend * ik.Blend * ik.BlendEvaluation.Evaluate(progr) * boneBlend;
        }

        public void UpdateIKInTPose()
        {
            if (ikCCDOk) IKCCDProcessor.Init(LatestAnimator.transform);

        }

        public void UpdateIKParams(ADClipSettings_IK.IKSet ik, float blend)
        {
            if (ik.Enabled == false) return;
            if (ikError) return;

            if (ik.IKType == ADClipSettings_IK.IKSet.EIKType.ArmIK)
            {
                if (IKArmProcessor == null || IKArmProcessor.UpperarmTransform == null  ) { ikError = true; return; }
                IKArmProcessor.IKWeight = blend;
                IKArmProcessor.IKPositionWeight = 1f;
                IKArmProcessor.HandRotationWeight = ik.IKRotationBlend;
                IKArmProcessor.UpperarmRotationOffset = Quaternion.Euler(ik.ThighOffset);
                IKArmProcessor.IKTargetPosition = Bones[3].pos;
                IKArmProcessor.IKTargetRotation = Bones[3].rot;
                IKArmProcessor.ManualHintPositionWeight = 1f;
                IKArmProcessor.ShoulderBlend = ik.ShoulderBlend;
                IKArmProcessor.AutoHintMode = FimpIK_Arm.FIK_HintMode.Default;
            }
            else if (ik.IKType == ADClipSettings_IK.IKSet.EIKType.FootIK)
            {
                if (IKLegProcessor == null || IKLegProcessor.Bones == null || IKLegProcessor.Bones.Length == 0) { ikError = true; return; }
                IKLegProcessor.IKWeight = blend;
                IKLegProcessor.IKTargetPosition = Bones[2].pos;
                IKLegProcessor.PositionWeight = 1f;
                IKLegProcessor.IKTargetRotation = Bones[2].rot;
                IKLegProcessor.StartBoneRotationOffset = Quaternion.Euler(ik.ThighOffset);

                IKLegProcessor.RotationWeight = ik.IKRotationBlend;
                IKLegProcessor.AutoHintMode = FIK_IKProcessor.FIK_HintMode.Default;
            }
            else if (ik.IKType == ADClipSettings_IK.IKSet.EIKType.ChainIK)
            {
                if (IKCCDProcessor == null || IKCCDProcessor.StartBone == null) { ikError = true; return; }
                IKCCDProcessor.IKWeight = blend;
                IKCCDProcessor.ContinousSolving = false;
                IKCCDProcessor.ReactionQuality = 32;
                IKCCDProcessor.IKTargetPosition = Bones[Bones.Count - 1].pos + LatestAnimator.transform.forward * IKCCDProcessor.ActiveLength * 0.05f;
                IKCCDProcessor.AutoWeightBones();
                IKCCDProcessor.AutoLimitAngle();
            }

        }

        public void IKCapture(ADClipSettings_IK.IKSet ik)
        {
            if (ik.Enabled == false) { return; }
            if (ikError) { return; }

            if (ik.IKType == ADClipSettings_IK.IKSet.EIKType.ArmIK)
            {
                if (IKArmProcessor != null) IKArmProcessor.CaptureKeyframeAnimation();
            }
            else if (ik.IKType == ADClipSettings_IK.IKSet.EIKType.FootIK)
            {
                if (IKLegProcessor != null) IKLegProcessor.RefreshAnimatorCoords();
            }
            else if (ik.IKType == ADClipSettings_IK.IKSet.EIKType.ChainIK)
            {
                if (IKCCDProcessor != null) IKCCDProcessor.PreCalibrate();
            }

        }

        public void IKUpdateSimulation(ADClipSettings_IK.IKSet ik, float dt, float progr, float boneBlend)
        {
            if (ik.Enabled == false) { return; }
            if (ikError) { return; }

            float blend = GetIKBlending(ik, progr, boneBlend);

            if (blend <= 0f) return;

            UpdateIKParams(ik, blend);
        }


        public void IKLateUpdateSimulation(ADClipSettings_IK.IKSet ik, float dt, float progr, float boneBlend, ADClipSettings_Main main)
        {
            _LatestIKRot = Quaternion.identity;

            if (ik.Enabled == false) return;
            if (ikError)
            {
                return;
            }

            float blend = GetIKBlending(ik, progr, boneBlend);

            if (blend <= 0f) return;

            if (ik.IKType == ADClipSettings_IK.IKSet.EIKType.ArmIK)
            {
                _LatestIKPos = ik.GetTargetIKPosition(IKArmProcessor.IKTargetPosition, LatestAnimator.transform, progr, IKArmProcessor, null);
                _LatestIKPos = Vector3.LerpUnclamped(IKArmProcessor.IKTargetPosition, _LatestIKPos, blend);

                _LatestHintPos = ik.GetHintPosition(IKArmProcessor.GetHintDefaultPosition(), LatestAnimator.transform, progr);

                _LatestIKRot = ik.GetTargetIKRotation(IKArmProcessor.IKTargetRotation, LatestAnimator.transform, progr);
                _LatestIKRot = Quaternion.SlerpUnclamped(IKArmProcessor.IKTargetRotation, _LatestIKRot, blend);

                if (ik.IKHintOffset == Vector3.zero)
                {
                    if (ik.AutoHintMode == ADClipSettings_IK.IKSet.EIKAutoHintMode.FollowHipsRotation) IKArmProcessor.AutoHintMode = FimpIK_Arm.FIK_HintMode.OnGoal;
                    else IKArmProcessor.AutoHintMode = FimpIK_Arm.FIK_HintMode.Default;
                }
                else
                {
                    IKArmProcessor.AutoHintMode = FimpIK_Arm.FIK_HintMode.Default;
                }


                IKArmProcessor.IKTargetPosition = _LatestIKPos;
                IKArmProcessor.IKTargetRotation = _LatestIKRot;
                IKArmProcessor.IKManualHintPosition = _LatestHintPos;
                IKArmProcessor.Update();
            }
            else if (ik.IKType == ADClipSettings_IK.IKSet.EIKType.FootIK)
            {
                if (ik.IsLegGroundingMode)
                {
                    IKLegProcessor.PositionWeight = 1f;
                    IKLegProcessor.RotationWeight = 1f;

                    Vector3 p = IKLegProcessor.IKTargetPosition; Quaternion q = IKLegProcessor.IKTargetRotation;
                    ik.ComputeFootIKGrounding(ref p, ref q, LatestAnimator.transform, progr, IKLegProcessor, main);
                    _LatestIKPos = p; _LatestIKRot = q;
                }
                else
                {
                    _LatestIKPos = ik.GetTargetIKPosition(IKLegProcessor.IKTargetPosition, LatestAnimator.transform, progr, null, IKLegProcessor);
                    _LatestIKPos = Vector3.LerpUnclamped(IKLegProcessor.IKTargetPosition, _LatestIKPos, blend);

                    _LatestIKRot = ik.GetTargetIKRotation(IKLegProcessor.IKTargetRotation, LatestAnimator.transform, progr);
                    _LatestIKRot = Quaternion.SlerpUnclamped(IKLegProcessor.IKTargetRotation, _LatestIKRot, blend);
                }


                if (ik.IKHintOffset != Vector3.zero)
                {
                    _LatestHintPos = ik.GetHintPosition(IKLegProcessor.GetHintDefaultPosition(), LatestAnimator.transform, progr);
                    IKLegProcessor.IKManualHintPosition = _LatestHintPos;
                    IKLegProcessor.ManualHintPositionWeight = 1f;
                }
                else
                {
                    if (ik.AutoHintMode == ADClipSettings_IK.IKSet.EIKAutoHintMode.FollowHipsRotation) IKLegProcessor.AutoHintMode = FIK_IKProcessor.FIK_HintMode.OnGoal;
                    else IKLegProcessor.AutoHintMode = FIK_IKProcessor.FIK_HintMode.Default;
                    IKLegProcessor.ManualHintPositionWeight = 0f;
                }

                IKLegProcessor.IKTargetPosition = _LatestIKPos;
                IKLegProcessor.IKTargetRotation = _LatestIKRot;
                IKLegProcessor.Update();

            }
            else if (ik.IKType == ADClipSettings_IK.IKSet.EIKType.ChainIK)
            {
                _LatestIKPos = ik.GetTargetIKPosition(IKCCDProcessor.IKTargetPosition, LatestAnimator.transform, progr, null, null);
                _LatestIKPos = Vector3.LerpUnclamped(IKCCDProcessor.IKTargetPosition, _LatestIKPos, blend);

                _LatestIKRot = ik.GetTargetIKRotation(IKCCDProcessor.IKTargetRotation, LatestAnimator.transform, progr);
                _LatestIKRot = Quaternion.SlerpUnclamped(IKCCDProcessor.IKTargetRotation, _LatestIKRot, blend);
                IKCCDProcessor.Smoothing = ik.ChainSmoothing;

                IKCCDProcessor.IKTargetPosition = _LatestIKPos;
                IKCCDProcessor.IKTargetRotation = _LatestIKRot;
                IKCCDProcessor.Update();
            }
        }

        public Vector3 _LatestIKPos { get; private set; } = Vector3.zero;
        public Quaternion _LatestIKRot { get; private set; } = Quaternion.identity;
        public Vector3 _LatestHintPos { get; private set; } = Vector3.zero;

    }
}