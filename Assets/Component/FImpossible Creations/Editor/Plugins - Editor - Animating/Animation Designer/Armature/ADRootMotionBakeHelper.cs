using System;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.AnimationTools
{
    /// <summary>
    /// Just for generic rigs
    /// </summary>
    public class ADRootMotionBakeHelper
    {
        public Transform AnimatorTransform { get; private set; }
        public Transform RootMotionTransform { get; private set; }
        public AnimationDesignerSave Save { get; private set; }
        public ADClipSettings_Main MainSet { get; private set; }
        public Animator Mecanim { get; private set; }
        public ADBoneReference RootRef { get; private set; }
        public AnimationClip BakingClip { get; private set; }
        public bool KeepMotionKeyframesOnRoot = false;
        Quaternion rootMapping;
        private float ScaleOffset = 1f;

        public ADRootMotionBakeHelper(Transform animatorTr, ADBoneReference rootRef, AnimationDesignerSave save, ADClipSettings_Main main, AnimationClip targetClip)
        {
            AnimatorTransform = animatorTr;
            RootRef = rootRef;
            RootMotionTransform = rootRef.TempTransform;
            if (RootMotionTransform == animatorTr) if (save) if (save.ReferencePelvis) RootMotionTransform = save.ReferencePelvis;
            Save = save;
            MainSet = main;
            KeepMotionKeyframesOnRoot = false;
            DetectedMotionInRootInsteadOfMotion = false;
            Mecanim = animatorTr.GetAnimator();
            ScaleOffset = 1f;
            BakingClip = targetClip;
        }

        public void ResetForBaking()
        {
            AnimationDesignerWindow.ForceTPose();

            PrepareRootMotionPosition();
            PrepareRootMotionRotation();

            startBakePos = RootMotionTransform.position;
            startBakeRot = RootMotionTransform.rotation;
            latestRootMotionPos = Vector3.zero;
            latestRootMotionRot = Quaternion.identity;
            latestRootMotionRotEnsure = Quaternion.identity;

            rootMapping = Quaternion.FromToRotation(RootMotionTransform.InverseTransformDirection(AnimatorTransform.right), Vector3.right);
            rootMapping *= Quaternion.FromToRotation(RootMotionTransform.InverseTransformDirection(AnimatorTransform.up), Vector3.up);
        }

        Vector3 startBakePos;
        Quaternion startBakeRot;

        Vector3 latestAnimatorPos;
        Quaternion latestAnimatorRot;

        Vector3 latestPos;
        Quaternion latestRot;

        public Vector3 latestRootMotionPos { get; private set; }
        public Quaternion latestRootMotionRot { get; private set; }

        // Just infiuriating exception handling... 
        public bool DetectedMotionInRootInsteadOfMotion { get; internal set; }

        Quaternion latestRootMotionRotEnsure;

        public static Vector3 RootModsOffsetAccumulation = Vector3.zero;
        public static Vector3 RootModsRotOffsetAccumulation = Vector3.zero;

        public void PostAnimator()
        {
            latestAnimatorPos = RootMotionTransform.position;
            latestAnimatorRot = RootMotionTransform.rotation;
        }

        public void PostRootMotion()
        {
            if (Mecanim) { if (Mecanim.isHuman && AnimationDesignerWindow._forceExportGeneric == false) { ScaleOffset = Mecanim.humanScale; } else ScaleOffset = 1f; } else ScaleOffset = 1f;
            Vector3 rootRefPos = latestAnimatorPos;
            if (AnimationDesignerWindow._forceExportGeneric) rootRefPos = startBakePos;

            Vector3 diff = RootMotionTransform.position - (rootRefPos);
            Vector3 local = AnimatorTransform.InverseTransformVector(diff / ScaleOffset);

            //UnityEngine.Debug.Log("diff = " + diff);
            latestRootMotionPos = local;

            //Quaternion rDiff = RootMotionTransform.rotation * Quaternion.Inverse(latestAnimatorRot);
            //rDiff = RootMotionTransform.rotation;
            //latestRootMotionRot = (rDiff) * Quaternion.Inverse(rootMapping);
            latestRootMotionRot = RootMotionTransform.rotation * Quaternion.Inverse(latestAnimatorRot);
            //Debug.Log("latestRootMotionRot = " + latestRootMotionRot.eulerAngles + " rootEul = " + latestRootMotionRot);
            //latestRootMotionRot = latestRootMotionRot;// * Quaternion.Inverse(rootMapping);
            latestPos = RootMotionTransform.position;

            if (KeepMotionKeyframesOnRoot == false) // Restoring state before root modificator
            {
                bool stripRootMot = true;
                if (Mecanim) if (Mecanim.applyRootMotion) if (BakingClip.hasRootCurves) stripRootMot = false;

                if (stripRootMot)
                {
                    // Stripping root motion out of keyframed animation
                    RootMotionTransform.position = latestAnimatorPos;
                    RootMotionTransform.rotation = latestAnimatorRot;
                }
            }
        }


        #region Just initializing curves


        [NonSerialized] public AnimationCurve _Bake_RootMPosX;
        [NonSerialized] public AnimationCurve _Bake_RootMPosY;
        [NonSerialized] public AnimationCurve _Bake_RootMPosZ;

        [NonSerialized] public AnimationCurve _Original_RootMPosX;
        [NonSerialized] public AnimationCurve _Original_RootMPosY;
        [NonSerialized] public AnimationCurve _Original_RootMPosZ;

        /// <summary> Just for generic rigs root motion </summary>
        void PrepareRootMotionPosition()
        {
            _Bake_RootMPosX = new AnimationCurve();
            _Bake_RootMPosY = new AnimationCurve();
            _Bake_RootMPosZ = new AnimationCurve();

            _Original_RootMPosX = new AnimationCurve();
            _Original_RootMPosY = new AnimationCurve();
            _Original_RootMPosZ = new AnimationCurve();
        }


        [NonSerialized] public AnimationCurve _Bake_RootMRotX;
        [NonSerialized] public AnimationCurve _Bake_RootMRotY;
        [NonSerialized] public AnimationCurve _Bake_RootMRotZ;
        [NonSerialized] public AnimationCurve _Bake_RootMRotW;

        [NonSerialized] public AnimationCurve _Original_RootMRotX;
        [NonSerialized] public AnimationCurve _Original_RootMRotY;
        [NonSerialized] public AnimationCurve _Original_RootMRotZ;
        [NonSerialized] public AnimationCurve _Original_RootMRotW;

        /// <summary> Just for generic rigs root motion </summary>
        void PrepareRootMotionRotation()
        {
            _Bake_RootMRotX = new AnimationCurve();
            _Bake_RootMRotY = new AnimationCurve();
            _Bake_RootMRotZ = new AnimationCurve();
            _Bake_RootMRotW = new AnimationCurve();

            _Original_RootMRotX = new AnimationCurve();
            _Original_RootMRotY = new AnimationCurve();
            _Original_RootMRotZ = new AnimationCurve();
            _Original_RootMRotW = new AnimationCurve();
        }

        #endregion


        /// <summary> Just for generic rigs </summary>
        public void SaveRootMotionPositionCurves(ref AnimationClip clip, string motionStr = "Motion", AnimationClip joinWith = null)
        {
            if (_Bake_RootMPosX == null || _Bake_RootMPosY == null || _Bake_RootMPosZ == null) return;

            if (_Original_RootMPosX != null && (_Original_RootMPosX.length > 0 || _Original_RootMPosY.length > 0 || _Original_RootMPosZ.length > 0))
                if (MainSet.IsUsingDefaultTimeEvaluation() == false)
                {
                    _Original_RootMPosX = ModifyCurveWithTimeWarpCurve(_Original_RootMPosX, MainSet.ClipEvaluateTimeCurve, BakingClip.length);
                    _Original_RootMPosY = ModifyCurveWithTimeWarpCurve(_Original_RootMPosY, MainSet.ClipEvaluateTimeCurve, BakingClip.length);
                    _Original_RootMPosZ = ModifyCurveWithTimeWarpCurve(_Original_RootMPosZ, MainSet.ClipEvaluateTimeCurve, BakingClip.length);
                }

            if (GetRootPositionCurvesMagnitude() < 0.0001f)
            {
                //_Bake_RootMPosX = AnimationDesignerWindow.CopyCurve(GetEditorCurve(BakingClip, motionStr + "T.x"));
                //_Bake_RootMPosY = AnimationDesignerWindow.CopyCurve(GetEditorCurve(BakingClip, motionStr + "T.y"));
                //_Bake_RootMPosZ = AnimationDesignerWindow.CopyCurve(GetEditorCurve(BakingClip, motionStr + "T.z"));

                return;
            }

            if (joinWith != null)
            {
                //UnityEngine.Debug.Log("joint");
                //AnimationCurve orig_x = AnimationDesignerWindow.CopyCurve(GetEditorCurve(BakingClip, motionStr + "T.x"));
                //AnimationCurve orig_y = AnimationDesignerWindow.CopyCurve(GetEditorCurve(BakingClip, motionStr + "T.y"));
                //AnimationCurve orig_z = AnimationDesignerWindow.CopyCurve(GetEditorCurve(BakingClip, motionStr + "T.z"));

                _Bake_RootMPosX = JoinAdditiveCurves(_Bake_RootMPosX, _Original_RootMPosX);
                _Bake_RootMPosY = JoinAdditiveCurves(_Bake_RootMPosY, _Original_RootMPosY);
                _Bake_RootMPosZ = JoinAdditiveCurves(_Bake_RootMPosZ, _Original_RootMPosZ);
            }
            else
            {
                _Bake_RootMPosX = _Original_RootMPosX;
                _Bake_RootMPosY = _Original_RootMPosY;
                _Bake_RootMPosZ = _Original_RootMPosZ;
            }


            if (MainSet.Export_RootMotionTangents != ADClipSettings_Main.ERootMotionCurveAdjust.None)
                if (MainSet.ClipDurationMultiplier != 1f)
                    if (_Bake_RootMPosZ.length > 0)
                    {
                        float start = _Bake_RootMPosZ.keys[0].time;
                        float newLen = BakingClip.length * MainSet.ClipDurationMultiplier;
                        AnimationGenerateUtils.DistrubuteCurveOnTime(ref _Bake_RootMPosX, start, newLen);
                        AnimationGenerateUtils.DistrubuteCurveOnTime(ref _Bake_RootMPosY, start, newLen);
                        AnimationGenerateUtils.DistrubuteCurveOnTime(ref _Bake_RootMPosZ, start, newLen);
                    }


            if (MainSet.Export_RootMotionTangents == ADClipSettings_Main.ERootMotionCurveAdjust.LinearTangents)
            {
                _Bake_RootMPosX = LinearizeCurveTangents(_Bake_RootMPosX);
                _Bake_RootMPosY = LinearizeCurveTangents(_Bake_RootMPosY);
                _Bake_RootMPosZ = LinearizeCurveTangents(_Bake_RootMPosZ);
            }
            else if (MainSet.Export_RootMotionTangents == ADClipSettings_Main.ERootMotionCurveAdjust.SmoothTangents)
            {
                _Bake_RootMPosX = SmoothCurveTangents(_Bake_RootMPosX);
                _Bake_RootMPosY = SmoothCurveTangents(_Bake_RootMPosY);
                _Bake_RootMPosZ = SmoothCurveTangents(_Bake_RootMPosZ);
            }

            clip.SetCurve("", typeof(Animator), motionStr + "T.x", _Bake_RootMPosX);
            clip.SetCurve("", typeof(Animator), motionStr + "T.y", _Bake_RootMPosY);
            clip.SetCurve("", typeof(Animator), motionStr + "T.z", _Bake_RootMPosZ);
        }

        AnimationCurve SmoothCurveTangents(AnimationCurve original, float smooth = 1f)
        {
            AnimationCurve nCurve = AnimationDesignerWindow.CopyCurve(original);

            for (int k = 0; k < nCurve.keys.Length; k++)
            {
                nCurve.SmoothTangents(k, 1f);
            }

            return nCurve;
        }

        AnimationCurve LinearizeCurveTangents(AnimationCurve original)
        {
            AnimationCurve nCurve = AnimationDesignerWindow.CopyCurve(original);

            for (int k = 0; k < nCurve.keys.Length; k++)
            {
                AnimationUtility.SetKeyLeftTangentMode(nCurve, k, AnimationUtility.TangentMode.Linear);
                AnimationUtility.SetKeyRightTangentMode(nCurve, k, AnimationUtility.TangentMode.Linear);
            }

            return nCurve;
        }

        AnimationCurve ModifyCurveWithTimeWarpCurve(AnimationCurve original, AnimationCurve timeWarp, float originalClipLength)
        {
            //float fCount = (float)(nCurve.keys.Length - 1);
            if (original.length <= 0f) return original;

            float timeStep = (1f / (float)BakingClip.frameRate);// / MainSet.ClipDurationMultiplier;
            float elapsed = 0f;

            AnimationCurve nCurve = new AnimationCurve();

            // To make it work universally, we need to make key per frame (since there are two curves time combination)
            while (elapsed <= BakingClip.length)
            {
                float warpedTime = timeWarp.Evaluate(elapsed / originalClipLength);//(float)k / fCount
                warpedTime = Mathf.Clamp01(warpedTime);
                float targetValue = original.Evaluate(warpedTime * originalClipLength);

                nCurve.AddKey(new Keyframe(elapsed, targetValue));
                elapsed += timeStep;
            }

            //for (int k = 0; k < nCurve.keys.Length; k++)
            //{
            //    Keyframe nKey = nCurve.keys[k];
            //    float warpedTime = timeWarp.Evaluate(nKey.time / originalClipLength);//(float)k / fCount
            //    warpedTime = Mathf.Clamp01(warpedTime);
            //    nKey.value = original.Evaluate(warpedTime * originalClipLength);

            //    nCurve.MoveKey(k, nKey);
            //}

            return nCurve;
        }

        private AnimationCurve JoinAdditiveCurves(AnimationCurve a, AnimationCurve b)
        {
            AnimationCurve ac = new AnimationCurve();

            for (int k = 0; k < a.length; k++)
            {
                Keyframe kf = new Keyframe(a[k].time, a[k].value + b.Evaluate(a[k].time));
                ac.AddKey(kf);
            }

            for (int k = 0; k < b.length; k++)
            {
                Keyframe kf = new Keyframe(b[k].time, b[k].value + a.Evaluate(b[k].time));
                ac.AddKey(kf);
            }

            return ac;
        }


        /// <summary> Just for generic rigs </summary>
        public void SaveRootMotionRotationCurves(ref AnimationClip clip, string motionStr = "Motion", AnimationClip joinWith = null)
        {
            if (_Bake_RootMRotX == null || _Bake_RootMRotY == null || _Bake_RootMRotZ == null || _Bake_RootMRotW == null) return;

            if (_Original_RootMRotX != null && (_Original_RootMRotX.length > 0 || _Original_RootMRotY.length > 0 || _Original_RootMRotZ.length > 0 || _Original_RootMRotW.length > 0))
            {
                if (MainSet.IsUsingDefaultTimeEvaluation() == false)
                {
                    _Original_RootMRotX = ModifyCurveWithTimeWarpCurve(_Original_RootMRotX, MainSet.ClipEvaluateTimeCurve, BakingClip.length);
                    _Original_RootMRotY = ModifyCurveWithTimeWarpCurve(_Original_RootMRotY, MainSet.ClipEvaluateTimeCurve, BakingClip.length);
                    _Original_RootMRotZ = ModifyCurveWithTimeWarpCurve(_Original_RootMRotZ, MainSet.ClipEvaluateTimeCurve, BakingClip.length);
                    _Original_RootMRotW = ModifyCurveWithTimeWarpCurve(_Original_RootMRotW, MainSet.ClipEvaluateTimeCurve, BakingClip.length);
                }
            }



            if (joinWith != null)
            {
                _Bake_RootMRotX = JoinAdditiveCurves(_Bake_RootMRotX, _Original_RootMRotX);
                _Bake_RootMRotY = JoinAdditiveCurves(_Bake_RootMRotY, _Original_RootMRotY);
                _Bake_RootMRotZ = JoinAdditiveCurves(_Bake_RootMRotZ, _Original_RootMRotZ);
                _Bake_RootMRotW = JoinAdditiveCurves(_Bake_RootMRotW, _Original_RootMRotW);
            }
            else
            {
                _Bake_RootMRotX = _Original_RootMRotX;
                _Bake_RootMRotY = _Original_RootMRotY;
                _Bake_RootMRotZ = _Original_RootMRotZ;
                _Bake_RootMRotW = _Original_RootMRotW;
            }


            if (GetRootRotationCurvesMagnitude() < 0.0001f)
            {
                //_Bake_RootMRotX = AnimationDesignerWindow.CopyCurve(GetEditorCurve(BakingClip, motionStr + "Q.x"));
                //_Bake_RootMRotY = AnimationDesignerWindow.CopyCurve(GetEditorCurve(BakingClip, motionStr + "Q.y"));
                //_Bake_RootMRotZ = AnimationDesignerWindow.CopyCurve(GetEditorCurve(BakingClip, motionStr + "Q.z"));
                //_Bake_RootMRotW = AnimationDesignerWindow.CopyCurve(GetEditorCurve(BakingClip, motionStr + "Q.w"));
                return;
            }




            if (MainSet.Export_RootMotionTangents == ADClipSettings_Main.ERootMotionCurveAdjust.LinearTangents)
            {
                _Bake_RootMRotX = LinearizeCurveTangents(_Bake_RootMRotX);
                _Bake_RootMRotY = LinearizeCurveTangents(_Bake_RootMRotY);
                _Bake_RootMRotZ = LinearizeCurveTangents(_Bake_RootMRotZ);
                _Bake_RootMRotW = LinearizeCurveTangents(_Bake_RootMRotW);
            }
            else if (MainSet.Export_RootMotionTangents == ADClipSettings_Main.ERootMotionCurveAdjust.SmoothTangents)
            {
                _Bake_RootMRotX = SmoothCurveTangents(_Bake_RootMRotX);
                _Bake_RootMRotY = SmoothCurveTangents(_Bake_RootMRotY);
                _Bake_RootMRotZ = SmoothCurveTangents(_Bake_RootMRotZ);
                _Bake_RootMRotW = SmoothCurveTangents(_Bake_RootMRotW);
            }


            clip.SetCurve("", typeof(Animator), motionStr + "Q.x", _Bake_RootMRotX);
            clip.SetCurve("", typeof(Animator), motionStr + "Q.y", _Bake_RootMRotY);
            clip.SetCurve("", typeof(Animator), motionStr + "Q.z", _Bake_RootMRotZ);
            clip.SetCurve("", typeof(Animator), motionStr + "Q.w", _Bake_RootMRotW);
        }

        internal void BakeCurrentState(float keyTime)
        {
            Vector3 pos = latestRootMotionPos;

            _Bake_RootMPosX.AddKey(keyTime, pos.x);
            _Bake_RootMPosY.AddKey(keyTime, pos.y);
            _Bake_RootMPosZ.AddKey(keyTime, pos.z);

            Quaternion rot = AnimationGenerateUtils.EnsureQuaternionContinuity(latestRootMotionRotEnsure, latestRootMotionRot);
            latestRootMotionRotEnsure = rot;
            _Bake_RootMRotX.AddKey(keyTime, rot.x);
            _Bake_RootMRotY.AddKey(keyTime, rot.y);
            _Bake_RootMRotZ.AddKey(keyTime, rot.z);
            _Bake_RootMRotW.AddKey(keyTime, rot.w);
        }

        internal string CheckForMotionOrRootTag(AnimationClip clip, bool allowRoot = false)
        {
            string motionStr = "Motion";
            if (ClipContainsRootPositionCurves(clip) == false)
            {
                if (allowRoot) if (ClipContainsRootPositionCurves(clip, "Root"))
                    {
                        motionStr = "Root";
                        DetectedMotionInRootInsteadOfMotion = true;
                    }
            }

            return motionStr;
        }

        internal void CopyRootMotionFrom(AnimationClip clip, bool allowRoot = false)
        {
            string motionStr = CheckForMotionOrRootTag(clip, allowRoot);

            _Bake_RootMPosX = AnimationDesignerWindow.CopyCurve(GetEditorCurve(clip, motionStr + "T.x"));
            _Bake_RootMPosY = AnimationDesignerWindow.CopyCurve(GetEditorCurve(clip, motionStr + "T.y"));
            _Bake_RootMPosZ = AnimationDesignerWindow.CopyCurve(GetEditorCurve(clip, motionStr + "T.z"));

            _Bake_RootMRotX = AnimationDesignerWindow.CopyCurve(GetEditorCurve(clip, motionStr + "Q.x"));
            _Bake_RootMRotY = AnimationDesignerWindow.CopyCurve(GetEditorCurve(clip, motionStr + "Q.y"));
            _Bake_RootMRotZ = AnimationDesignerWindow.CopyCurve(GetEditorCurve(clip, motionStr + "Q.z"));
            _Bake_RootMRotW = AnimationDesignerWindow.CopyCurve(GetEditorCurve(clip, motionStr + "Q.w"));
        }

        internal void PreapreOriginalRootMotionFrom(AnimationClip clip, bool allowRoot = false)
        {
            string motionStr = CheckForMotionOrRootTag(clip, allowRoot);
            _Original_RootMPosX = AnimationDesignerWindow.CopyCurve(GetEditorCurve(clip, motionStr + "T.x"));
            _Original_RootMPosY = AnimationDesignerWindow.CopyCurve(GetEditorCurve(clip, motionStr + "T.y"));
            _Original_RootMPosZ = AnimationDesignerWindow.CopyCurve(GetEditorCurve(clip, motionStr + "T.z"));

            _Original_RootMRotX = AnimationDesignerWindow.CopyCurve(GetEditorCurve(clip, motionStr + "Q.x"));
            _Original_RootMRotY = AnimationDesignerWindow.CopyCurve(GetEditorCurve(clip, motionStr + "Q.y"));
            _Original_RootMRotZ = AnimationDesignerWindow.CopyCurve(GetEditorCurve(clip, motionStr + "Q.z"));
            _Original_RootMRotW = AnimationDesignerWindow.CopyCurve(GetEditorCurve(clip, motionStr + "Q.w"));
        }

        internal bool BakedSomePositionRootMotion()
        {
            if (_Bake_RootMPosX == null || _Bake_RootMPosY == null || _Bake_RootMPosZ == null) return false;

            float magn = GetRootPositionCurvesMagnitude();
            if (magn < 0.0001f) return false;
            return true;
        }

        internal bool BakedSomeRotationRootMotion()
        {
            if (_Bake_RootMRotX == null || _Bake_RootMRotY == null || _Bake_RootMRotZ == null || _Bake_RootMRotW == null) return false;

            float magn = GetRootRotationCurvesMagnitude();
            if (magn < 0.0001f) return false;
            return true;
        }

        internal bool DetectedBakedMotion()
        {
            if (GetRootPositionCurvesMagnitude() < 0.005f && GetRootRotationCurvesMagnitude() < 0.005f)
            {
                return false;
            }

            return true;
        }

        internal float GetRootPositionCurvesMagnitude()
        {
            float magn = ADBoneReference.ComputePositionMagn(_Bake_RootMPosX);
            magn += ADBoneReference.ComputePositionMagn(_Bake_RootMPosY);
            magn += ADBoneReference.ComputePositionMagn(_Bake_RootMPosZ);

            magn += ADBoneReference.ComputePositionMagn(_Original_RootMPosX);
            magn += ADBoneReference.ComputePositionMagn(_Original_RootMPosY);
            magn += ADBoneReference.ComputePositionMagn(_Original_RootMPosZ);

            return magn;
        }

        internal float GetRootRotationCurvesMagnitude()
        {
            float magn = ADBoneReference.ComputePositionMagn(_Bake_RootMRotX);
            magn += ADBoneReference.ComputePositionMagn(_Bake_RootMRotY);
            magn += ADBoneReference.ComputePositionMagn(_Bake_RootMRotZ);
            magn += ADBoneReference.ComputePositionMagn(_Bake_RootMRotW);

            magn += ADBoneReference.ComputePositionMagn(_Original_RootMRotX);
            magn += ADBoneReference.ComputePositionMagn(_Original_RootMRotY);
            magn += ADBoneReference.ComputePositionMagn(_Original_RootMRotZ);
            magn += ADBoneReference.ComputePositionMagn(_Original_RootMRotW);

            return magn;
        }


        internal static AnimationCurve GetEditorCurve(AnimationClip clip, string propertyPath)
        {
            EditorCurveBinding binding = EditorCurveBinding.FloatCurve(string.Empty, typeof(Animator), propertyPath);
            return AnimationUtility.GetEditorCurve(clip, binding);
        }

        internal static bool ClipContainsAnyRootCurves(AnimationClip originalClip, string motionStr = "Motion")
        {
            return ClipContainsRootPositionCurves(originalClip, motionStr) || ClipContainsRootRotationCurves(originalClip, motionStr);
        }

        static string GetMotionString(bool isHumanoid)
        {
            if (isHumanoid)
                return "Motion";
            else
                return "Root";
        }

        internal static bool ClipContainsRootPositionCurves(AnimationClip clip, string motionStr = "Motion")
        {
            var tX = GetEditorCurve(clip, motionStr + "T.x");
            if (tX == null) return false;
            var tY = GetEditorCurve(clip, motionStr + "T.y");
            if (tY == null) return false;
            var tZ = GetEditorCurve(clip, motionStr + "T.z");
            if (tZ == null) return false;

            float magn = ADBoneReference.ComputePositionMagn(tX);
            magn += ADBoneReference.ComputePositionMagn(tY);
            magn += ADBoneReference.ComputePositionMagn(tZ);
            if (magn < 0.0001f) return false;

            return true;
        }

        internal static bool ClipContainsRootRotationCurves(AnimationClip clip, string motionStr = "Motion")
        {
            var tX = GetEditorCurve(clip, motionStr + "Q.x");
            if (tX == null) return false;
            var tY = GetEditorCurve(clip, motionStr + "Q.y");
            if (tY == null) return false;
            var tZ = GetEditorCurve(clip, motionStr + "Q.z");
            if (tZ == null) return false;
            var tW = GetEditorCurve(clip, motionStr + "Q.w");
            if (tW == null) return false;

            float magn = ADBoneReference.ComputePositionMagn(tX);
            magn += ADBoneReference.ComputePositionMagn(tY);
            magn += ADBoneReference.ComputePositionMagn(tZ);
            magn += ADBoneReference.ComputePositionMagn(tW);
            if (magn < 0.0001f) return false;

            return true;
        }
    }
}