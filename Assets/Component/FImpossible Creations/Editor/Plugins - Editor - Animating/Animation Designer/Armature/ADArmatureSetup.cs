using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.AnimationTools
{
    /// <summary>
    /// Bone Transforms References Set (No Limbs - Just Bone Skeleton)
    /// + Animation Baking Helper
    /// </summary>
    [System.Serializable]
    public class ADArmatureSetup
    {
        public List<ADBoneReference> BonesSetup = new List<ADBoneReference>();
        private List<Transform> gatheredBones = new List<Transform>();

        public ADBoneReference RootBoneReference = new ADBoneReference(null, -1, null);
        public ADBoneReference PelvisBoneReference = new ADBoneReference(null, -1, null);
        public ADBoneReference PelvisAdditionalRef = new ADBoneReference(null, -1, null);

        public Transform Root { get { return RootBoneReference.TempTransform; } }

        [Tooltip("Some humanoid rigs may require using other root bone than animator transform for correct export.\n(Using 'Skeleton Root' field)")]
        public bool UseRootBoneForAvatar = false;

        #region Setting Up Armature

        internal void Prepare(Transform anim)
        {
            LatestAnimator = anim;

            if (RootBoneReference.ID == 0) RootBoneReference.GatherTempTransform(anim.transform);
            if (PelvisBoneReference.ID == 0) PelvisBoneReference.GatherTempTransform(anim.transform);

            gatheredBones.Clear();
            List<SkinnedMeshRenderer> skins = FTransformMethods.FindComponentsInAllChildren<SkinnedMeshRenderer>(anim.transform, true);

            for (int s = 0; s < skins.Count; s++)
            {
                for (int b = 0; b < skins[s].bones.Length; b++)
                {
                    var bone = skins[s].bones[b];
                    //UnityEngine.Debug.Log("bone[" + b + "]: " + skins[s].bones[b].name);
                    AddBone(bone, anim);
                }
            }


            if (PelvisBoneReference.TempTransform == null)
                if (RootBoneReference.TempTransform != null)
                {
                    SetPelvisRef(RootBoneReference.TempTransform.GetChild(0));
                }


            if (PelvisBoneReference.TempTransform != null)
            {
                AddBone(PelvisBoneReference.TempTransform, anim);
            }

            if (gatheredBones.Count < 4)
            {
                UnityEngine.Debug.Log("[Animation Designer] Found less than 4 bones in the " + anim.name + " : forcing gathering all child transforms of " + anim.name + ", unfold upper tab of Animation Designer and check 'armature report' to remove not wanted bones to animate.");

                var allTr = anim.GetComponentsInChildren<Transform>();
                for (int i = 0; i < allTr.Length; i++)
                {
                    AddBone(allTr[i], anim);
                }
            }

            SetCorrectBonesOrder();
        }


        bool ContainedBy(Transform target, List<SkinnedMeshRenderer> skins)
        {
            for (int i = 0; i < skins.Count; i++)
            {
                if (skins[i].rootBone == target) return true;
                if (skins[i].bones.Contains(target)) return true;
            }

            return false;
        }


        ADBoneReference AddBone(Transform t, Transform anim)
        {
            if (gatheredBones.Contains(t)) return null; // Already in list!

            ADBoneReference aRef = new ADBoneReference(t, gatheredBones.Count, anim);
            BonesSetup.Add(aRef);
            gatheredBones.Add(t);

            return aRef;
        }

        internal Bounds CalculateBounds()
        {
            if (Root == null) return new Bounds();
            if (BonesSetup == null) return new Bounds();

            Bounds bounds = new Bounds(Root.transform.position, Vector3.zero);

            for (int i = 0; i < BonesSetup.Count; i++)
            {
                var b = BonesSetup[i];
                if (b == null) continue;
                if (b.TempTransform == null) continue;
                bounds.Encapsulate(b.TempTransform.position);
            }

            return bounds;
        }


        #endregion


        #region Controllling Armature Transform Structure


        public void RefreshBonesSpecifics(Transform anim)
        {
            Animator a = anim.GetAnimator();
            bool human = false; if (a) human = a.isHuman;

            for (int i = 0; i < BonesSetup.Count; i++)
            {
                var b = BonesSetup[i];
                b.DefineDepth(anim);

                if (human == false) b.DefineBakePathName(anim.transform);
                else b.DefineHumanoidBone(a);
            }
        }

        public void ValidateBones()
        {
            for (int i = BonesSetup.Count - 1; i >= 0; i--)
            {
                var b = BonesSetup[i];
                if (string.IsNullOrWhiteSpace(b.BoneName)) BonesSetup.RemoveAt(i);
            }
        }


        public void GatherBones(Transform anim)
        {
            ValidateBones();

            if (RootBoneReference.ID == 0) RootBoneReference.GatherTempTransform(anim.transform);
            if (PelvisBoneReference.ID == 0) PelvisBoneReference.GatherTempTransform(anim.transform);


            #region Backup

            //if (RootBoneReference.ID == 0) { RootBoneReference.GatherTempTransform(anim.transform); }
            //else if (RootBoneReference.ID == -1) if (RootBoneReference.TempTransform == null) { RootBoneReference.GatherTempTransform(anim.transform); }

            //if (PelvisBoneReference.ID == -1)
            //{
            //    if (PelvisBoneReference.TempTransform == null)
            //    {
            //        if (anim.isHuman)
            //            PelvisBoneReference.GatherTempTransform(anim.GetBoneTransform(HumanBodyBones.Hips));
            //        else
            //        {
            //            if (anim.transform.root)
            //                PelvisBoneReference.GatherTempTransform(anim.transform.root);
            //        }
            //    }
            //}
            //else if (PelvisBoneReference.ID == 0) PelvisBoneReference.GatherTempTransform(anim.transform);

            #endregion


            gatheredBones.Clear();
            var rootBones = anim.GetComponentsInChildren<Transform>(true);


            for (int i = 0; i < BonesSetup.Count; i++)
                for (int r = 0; r < rootBones.Length; r++)
                    if (rootBones[r].name == BonesSetup[i].BoneName)
                    {
                        if (ADBoneReference.GetDepth(rootBones[r], anim.transform) == BonesSetup[i].Depth)
                        {
                            BonesSetup[i].TempTransform = rootBones[r];
                            gatheredBones.Add(rootBones[r]);
                        }
                    }
        }


        ADBoneReference GetBoneSetupOf(Transform tempTransform)
        {
            for (int i = 0; i < BonesSetup.Count; i++)
            {
                if (BonesSetup[i].TempTransform == tempTransform) return BonesSetup[i];
            }

            return null;
        }


        internal Transform GetBoneWithName(string name)
        {
            for (int i = 0; i < BonesSetup.Count; i++)
            {
                if (BonesSetup[i].BoneName == name) return BonesSetup[i].TempTransform;
            }

            return null;
        }


        /// <summary>
        /// Lowest depth bones should be first
        /// </summary>
        void SetCorrectBonesOrder()
        {
            BonesSetup = BonesSetup.OrderBy(o => o.Depth).ToList();
        }

        internal void SetPelvisRef(Transform t)
        {
            PelvisBoneReference = new ADBoneReference(t, 0, null);
            PelvisBoneReference.BoneName = t.name;
        }

        internal void SetRootBoneRef(Transform t)
        {
            RootBoneReference = new ADBoneReference(t, 0, null);
            RootBoneReference.BoneName = t.name;
        }


        #endregion


        internal void VerifyArmatureWithAnimationClip(AnimationClip clip, bool dialogs, UnityEngine.Object toDirty)
        {
            if (clip == null) return;

            if (clip.isHumanMotion)
            {
                if (dialogs) EditorUtility.DisplayDialog("Cant verify", "Cant verify generic rig with humanoid animation", "Ok");
            }
            else if (LatestAnimator != null)
            {
                EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(clip);
                List<Transform> bindBones = new List<Transform>();

                int positionT = 0;
                int rotationT = 0;
                int scaleT = 0;
                int preBones = BonesSetup.Count;

                for (int i = 0; i < bindings.Length; i++)
                {
                    var bind = bindings[i];
                    UnityEngine.Object animated = AnimationUtility.GetAnimatedObject(LatestAnimator.gameObject, bind);
                    Transform animT = animated as Transform;

                    bool isPosition = bind.propertyName.Contains("Position");
                    bool isRotation = false; if (!isPosition) isRotation = bind.propertyName.Contains("Rotation");
                    bool isScale = false; if (!isPosition && !isRotation) isScale = bind.propertyName.Contains("Scale");

                    if (animT)
                    {
                        var boneSetup = GetBoneSetupOf(animT);
                        if (boneSetup != null)
                        {
                            if (isPosition) if (boneSetup.BakePosition == false) { positionT += 1; boneSetup.BakePosition = true; }
                            if (isRotation) if (boneSetup.BakeBone == false) { rotationT += 1; boneSetup.BakeBone = true; }
                            if (isScale) if (boneSetup.BakeScale == false) { scaleT += 1; boneSetup.BakeScale = true; }
                        }

                        if (!bindBones.Contains(animT))
                        {
                            bindBones.Add(animT);
                            if (boneSetup == null) // Add new armature reference bone
                            {
                                var newBoneS = AddBone(animT, LatestAnimator);
                                if (newBoneS != null)
                                {
                                    newBoneS.BakePosition = isPosition;
                                    newBoneS.BakeScale = isScale;
                                }
                            }
                        }
                    }
                }


                if (dialogs)
                {
                    int newBones = BonesSetup.Count - preBones;

                    string report = "Clip Verify Report:\n\n";

                    if (newBones > 0) report += "New Armature Bones = " + newBones + "\n";
                    if (positionT > 0) report += "Position Bake Switch For = " + positionT + "\n";
                    if (rotationT > 0) report += "Rotation Bake Switch For = " + rotationT + "\n";
                    if (scaleT > 0) report += "Scale Bake Switch For = " + scaleT + "\n";

                    EditorUtility.DisplayDialog("Verify Clip Report", report, "Ok");
                }

                if (toDirty) EditorUtility.SetDirty(toDirty);
                //    AnimationUtility.GetAnimatedObject
                //AnimationUtility.
                //if (armatureVerifyWith.)
            }
            else
            {
                if (dialogs) EditorUtility.DisplayDialog("Cant verify", "Cant verify because no Animator was found!", "Ok");
            }
        }


        #region Controlling Bake Process

        public ADArmatureBakeHelper bake { get; private set; }
        public ADRootMotionBakeHelper rootBake { get; private set; }
        public Transform LatestAnimator { get; set; }
        public bool Humanoid { get { return LatestAnimator.IsHuman(); } }
        public List<ADBoneReference> BakeBonesSetup = new List<ADBoneReference>();

        internal void Bake_Prepare(Transform anim, AnimationClip originalClip, AnimationDesignerSave save, ADClipSettings_Main main)
        {
            LatestAnimator = anim;

            for (int i = 0; i < BonesSetup.Count; i++)
            {
                var b = BonesSetup[i];
                b.ResetCurvesAndNamesForBaking(anim);
            }

            if (RootBoneReference != null)
            {
                RootBoneReference._Bake_IsRoot = true;
                RootBoneReference.ResetCurvesAndNamesForBaking(anim);
            }

            if (PelvisBoneReference != null) PelvisBoneReference.ResetCurvesAndNamesForBaking(anim);

            rootBake = null;
            bake = new ADArmatureBakeHelper(this, originalClip, main, save);
            bake.PrepareAndDefine();

            RootBoneReference.UseAdditionalFramesLoop = ADBoneReference.DoingIdleWrap;
            RootBoneReference.DontDoInitialFramesWrap = false;

            BakeBonesSetup.Clear();

            if (bake.Humanoid)
            {
                RootBoneReference.ResetCurvesAndNamesForBaking(anim);
                RootBoneReference.BoneBakePathName = "Root";
                RootBoneReference.BakePosition = true;
                RootBoneReference.BakeBone = true;
                RootBoneReference.HumanoidBoneDefined = true;
                RootBoneReference.PrepareBakePropertyNames();
                RootBoneReference.DontDoInitialFramesWrap = originalClip.hasRootCurves;
                PelvisBoneReference.DontDoInitialFramesWrap = false;
                PelvisBoneReference.UseAdditionalFramesLoop = ADBoneReference.DoingIdleWrap;
            }
            else
            {
                RootBoneReference.HumanoidBoneDefined = false;
                RootBoneReference.BakeBone = true;
                RootBoneReference.BakePosition = true;
                RootBoneReference.DefineBakePathName(anim.transform);

                PelvisBoneReference.HumanoidBoneDefined = false;
                PelvisBoneReference.BakePosition = true;
                PelvisBoneReference.BakeBone = true;
                PelvisBoneReference.DefineBakePathName(anim.transform);

                RootBoneReference._Bake_PivotSpace = true;
                RootBoneReference._Bake_PivotPosition = LatestAnimator.transform.position;
                RootBoneReference._Bake_PivotRotation = LatestAnimator.transform.rotation;

            }

            rootBake = new ADRootMotionBakeHelper(LatestAnimator, RootBoneReference, save, main, originalClip);
            rootBake.ResetForBaking();

            if (bake.Humanoid)
            {
                if (bake.OriginalClipWithAnyRootMotion)
                    // When copying root motion from the root -> we shouldn't keep custom root motion in it
                    rootBake.KeepMotionKeyframesOnRoot = false;
                else // When baking new - humanoid - root motion -> root motion translation must be also in root bone
                    rootBake.KeepMotionKeyframesOnRoot = true;
            }
            else
            {
                if (bake.OriginalClipWithAnyRootMotion)
                    // When copying root motion from the root
                    rootBake.KeepMotionKeyframesOnRoot = true;
                else // When baking new - generic - root motion
                    rootBake.KeepMotionKeyframesOnRoot = save.Export_IncludeRootMotionInKeyAnimation;
            }


            #region Preparing Bones which should be baked


            Transform requestBone = null;

            PelvisBoneReference.UseAdditionalFramesLoop = !ADBoneReference.DoingIdleWrap;

            // Handling some exception
            if (PelvisBoneReference.TempTransform != null)
                if (PelvisBoneReference.TempTransform.parent != Root)
                    requestBone = PelvisBoneReference.TempTransform.parent;

            if (bake.Humanoid == false)
                if (!BonesSetup.Contains(PelvisBoneReference))
                {
                    BakeBonesSetup.Add(PelvisBoneReference);
                }

            bool requestMet = requestBone == null;

            for (int i = 0; i < BonesSetup.Count; i++)
            {
                if (BonesSetup[i].BakeBone)
                {
                    if (BonesSetup[i].TempTransform == PelvisBoneReference.TempTransform) continue;

                    BakeBonesSetup.Add(BonesSetup[i]);
                    if (requestBone != null) if (BonesSetup[i].TempTransform == requestBone) { requestMet = true; }
                }
            }


            // Handling some exception
            if (!requestMet)
            {
                ADBoneReference adRef = new ADBoneReference(requestBone, 1, anim);
                adRef.ResetCurvesAndNamesForBaking(anim);
                adRef.BakeBone = true;
                adRef.BakePosition = true;
                adRef.UseAdditionalFramesLoop = ADBoneReference.DoingIdleWrap;
                BakeBonesSetup.Add(adRef);
            }

            if (save.Export_BakeRootIndividually == false)
            {
                // validating if something not went wrong way
                for (int i = BakeBonesSetup.Count - 1; i >= 0; i--)
                {
                    if (BakeBonesSetup[i].TempTransform == RootBoneReference.TempTransform)
                    {
                        BakeBonesSetup.RemoveAt(i);
                        continue;
                    }
                }
            }
            else
            {
                if (BakeBonesSetup.Contains(RootBoneReference) == false) BakeBonesSetup.Add(RootBoneReference);
                if (BakeBonesSetup.Contains(PelvisBoneReference) == false) BakeBonesSetup.Add(PelvisBoneReference);
            }

            #endregion

        }

        public void Bake_CaptureFramePose(float elapsed)
        {
            bake.CaptureArmaturePoseFrame(elapsed);

            RootBoneReference.BakeCurrentState(elapsed, bake.bodyPosition, bake.bodyRotation);

            if (rootBake != null) rootBake.BakeCurrentState(elapsed);

            for (int a = 0; a < BakeBonesSetup.Count; a++)
            {
                var b = BakeBonesSetup[a];
                b.BakeCurrentState(this, elapsed);
            }
        }

        public static readonly float _LowestCompr = 0.000001f;

        internal void Bake_Complete(ref AnimationClip clip, AnimationDesignerSave save, AnimationClip originalClip, ADClipSettings_Main main)
        {

            #region Defining Bones Compression Factor

            if (save.Export_HipsAndLegsPrecisionBoost > 0f)
            {
                var ikSet = save.GetSetupForClip(save.IKSetupsForClips, originalClip, AnimationDesignerWindow._toSet_SetSwitchToHash);

                float tgt = save.Export_OptimizeCurves * 0.8f * (1f - save.Export_HipsAndLegsPrecisionBoost);
                tgt = Mathf.Max(_LowestCompr, tgt);

                if (ikSet != null)
                {
                    // Lowering compression for leg bones
                    for (int l = 0; l < save.Limbs.Count; l++)
                    {
                        var limbIkSet = ikSet.GetIKSettingsForLimb(save.Limbs[l], save);
                        if (limbIkSet == null) continue;

                        if (limbIkSet.IKType == ADClipSettings_IK.IKSet.EIKType.FootIK)
                        {
                            for (int b = 0; b < save.Limbs[l].Bones.Count; b++)
                            {
                                var bn = save.Limbs[l].Bones[b];

                                for (int bk = 0; bk < BakeBonesSetup.Count; bk++)
                                {
                                    if (BakeBonesSetup[bk].TempTransform == bn.T)
                                    {
                                        BakeBonesSetup[bk].CompressionFactor = tgt;
                                        BakeBonesSetup[bk].UseAdditionalFramesLoop = !ADBoneReference.DoingIdleWrap;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }

                // Find if there is bone between root and pelvis
                if (PelvisBoneReference.TempTransform.parent != RootBoneReference.TempTransform)
                {
                    Transform toUncompress = PelvisBoneReference.TempTransform.parent;
                    for (int i = 0; i < BakeBonesSetup.Count; i++)
                    {
                        if (BakeBonesSetup[i].TempTransform == toUncompress)
                        {
                            BakeBonesSetup[i].UseAdditionalFramesLoop = ADBoneReference.DoingIdleWrap;
                            BakeBonesSetup[i].CompressionFactor = tgt;
                            break;
                        }
                    }
                }

                // Root and pelvis compression
                RootBoneReference.CompressionFactor = tgt;
                PelvisBoneReference.CompressionFactor = tgt;

                if (bake.Humanoid) // Humanoid IK compression set
                {
                    for (int i = 0; i < bake.Ar.BakeBonesSetup.Count; i++)
                    {
                        var bn = bake.Ar.BakeBonesSetup[i];
                        if (bn.HumanoidBoneDefined == false) continue;
                        if (bn.HumanoidBodyBone == HumanBodyBones.LeftFoot || bn.HumanoidBodyBone == HumanBodyBones.RightFoot || bn.HumanoidBodyBone == HumanBodyBones.Hips)
                        {
                            bn.CompressionFactor = tgt;
                        }
                    }
                }

                // Making sure pelvis bone compression was assigned
                for (int bk = 0; bk < BakeBonesSetup.Count; bk++)
                {
                    if (BakeBonesSetup[bk].TempTransform == PelvisBoneReference.TempTransform)
                    {
                        BakeBonesSetup[bk].CompressionFactor = tgt;
                        break;
                    }
                }
            }
            else
            {
                RootBoneReference.CompressionFactor = 1f;
                for (int a = 0; a < BakeBonesSetup.Count; a++) BakeBonesSetup[a].CompressionFactor = 1f;
            }

            #endregion



            if (bake.Humanoid) // Humanoid
            {

                #region Root motion related

                if (main.Export_DisableRootMotionExport == false && bake.OriginalClipWithAnyRootMotion)
                {
                    RootBoneReference.DontDoInitialFramesWrap = true;
                    RootBoneReference.UseAdditionalFramesLoop = false;

                    PelvisBoneReference.DontDoInitialFramesWrap = true;
                    PelvisBoneReference.UseAdditionalFramesLoop = false;
                }
                else // No root motion in original clip
                {
                    if (main.Export_DisableRootMotionExport == false)
                    {
                        if (rootBake != null)
                        {
                            rootBake.SaveRootMotionPositionCurves(ref clip);
                            rootBake.SaveRootMotionRotationCurves(ref clip);

                            if (rootBake.BakedSomePositionRootMotion())
                            {
                                RootBoneReference.DontDoInitialFramesWrap = true;
                                RootBoneReference.UseAdditionalFramesLoop = false;
                            }
                        }
                    }
                }

                // Copy original root motion curves
                if (main.Export_DisableRootMotionExport == false && bake.OriginalClipWithAnyRootMotion)
                {
                    string motionStr = "Motion";

                    if (ADRootMotionBakeHelper.ClipContainsAnyRootCurves(originalClip, motionStr))
                        rootBake.PreapreOriginalRootMotionFrom(originalClip, false);

                    //if (!rootBake.DetectedBakedMotion())
                    //{
                    //    rootBake.CopyRootMotionFrom(originalClip);
                    //}

                    if (main.Export_JoinRootMotion)
                    {
                        rootBake.SaveRootMotionPositionCurves(ref clip, "Motion", originalClip);
                        rootBake.SaveRootMotionRotationCurves(ref clip, "Motion", originalClip);
                    }
                    else
                    {
                        rootBake.SaveRootMotionPositionCurves(ref clip);
                        rootBake.SaveRootMotionRotationCurves(ref clip);
                    }
                }

                #endregion

                RootBoneReference.SaveCurvesForClip(main.Export_WrapLoopBakeMode, ref clip, save.Export_OptimizeCurves * 0.6f, true);

            }
            else // Generic
            {
                if (main.Export_DisableRootMotionExport == false)
                {

                    //bool motionInRootInsteadOfMotion = false; // Just infiuriating exception handling... 

                    // Copy original root motion curves
                    if (bake.OriginalClipWithAnyRootMotion)
                    {
                        bool allowRoot = false;
                        if (bake.BakeMain != null) allowRoot = bake.BakeMain.Export_ForceRootMotion;

                        string motionStr = rootBake.CheckForMotionOrRootTag(originalClip, allowRoot);

                        if (ADRootMotionBakeHelper.ClipContainsAnyRootCurves(originalClip, motionStr))
                            rootBake.PreapreOriginalRootMotionFrom(originalClip, allowRoot);

                        //if (!rootBake.DetectedBakedMotion())
                        //{
                        //    //rootBake.CheckForMotionOrRootTag(originalClip, allowRoot);//CopyRootMotionFrom(originalClip, allowRoot);
                        //}

                        //if (allowRoot) motionInRootInsteadOfMotion = rootBake.DetectedMotionInRootInsteadOfMotion;

                        if (!main.Export_JoinRootMotion)
                        {
                            rootBake.SaveRootMotionPositionCurves(ref clip, motionStr);
                            rootBake.SaveRootMotionRotationCurves(ref clip, motionStr);
                        }
                        else
                        {
                            rootBake.SaveRootMotionPositionCurves(ref clip, motionStr, originalClip);
                            rootBake.SaveRootMotionRotationCurves(ref clip, motionStr, originalClip);
                        }
                    }
                    else // Saving new root motion to the animation
                    {
                        float posMagn = RootBoneReference.ComputePositionCurvesMagnitude();
                        if (posMagn > 0.0001f)
                        {
                            //RootBoneReference.SaveRootMotionPositionCurves(ref clip);
                            if (!save.Export_IncludeRootMotionInKeyAnimation) RootBoneReference.ResetPositionCurves();
                        }

                        float RotMagn = RootBoneReference.ComputeRotationCurvesMagnitude();
                        if (RotMagn > 0.001f)
                        {
                            //RootBoneReference.SaveRootMotionRotationCurves(ref clip);
                            if (!save.Export_IncludeRootMotionInKeyAnimation) RootBoneReference.ResetRotationCurves();
                        }

                        if (rootBake != null)
                        {
                            rootBake.SaveRootMotionPositionCurves(ref clip);
                            rootBake.SaveRootMotionRotationCurves(ref clip);
                        }
                    }

                    if (save.Export_IncludeRootMotionInKeyAnimation)
                    {
                        RootBoneReference.SaveCurvesForClip(main.Export_WrapLoopBakeMode, ref clip, save.Export_OptimizeCurves * 0.6f, false);
                    }
                }
            }


            for (int a = 0; a < BakeBonesSetup.Count; a++)
            {
                var b = BakeBonesSetup[a];
                if (b == null) continue;
                b.SaveCurvesForClip(main.Export_WrapLoopBakeMode, ref clip, save.Export_OptimizeCurves * 0.8f);
            }

            bake.SaveHumanoidCurves(ref clip, save.Export_OptimizeCurves, save.Export_HipsAndLegsPrecisionBoost);


            #region Save leg grounding curves

            var iks = save.GetSetupForClip<ADClipSettings_IK>(save.IKSetupsForClips, originalClip, AnimationDesignerWindow._toSet_SetSwitchToHash);

            if (iks != null)
                for (int i = 0; i < iks.LimbIKSetups.Count; i++)
                {
                    var ikLimb = iks.LimbIKSetups[i];
                    if (ikLimb.IKType != ADClipSettings_IK.IKSet.EIKType.FootIK) continue;
                    //if (ikLimb.LegMode == ADClipSettings_IK.IKSet.ELegMode.BasicIK) continue;
                    if (ikLimb.ExportGroundingCurve == false) continue;
                    if (ikLimb.FootDataAnalyze == null) continue;

                    EditorCurveBinding bind = new EditorCurveBinding();
                    bind.type = typeof(Animator);
                    bind.propertyName = ikLimb.GetName + "-H";

                    AnimationCurve tgtCurve = AnimationDesignerWindow.CopyCurve(ikLimb.FootDataAnalyze.GroundingCurve);

                    if (tgtCurve.keys.Length > 1)
                    {
                        if (tgtCurve.keys[0].time > 0f)
                        {
                            tgtCurve.AddKey(new Keyframe(0f, tgtCurve.keys[0].value));
                        }

                        if (tgtCurve.keys[tgtCurve.keys.Length - 1].time < 1f)
                        {
                            tgtCurve.AddKey(new Keyframe(1f, tgtCurve.keys[tgtCurve.keys.Length - 1].value));
                        }
                    }

                    AnimationGenerateUtils.DistrubuteCurveOnTime(ref tgtCurve, 0f, clip.length);
                    AnimationUtility.SetEditorCurve(clip, bind, tgtCurve);
                }

            #endregion


        }


        internal void DampSessionReferences(bool dampLatestAnimator = true)
        {
            if (dampLatestAnimator)
                LatestAnimator = null;

            if (RootBoneReference != null) RootBoneReference.TempTransform = null;
            if (PelvisBoneReference != null) PelvisBoneReference.TempTransform = null;

            for (int i = 0; i < BonesSetup.Count; i++)
            {
                BonesSetup[i].TempTransform = null;
            }
        }


        #endregion


    }
}