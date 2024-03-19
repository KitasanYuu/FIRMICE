using FIMSpace.FEditor;
using FIMSpace.FTools;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.AnimationTools
{
    public partial class ADClipSettings_Modificators
    {

        #region Triggering Stacks

        internal void CheckInitialization(AnimationDesignerSave save, bool reInitialize, ADClipSettings_Main main)
        {
            for (int i = 0; i < BonesModificators.Count; i++)
            {
                BonesModificators[i].CheckInitialization(save, reInitialize);
                BonesModificators[i].RefreshMod(save, main);
            }
        }


        internal void RefreshTransformReferences(AnimationDesignerSave save)
        {
            for (int i = 0; i < BonesModificators.Count; i++)
            {
                BonesModificators[i].RefreshTransformReference(save, true);
            }
        }


        internal void ResetStates()
        {
            for (int i = 0; i < BonesModificators.Count; i++)
            {
                if (BonesModificators[i].T == null) Debug.Log("[Animation Designer] Bone Modificator '" + BonesModificators[i].ModName + "' contains null bone! Please check it and assing bone again.");
                BonesModificators[i].ModificatorReset();
            }
        }

        internal void PreLateUpdateModificators(float dt)
        {
            for (int i = 0; i < BonesModificators.Count; i++)
            {
                var b = BonesModificators[i];
                if (b.Enabled == false) continue;
                BonesModificators[i].ModificatorPreLateUpdate(dt);
            }
        }

        internal void BeforeLateUpdateModificators(float dt, float progr, AnimationDesignerSave save, ADClipSettings_Main _anim_MainSet)
        {
            ADRootMotionBakeHelper.RootModsOffsetAccumulation = Vector3.zero;

            for (int i = 0; i < BonesModificators.Count; i++)
            {
                var b = BonesModificators[i];
                if (b.Enabled == false) continue;
                if (b.UpdateOrder != ModificatorSet.EOrder.BeforeEverything) continue;
                BonesModificators[i].ModificatorLateUpdateSimulation(dt, progr, this, save, _anim_MainSet);
            }
        }


        internal void PreElasticnessLateUpdateModificators(float dt, float progr, AnimationDesignerSave save, ADClipSettings_Main _anim_MainSet)
        {
            for (int i = 0; i < BonesModificators.Count; i++)
            {
                var b = BonesModificators[i];
                if (b.Enabled == false) continue;

                //BonesModificators[i].ModificatorPreLateUpdate(dt);

                if (b.UpdateOrder != ModificatorSet.EOrder.InheritElasticity) continue;
                BonesModificators[i].ModificatorLateUpdateSimulation(dt, progr, this, save, _anim_MainSet);
            }
        }


        internal void LateUpdateModificators(float dt, float progr, AnimationDesignerSave save, ADClipSettings_Main _anim_MainSet)
        {

            for (int i = 0; i < BonesModificators.Count; i++)
            {
                var b = BonesModificators[i];
                if (b.Enabled == false) continue;
                if (b.UpdateOrder != ModificatorSet.EOrder.AffectIK) continue;
                BonesModificators[i].ModificatorLateUpdateSimulation(dt, progr, this, save, _anim_MainSet);
            }
        }

        internal void LastLateUpdateModificators(float dt, float progr, AnimationDesignerSave save, ADClipSettings_Main _anim_MainSet)
        {
            for (int i = 0; i < BonesModificators.Count; i++)
            {
                var b = BonesModificators[i];
                if (b.Enabled == false) continue;
                if (b.UpdateOrder != ModificatorSet.EOrder.Last_Override) continue;
                BonesModificators[i].ModificatorLateUpdateSimulation(dt, progr, this, save, _anim_MainSet);
            }
        }


        #endregion


        public partial class ModificatorSet
        {
            public FMuscle_Quaternion RotationMuscle { get; private set; }
            public UniRotateBone RotationBone { get; private set; }

            internal void CheckInitialization(AnimationDesignerSave s, bool reInitialize)
            {
                RefreshTransformReference(s);
                //throw new NotImplementedException();

                if (T == null) return;

                if (RotationMuscle == null || RotationMuscle.IsCorrect == false || reInitialize)
                {
                    AnimationDesignerWindow.ForceTPose();
                    RotationMuscle = new FMuscle_Quaternion();
                    RotationMuscle.Initialize(T.rotation);
                }

                if (s.Armature != null)
                    if (s.Armature.RootBoneReference != null)
                        if (RotationBone == null || reInitialize)
                        {
                            AnimationDesignerWindow.ForceTPose();
                            RotationBone = new UniRotateBone(T, s.LatestAnimator.transform);
                        }
            }


            public void RefreshTransformReference(AnimationDesignerSave save, bool force = false)
            {
                if (T == null || force)
                {
                    if (string.IsNullOrEmpty(BoneName)) return;
                    Transform = save.GetBoneByName(BoneName);
                }
                //throw new NotImplementedException();
            }


            internal void ModificatorReset()
            {
                if (T == null) return;
                if (RotationMuscle != null) RotationMuscle.OverrideProceduralRotation(rot);
            }

            public float GetBlending(float progr, float boneBlend)
            {
                return Blend * BlendEvaluation.Evaluate(progr) * boneBlend;
            }

            internal void ModificatorPreLateUpdate(float dt)
            {

                if (Type == EModification.LookAtPosition)
                {
                    if (RotationBone != null)
                    {
                        //RotationBone.PreCalibrate();
                        RotationBone.RefreshCustomAxis(Vector3.up, Vector3.forward);
                    }
                }

            }


            internal void ModificatorLateUpdateSimulation(float dt, float animationProgress, ADClipSettings_Modificators set, AnimationDesignerSave save, ADClipSettings_Main main)
            {
                float blendC = GetBlending(animationProgress, 1f) * set.AllModificatorsBlend;
                if (T == null) return;

                //throw new NotImplementedException();
                if (Type == EModification.AdditiveRotation)
                {
                    Vector3 anglesOff = RotationValue;
                    anglesOff *= blendC * RotationBlend * RotationEvaluate.Evaluate(animationProgress);
                    Quaternion preTRot = T.rotation;

                    if (anglesOff != Vector3.zero)
                    {
                        if (FixedAxisRotation)
                            T.rotation = Quaternion.Euler(anglesOff) * (T.parent.rotation * T.localRotation);
                        else
                            T.localRotation *= Quaternion.Euler(anglesOff);
                    }

                    if (T.childCount > 0)
                    {
                        if (RotationEvaluate2 != null)
                        {
                            Transform ch = T.GetChild(0);
                            float eval2 = blendC * RotationEvaluate2.Evaluate(animationProgress) * RotationBlend;
                            if (eval2 != 0f) if (RotationValue2 != Vector3.zero)
                                {
                                    if (FixedAxisRotation)
                                        ch.rotation = Quaternion.Euler(RotationValue2 * eval2) * ((ChildRotIndependent ? preTRot : ch.parent.rotation ) * ch.localRotation);
                                    else
                                        ch.localRotation *= Quaternion.Euler(RotationValue2 * eval2);
                                }
                        }
                    }
                }
                else if (Type == EModification.AdditivePosition)
                {
                    Vector3 posOff = PositionValue;
                    posOff *= blendC * PositionBlend * PositionEvaluate.Evaluate(animationProgress);

                    if (posOff != Vector3.zero)
                    {
                        //if (RootMotionTransformSpace == false)
                        {
                            T.localPosition += posOff;
                        }
                        //else
                        //{
                        //    if (main.RootMotionTransform)
                        //        T.position += main.RootMotionTransform.TransformVector(posOff);
                        //}

                        if (UpdateOrder == EOrder.BeforeEverything)
                        {
                            if (save) if (save.SkelRootBone)
                                    if (T == save.SkelRootBone)
                                    {
                                        ADRootMotionBakeHelper.RootModsOffsetAccumulation += save.LatestAnimator.TransformVector(posOff);
                                    }
                        }

                    }

                }
                else if (Type == EModification.OverrideRotation)
                {
                    float blend = blendC * RotationBlend * RotationEvaluate.Evaluate(animationProgress);
                    if (blend > 0f) T.localRotation = Quaternion.SlerpUnclamped(Quaternion.identity, Quaternion.Euler(RotationValue), blend);
                }
                else if (Type == EModification.OverridePosition)
                {
                    float blend = blendC * PositionBlend * PositionEvaluate.Evaluate(animationProgress);

                    if (blend > 0f && PositionValue != Vector3.zero)
                    {
                        Vector3 val = Vector3.LerpUnclamped(Vector3.zero, PositionValue, blend);
                        Vector3 tgt = save.LatestAnimator.transform.InverseTransformPoint(T.position);

                        if (val.x != 0f) tgt.x = val.x;
                        if (val.y != 0f) tgt.y = val.y;
                        if (val.z != 0f) tgt.z = val.z;

                        T.position = save.LatestAnimator.transform.TransformPoint(tgt);
                    }

                }
                else if (Type == EModification.ElasticRotation)
                {
                    RotationMuscle.Acceleration = RotationsRapidity * 10000f + RotationsBoost * 4000f;
                    RotationMuscle.AccelerationLimit = RotationsRapidity * 5000f + RotationsBoost * 1200f;

                    RotationMuscle.BrakePower = 1f - RotationsSwinginess;
                    RotationMuscle.Damping = RotationsDamping * 40f;

                    RotationMuscle.Update(dt, T.transform.rotation);

                    if (blendC == 1f) T.rotation = RotationMuscle.ProceduralRotation;
                    else T.rotation = Quaternion.LerpUnclamped(T.rotation, RotationMuscle.ProceduralRotation, blendC * (1f + RotationBlend * 1.75f));
                }
                else if (Type == EModification.LookAtPosition)
                {
                    if (RotationBone != null)
                    {
                        if (RotationBone.root)
                        {
                            Vector3 target = RotationBone.root.TransformPoint(PositionValue);

                            if (alignTo != null) if (AlignToBlend > 0f) target = Vector3.Lerp(target, alignTo.position, AlignToBlend);

                            Quaternion newRot;
                            Vector3 toTarget = target - RotationBone.transform.position;

                            if (ModeSwitcher == 0)
                            {
                                Vector2 lookAngles = RotationBone.GetCustomLookAngles(toTarget, RotationBone);
                                newRot = RotationBone.RotateCustomAxis(lookAngles.x, lookAngles.y, RotationBone) * RotationBone.transform.rotation;
                            }
                            else 
                            {
                                Vector3 refRot = Quaternion.LookRotation(target - RotationBone.transform.position, Vector3.up).eulerAngles;
                                newRot = RotationBone.transform.parent.rotation * RotationBone.initialLocalRotation;
                                newRot *= Quaternion.AngleAxis(refRot.x, RotationBone.right);
                                newRot *= Quaternion.AngleAxis(refRot.y, RotationBone.up);
                                newRot *= Quaternion.AngleAxis(refRot.z, RotationBone.forward);
                            }

                            if (blendC == 1f) T.rotation = newRot;
                            else T.rotation = Quaternion.LerpUnclamped(T.rotation, newRot, blendC);
                        }
                    }
                }

            }

            internal void DrawGizmos(float boneFatness)
            {
                if (T == null) return;
                if (T.childCount == 0) return;

                Transform ch = T.GetChild(0);
                Vector3 toCh = ch.position - pos;

                float len = toCh.magnitude;
                Vector3 fatOff = Vector3.one * (len * 0.01f);
                Handles.CubeHandleCap(0, T.position, T.rotation, len * 0.05f, EventType.Repaint);
                FGUI_Handles.DrawBoneHandle(T.position, ch.position, boneFatness, true);
                FGUI_Handles.DrawBoneHandle(T.position + fatOff, ch.position + fatOff, boneFatness, true);
                FGUI_Handles.DrawBoneHandle(T.position - fatOff, ch.position - fatOff, boneFatness, true);
                Handles.CubeHandleCap(0, ch.position, ch.rotation, len * 0.05f, EventType.Repaint);
            }

            internal void DampReferences()
            {
                Transform = null;
            }
        }

        internal void DampReferences()
        {
            for (int i = 0; i < BonesModificators.Count; i++)
            {
                BonesModificators[i].DampReferences();
            }
        }
    }
}