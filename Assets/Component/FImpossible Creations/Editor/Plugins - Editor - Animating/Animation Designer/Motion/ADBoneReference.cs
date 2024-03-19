using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.AnimationTools
{
    [System.Serializable]
    public class ADBoneReference
    {
        public string BoneName;
        public int ID = -1;
        public string ParentBoneName;
        public int Depth = -1;
        public bool BakeBone = true;
        public bool IsIKElement = false;
        public HumanBodyBones HumanoidBodyBone;

        [SerializeField, HideInInspector]
        private bool wasInitializing = false;

        /// <summary> Change value to 0 to not compress bone animation </summary>
        [NonSerialized] public float CompressionFactor = 1f;

        [NonSerialized] public Transform TempTransform;

        #region Bake Utils

        public string BoneBakePathName;
        public string HumToGenericBoneBakePathName;
        public bool HumanoidBoneDefined = false;
        public bool BakePosition = false;

        /// <summary> Not Supported Yet</summary>
        [Tooltip("Not Supported Yet")]
        public bool BakeScale = false;

        internal string tlx, tly, tlz;
        internal string rlx, rly, rlz, rlw;
        //Quaternion? latestSetRot = null;


        #endregion

        public ADBoneReference(Transform t, int count, Transform anim)
        {
            ID = count;
            Depth = -1;

            if (t == null) return;

            TempTransform = t;
            BoneName = t.name;

            if (t.parent) ParentBoneName = t.parent.name; else ParentBoneName = "";

            if (anim != null)
            {
                DefineDepth(anim.transform);
                Animator a = anim.GetAnimator();
                bool human = false; if (a) human = a.isHuman;

                if (human == false) DefineBakePathName(anim.transform);
                else
                {
                    DefineHumanoidBone(a);
                }
            }
        }

        internal void ApplyCoords(ADBoneKeyPose t)
        {
            if (TempTransform == null) return;
            TempTransform.localPosition = t.LocalPosition;
            TempTransform.localRotation = t.LocalRotation;
            TempTransform.localScale = t.LocalScale;
        }

        internal void ApplyCoords(Transform t)
        {
            if (TempTransform == null) return;
            TempTransform.localPosition = t.localPosition;
            TempTransform.localRotation = t.localRotation;
            TempTransform.localScale = t.localScale;
        }


        #region Bone Transform related

        internal void DefineBakePathName(Transform skelRootBone)
        {
            if (skelRootBone == null) return;
            if (TempTransform == null) return;
            BoneBakePathName = GetBonePathName(skelRootBone);
            HumToGenericBoneBakePathName = BoneBakePathName;
        }

        private string GetBonePathName(Transform skelRootBone)
        {
            return AnimationUtility.CalculateTransformPath(TempTransform, skelRootBone);
        }

        internal void DefineDepth(Transform skelRootBone)
        {
            Depth = GetDepth(TempTransform, skelRootBone);
        }

        public static Transform GetTransformUsingPath(AnimationDesignerSave save, string path)
        {
            Transform t = null;
            if ( save.LatestAnimator) t = save.LatestAnimator.transform.Find(path);
            if (t == null) if (save.SkelRootBone) t = save.SkelRootBone.Find(path);
            return t;
        }

        public static int GetDepth(Transform t, Transform skelRootBone, int notFoundReturn = 0)
        {
            int depth = 0;
            if (t == null) return notFoundReturn;

            int iters = 0;
            while (t != null && t.parent != skelRootBone)
            {
                t = t.parent;
                depth += 1;
                iters += 1;
            }

            if (iters == 0) return notFoundReturn;

            return depth;
        }

        public void DefineHumanoidBone(Animator anim)
        {
            HumanoidBoneDefined = false;
            if (anim == null) return;
            if (anim.isHuman == false) return;

            BoneBakePathName = "";

            foreach (HumanBodyBones bn in Enum.GetValues(typeof(HumanBodyBones)))
            {
                if ((int)bn < 0) continue;
                if ((int)bn >= (int)HumanBodyBones.LastBone) continue;

                Transform bone = anim.GetBoneTransform(bn);

                if (bone)
                    if (TempTransform == bone)
                    {
                        HumanoidBoneDefined = true;
                        HumanoidBodyBone = bn;
                        BoneBakePathName = bn.ToString();
                        break;
                    }
            }

            if (HumanoidBoneDefined == false)
            {
                if (!wasInitializing) BakeBone = false;
                DefineBakePathName(anim.transform);
            }

            wasInitializing = true;

        }

        public void GatherTempTransform(Transform parentOfSkel, Transform depthRef = null)
        {
            if (depthRef == null)
            {
                var transforms = parentOfSkel.GetComponentsInChildren<Transform>(true);

                foreach (var t in transforms)
                    if (t.name == BoneName)
                    //if (t.parent.name == ParentBoneName)
                    { TempTransform = t; return; }
            }
            else
            {
                foreach (var t in parentOfSkel.GetComponentsInChildren<Transform>(true))
                    if (t.name == BoneName)
                        if (t.parent.name == ParentBoneName)
                        {
                            int depth = 0;
                            Transform c = t;

                            while (c.parent != depthRef)
                            {
                                depth += 1;
                                c = c.parent;
                                if (c == null) break;
                            }

                            if (depth == Depth)
                            {
                                TempTransform = t;
                                return;
                            }

                        }
            }

            if (!string.IsNullOrEmpty(BoneName))
                UnityEngine.Debug.Log("[Animation Designer] Could not find bone '" + BoneName + "' in " + parentOfSkel);
        }

        #endregion


        #region Baking


        #region Bake Utils

        public void PrepareBakePropertyNames()
        {
            if (HumanoidBoneDefined && AnimationDesignerWindow._forceExportGeneric == false)
            {
                rlx = BoneBakePathName + "Q.x";
                rly = BoneBakePathName + "Q.y";
                rlz = BoneBakePathName + "Q.z";
                rlw = BoneBakePathName + "Q.w";

                tlx = BoneBakePathName + "T.x";
                tly = BoneBakePathName + "T.y";
                tlz = BoneBakePathName + "T.z";
            }
            else
            {
                rlx = "localRotation.x";
                rly = "localRotation.y";
                rlz = "localRotation.z";
                rlw = "localRotation.w";

                tlx = "localPosition.x";
                tly = "localPosition.y";
                tlz = "localPosition.z";
            }
        }



        [NonSerialized] public AnimationCurve _Bake_LocalPosX;
        [NonSerialized] public AnimationCurve _Bake_LocalPosY;
        [NonSerialized] public AnimationCurve _Bake_LocalPosZ;



        public float ComputePositionCurvesMagnitude()
        {
            return ComputePositionMagn(_Bake_LocalPosX) + ComputePositionMagn(_Bake_LocalPosY) + ComputePositionMagn(_Bake_LocalPosZ);
        }

        public static float ComputePositionMagn(AnimationCurve c)
        {
            if (c == null) return 0f;
            if (c.length < 2) return 0f;
            float sum = 0f;

            for (int i = 0; i < c.length - 1; i++) { sum += Mathf.Abs(c[i].value - c[i + 1].value); }
            return sum;
        }


        [NonSerialized] public AnimationCurve _Bake_LocalRotX;
        [NonSerialized] public AnimationCurve _Bake_LocalRotY;
        [NonSerialized] public AnimationCurve _Bake_LocalRotZ;
        [NonSerialized] public AnimationCurve _Bake_LocalRotW;
        public float ComputeRotationCurvesMagnitude()
        {
            return ComputeRotationMagn(_Bake_LocalRotX) + ComputePositionMagn(_Bake_LocalRotY) + ComputePositionMagn(_Bake_LocalRotW);
        }
        public static float ComputeRotationMagn(AnimationCurve c)
        {
            if (c == null) return 0f;
            if (c.length < 2) return 0f;
            float sum = 0f;

            for (int i = 0; i < c.length - 1; i++) { sum += Mathf.Abs(Quaternion.Angle(Quaternion.Euler(c[i].value, 0f, 0f), Quaternion.Euler(c[i + 1].value, 0f, 0f))); }
            return sum;
        }

        /// <summary> Used for baking local space coords </summary>
        public bool _Bake_PivotSpace = false;
        public bool _Bake_IsRoot = false;
        public Vector3 _Bake_PivotPosition = Vector3.zero;
        public Quaternion _Bake_PivotRotation = Quaternion.identity;

        public void ResetCurvesAndNamesForBaking(Transform anim)
        {
            PrepareBakePropertyNames();

            if (string.IsNullOrEmpty(HumToGenericBoneBakePathName))
            {
                if (anim != null) HumToGenericBoneBakePathName = GetBonePathName(anim.transform);
            }

            _Bake_LocalPosX = new AnimationCurve();
            _Bake_LocalPosY = new AnimationCurve();
            _Bake_LocalPosZ = new AnimationCurve();
            _Bake_LocalRotX = new AnimationCurve();
            _Bake_LocalRotY = new AnimationCurve();
            _Bake_LocalRotZ = new AnimationCurve();
            _Bake_LocalRotW = new AnimationCurve();

            _Bake_PivotSpace = false;

            if (HumanoidBoneDefined && AnimationDesignerWindow._forceExportGeneric == false)
            {
                BakeBone = false;

                if (HumanoidBodyBone == HumanBodyBones.LeftHand || HumanoidBodyBone == HumanBodyBones.RightHand ||
                    HumanoidBodyBone == HumanBodyBones.LeftFoot || HumanoidBodyBone == HumanBodyBones.RightFoot)
                {
                    BakeBone = true;
                    BakePosition = true;
                    IsIKElement = true;
                }
            }

            if (AnimationDesignerWindow._forceExportGeneric)
            {
                if (HumanoidBoneDefined) BakePosition = false;
                BakeBone = true;
            }

            UseAdditionalFramesLoop = true;
        }

        #endregion

        internal void BakeCurrentState(ADArmatureSetup armature, float keyTime)
        {
            Animator a = armature.LatestAnimator.GetAnimator();
            bool human = false; if (a) if (AnimationDesignerWindow._forceExportGeneric == false) human = a.isHuman;

            if (human)
            {
                //Transform root = armature.RootBoneReference.TempTransform;

                Vector3 bonePos = TempTransform.position;
                Quaternion boneRot = TempTransform.rotation;


                if (IsIKElement)
                {

                    #region IK Helper

                    #region Define IK Goal

                    AvatarIKGoal goal = AvatarIKGoal.LeftFoot;
                    if (HumanoidBodyBone == HumanBodyBones.RightFoot) goal = AvatarIKGoal.RightFoot;
                    else if (HumanoidBodyBone == HumanBodyBones.LeftHand) goal = AvatarIKGoal.LeftHand;
                    else if (HumanoidBodyBone == HumanBodyBones.RightHand) goal = AvatarIKGoal.RightHand;

                    #endregion

                    #region Get Unity Internal Avatar Helper Methods

                    MethodInfo methodGetAxisLength = typeof(Avatar).GetMethod("GetAxisLength", BindingFlags.Instance | BindingFlags.NonPublic);
                    if (methodGetAxisLength == null) throw new InvalidOperationException("Cannot find GetAxisLength method.");

                    MethodInfo methodGetPostRotation = typeof(Avatar).GetMethod("GetPostRotation", BindingFlags.Instance | BindingFlags.NonPublic);
                    if (methodGetPostRotation == null) throw new InvalidOperationException("Cannot find GetPostRotation method.");

                    #endregion

                    Quaternion postRotation = (Quaternion)methodGetPostRotation.Invoke(a.avatar, new object[] { (int)HumanoidBodyBone });

                    // Adjusting ik position / rotation
                    // with unity avatar computed offsets for hands / foots

                    Vector3 ikPos = bonePos;
                    Quaternion ikRot = boneRot * postRotation;

                    if (goal == AvatarIKGoal.LeftFoot || goal == AvatarIKGoal.RightFoot)
                    {
                        float axislength = (float)methodGetAxisLength.Invoke(a.avatar, new object[] { (int)HumanoidBodyBone });
                        Vector3 footBottom = new Vector3(axislength, 0, 0);
                        ikPos += (ikRot * footBottom);
                    }

                    Quaternion bodySpaceRot = Quaternion.Inverse(armature.bake.bodyRotation);
                    ikPos = bodySpaceRot * (ikPos - armature.bake.bodyPosition * a.humanScale);
                    ikRot = bodySpaceRot * ikRot;
                    ikPos = ikPos / a.humanScale;

                    ikRot = Quaternion.LookRotation(ikRot * Vector3.forward, ikRot * Vector3.up);

                    bonePos = ikPos;
                    boneRot = ikRot;

                    #endregion

                }
                else
                {
                    if (HumanoidBoneDefined == false)
                        if (_Bake_PivotSpace == false)
                        {
                            bonePos = TempTransform.localPosition;
                            boneRot = TempTransform.localRotation;
                        }
                }

                BakeCurrentState(keyTime, bonePos, boneRot);

            }
            else
            {
                if (TempTransform)
                {
                    BakeCurrentState(keyTime, TempTransform.localPosition, TempTransform.localRotation);
                }
            }
        }

        Quaternion? latestRot = null;
        internal void BakeCurrentState(float keyTime, Vector3 pos, Quaternion rot)
        {
            if (BakePosition)
            {
                if (_Bake_PivotSpace)
                {
                    pos = TempTransform.position - _Bake_PivotPosition;
                }

                _Bake_LocalPosX.AddKey(keyTime, pos.x);
                _Bake_LocalPosY.AddKey(keyTime, pos.y);
                _Bake_LocalPosZ.AddKey(keyTime, pos.z);
            }

            if (_Bake_PivotSpace)
            {
                rot = FEngineering.QToLocal(_Bake_PivotRotation, TempTransform.rotation);
            }

            // Very important for generic Rigs IK !!! Legacy don't need it for some unknown reason
            if (latestRot != null) rot = AnimationGenerateUtils.EnsureQuaternionContinuity(latestRot.Value, rot);
            latestRot = rot;

            _Bake_LocalRotX.AddKey(keyTime, rot.x);
            _Bake_LocalRotY.AddKey(keyTime, rot.y);
            _Bake_LocalRotZ.AddKey(keyTime, rot.z);
            _Bake_LocalRotW.AddKey(keyTime, rot.w);

        }



        #region Loop Wrap bake Keys Algorithms


        public bool DontDoInitialFramesWrap = false;
        public static bool LoopBakedPose = false;
        //public static EWrapBakeAlgrithmType FramesWrapType = EWrapBakeAlgrithmType.V1_MildFade;
        public static bool DoingIdleWrap { get 
            {
                if (AnimationDesignerWindow.Get == null) return false;
                var main = AnimationDesignerWindow.Get.Anim_MainSet;
                if (main == null) return false;
                return main.Export_WrapLoopBakeMode == EWrapBakeAlgrithmType.V3_Snap; 
            } }
        //public static bool DoingIdleWrap { get { return FramesWrapType == EWrapBakeAlgrithmType.V3_Snap; } }
        public static int LoopWrapAdditionalFrames = 0;

        public enum EWrapBakeAlgrithmType
        {
            V1_MildFade,
            V2_FadeGradually,
            V3_Snap,
            None
        }

        public static void WrapBake(EWrapBakeAlgrithmType wrapLoopMode, AnimationCurve curve, bool useAdditionalFrames = true)
        {
            switch (wrapLoopMode)
            {
                case EWrapBakeAlgrithmType.V1_MildFade:
                    WrapBakeV1Mild(curve, useAdditionalFrames);
                    break;
                case EWrapBakeAlgrithmType.V2_FadeGradually:
                    WrapBakeV2Gradual(curve, useAdditionalFrames);
                    break;
                case EWrapBakeAlgrithmType.V3_Snap:
                    WrapBakeV3IdleWrap(curve, useAdditionalFrames);
                    break;
            }
        }


        public static void WrapBakeV1Mild(AnimationCurve curve, bool useAdditionalFrames = true)
        {
            if (curve.keys.Length <= 1) return;

            WrapKeys(curve, 0, curve.keys.Length - 1, 0.75f, true, false);

            if (useAdditionalFrames == false) return;
            if (LoopWrapAdditionalFrames == 0) return;
            if (curve.keys.Length < LoopWrapAdditionalFrames + 3) return;
            float allFr = LoopWrapAdditionalFrames;

            WrapKeys(curve, 0, curve.keys.Length - 1, 0.75f, true, false);

            //for (int i = 1; i <= LoopWrapAdditionalFrames; i++)
            for (int i = LoopWrapAdditionalFrames; i >= 1; i--)
            {
                float ifl = (float)i;
                int lastKey = curve.keys.Length - i;
                //WrapKeys(curve, lastKey - 1, lastKey, 1f, false);
                //WrapKeys(curve, lastKey - 1, lastKey, 0.6f - (ifl / allFr) * 0.3f, false);
                WrapKeys(curve, lastKey - 1, 0, 0.4f - ((ifl - 1f) / allFr) * 0.35f, false);
            }
        }

        public static void WrapBakeV2Gradual(AnimationCurve curve, bool useAdditionalFrames = true)
        {
            if (curve.keys.Length <= 1) return;

            WrapKeys(curve, 0, curve.keys.Length - 1, 0.7f, true, false);

            if (useAdditionalFrames == false) return;
            if (LoopWrapAdditionalFrames == 0) return;
            if (curve.keys.Length < LoopWrapAdditionalFrames + 3) return;

            float loopAdditionalFrames = LoopWrapAdditionalFrames;

            WrapKeys(curve, 0, curve.keys.Length - 1, 0.5f, true, false);
            WrapKeys(curve, 0, curve.keys.Length - 1, 0.5f, true, true);

            for (int i = 1; i <= LoopWrapAdditionalFrames; i++)
            {
                float i_f = (float)i;
                float progrFromMidToClipEnd = 1f - ((i_f - 1f) / (loopAdditionalFrames));

                int lastKey = curve.keys.Length - i;
                WrapKeys(curve, lastKey - 1, 0, 0.05f + progrFromMidToClipEnd * 0.35f, false);
                WrapKeys(curve, lastKey - 1, lastKey, 0.3f * Mathf.Lerp(0.75f, 1.2f, progrFromMidToClipEnd), false);
            }
        }

        public static void WrapBakeV3IdleWrap(AnimationCurve curve, bool useAdditionalFrames = true)
        {
            if (curve.keys.Length <= 1) return;

            WrapKeys(curve, 0, curve.keys.Length - 1, 0.4f, true, false);

            if (useAdditionalFrames == false) return;
            if (LoopWrapAdditionalFrames == 0) return;
            if (curve.keys.Length < LoopWrapAdditionalFrames + 3) return;

            float loopAdditionalFrames = LoopWrapAdditionalFrames;
            WrapKeys(curve, 0, curve.keys.Length - 1, 0.5f, true, false);

            for (int i = LoopWrapAdditionalFrames; i >= 1; i -= 1)
            {
                float i_f = (float)i;
                float progrFromMidToClipEnd = 1f - ((i_f - 1f) / (loopAdditionalFrames));

                int lastKey = curve.keys.Length - i;

                WrapKeys(curve, lastKey - 1, lastKey, 0.1f + progrFromMidToClipEnd * 0.55f, false);
                //WrapKeys(curve, lastKey - 1, i == 1 ? 0 : lastKey + 1, 0.1f + progrFromMidToClipEnd * 0.65f, false);
                //WrapKeys(curve, lastKey - 1, lastKey, 0.5f - ((ifl - 1f) / allFr) * 0.25f, false);
            }

            WrapKeys(curve, 0, curve.keys.Length - 1, 0.5f, true, true);
        }

        static void WrapKeys(AnimationCurve curve, int keyA, int keyB, float toB = 0.7f, bool changeB = true, bool changeA = true)
        {
            float startTime = curve.keys[keyA].time;
            float endTime = curve.keys[keyB].time;

            float startVal = curve.keys[keyA].value;
            float endVal = curve.keys[keyB].value;

            float averageVal = Mathf.LerpUnclamped(startVal, endVal, toB);

            if (changeA) curve.MoveKey(keyA, new Keyframe(startTime, averageVal));
            if (changeB) curve.MoveKey(keyB, new Keyframe(endTime, averageVal));
        }


        #endregion



        /// <summary> Just for generic rigs root motion </summary>
        internal void ResetRotationCurves()
        {
            Quaternion start = Quaternion.identity;
            if (_Bake_LocalRotX.length > 0) start.x = _Bake_LocalRotX[0].value;
            if (_Bake_LocalRotY.length > 0) start.y = _Bake_LocalRotY[0].value;
            if (_Bake_LocalRotZ.length > 0) start.z = _Bake_LocalRotZ[0].value;
            if (_Bake_LocalRotW.length > 0) start.w = _Bake_LocalRotW[0].value;

            _Bake_LocalRotX = new AnimationCurve(new Keyframe(0f, start.x));
            _Bake_LocalRotY = new AnimationCurve(new Keyframe(0f, start.y));
            _Bake_LocalRotZ = new AnimationCurve(new Keyframe(0f, start.z));
            _Bake_LocalRotW = new AnimationCurve(new Keyframe(0f, start.w));
        }

        /// <summary> Just for generic rigs root motion </summary>
        internal void ResetPositionCurves()
        {
            Vector3 start = Vector3.zero;
            if (_Bake_LocalPosX.length > 0) start.x = _Bake_LocalPosX[0].value;
            if (_Bake_LocalPosY.length > 0) start.y = _Bake_LocalPosY[0].value;
            if (_Bake_LocalPosZ.length > 0) start.z = _Bake_LocalPosZ[0].value;

            _Bake_LocalPosX = new AnimationCurve(new Keyframe(0f, start.x));
            _Bake_LocalPosY = new AnimationCurve(new Keyframe(0f, start.y));
            _Bake_LocalPosZ = new AnimationCurve(new Keyframe(0f, start.z));
        }

        internal void WrapBakeKeys(EWrapBakeAlgrithmType wrapLoopMode, bool useAdditionalFrames)
        {
            if (DontDoInitialFramesWrap) return;
            if (wrapLoopMode == EWrapBakeAlgrithmType.None) return;

            if (BakePosition)
            {
                WrapBake(wrapLoopMode, _Bake_LocalPosX, useAdditionalFrames);
                WrapBake(wrapLoopMode, _Bake_LocalPosY, useAdditionalFrames);
                WrapBake(wrapLoopMode, _Bake_LocalPosZ, useAdditionalFrames);
            }

            WrapBake(wrapLoopMode, _Bake_LocalRotX, useAdditionalFrames);
            WrapBake(wrapLoopMode, _Bake_LocalRotY, useAdditionalFrames);
            WrapBake(wrapLoopMode, _Bake_LocalRotZ, useAdditionalFrames);
            WrapBake(wrapLoopMode, _Bake_LocalRotW, useAdditionalFrames);

            #region (Commented) Ensuring - fixing rotation after adjustements for loop
            //if (LoopWrapAdditionalFrames > 0)
            //{
            //    if (_Bake_LocalRotX.keys.Length > 12)
            //        for (int i = LoopWrapAdditionalFrames; i < _Bake_LocalRotX.keys.Length; i++)
            //        {
            //            int lastKey = _Bake_LocalRotX.keys.Length - 1;
            //            Quaternion q = GetRotationInKey(lastKey);
            //            q = AnimationGenerateUtils.EnsureQuaternionContinuity(GetRotationInKey(lastKey - 1), q);
            //            SetRotationInKey(lastKey, q);
            //        }
            //}
            #endregion

        }

        public Quaternion GetRotationInKey(int key)
        {
            return new Quaternion(
                _Bake_LocalRotX[key].value,
                _Bake_LocalRotY[key].value,
                _Bake_LocalRotZ[key].value,
                _Bake_LocalRotW[key].value
                );
        }

        public void SetRotationInKey(int key, Quaternion q)
        {
            _Bake_LocalRotX.MoveKey(key, new Keyframe(_Bake_LocalRotX[key].time, q.x));
            _Bake_LocalRotY.MoveKey(key, new Keyframe(_Bake_LocalRotY[key].time, q.y));
            _Bake_LocalRotZ.MoveKey(key, new Keyframe(_Bake_LocalRotZ[key].time, q.z));
            _Bake_LocalRotW.MoveKey(key, new Keyframe(_Bake_LocalRotW[key].time, q.w));
        }



        [NonSerialized] public bool UseAdditionalFramesLoop = true;
        public enum SaveCurvesMode
        {
            All, JustPosition, JustRotation
        }

        internal void SaveCurvesForClip(EWrapBakeAlgrithmType wrapLoopMode, ref AnimationClip clip, float reduction, bool animType = false, SaveCurvesMode mode = SaveCurvesMode.All)
        {
            string relPath = string.Empty;

            if (!HumanoidBoneDefined || AnimationDesignerWindow._forceExportGeneric)
            {
                relPath = HumToGenericBoneBakePathName;
            }

            if (!AnimationDesignerWindow._forceExportGeneric)
                if (IsIKElement) animType = true;

            Type curveType = typeof(Transform);
            if (animType) curveType = typeof(Animator);

            if (animType == false && string.IsNullOrEmpty(relPath)) return;


            if (LoopBakedPose)
            {
                WrapBakeKeys(wrapLoopMode, UseAdditionalFramesLoop);
            }


            #region Keyframes Reduction


            float compf = CompressionFactor;
            if (compf == 0f) compf = 1f;

            if (compf == ADArmatureSetup._LowestCompr) // Preventing unity mistakens and providing zero compression for bones which shouldn have zero compression
            {
                reduction = 0f;
            }
            else
                reduction *= compf;

            if (reduction > 0f)
            {
                if (BakePosition)
                {
                    _Bake_LocalPosX = AnimationGenerateUtils.ReduceKeyframes(_Bake_LocalPosX, reduction);
                    _Bake_LocalPosY = AnimationGenerateUtils.ReduceKeyframes(_Bake_LocalPosY, reduction);
                    _Bake_LocalPosZ = AnimationGenerateUtils.ReduceKeyframes(_Bake_LocalPosZ, reduction);
                }

                _Bake_LocalRotX = AnimationGenerateUtils.ReduceKeyframes(_Bake_LocalRotX, reduction);
                _Bake_LocalRotY = AnimationGenerateUtils.ReduceKeyframes(_Bake_LocalRotY, reduction);
                _Bake_LocalRotZ = AnimationGenerateUtils.ReduceKeyframes(_Bake_LocalRotZ, reduction);
                _Bake_LocalRotW = AnimationGenerateUtils.ReduceKeyframes(_Bake_LocalRotW, reduction);
            }

            #endregion


            if (mode == SaveCurvesMode.All || mode == SaveCurvesMode.JustPosition)
                if (BakePosition || mode == SaveCurvesMode.JustPosition)
                {
                    clip.SetCurve(relPath, curveType, tlx, _Bake_LocalPosX);
                    clip.SetCurve(relPath, curveType, tly, _Bake_LocalPosY);
                    clip.SetCurve(relPath, curveType, tlz, _Bake_LocalPosZ);
                }


            if (mode == SaveCurvesMode.All || mode == SaveCurvesMode.JustRotation)
            {
                clip.SetCurve(relPath, curveType, rlx, _Bake_LocalRotX);
                clip.SetCurve(relPath, curveType, rly, _Bake_LocalRotY);
                clip.SetCurve(relPath, curveType, rlz, _Bake_LocalRotZ);
                clip.SetCurve(relPath, curveType, rlw, _Bake_LocalRotW);
            }

        }



        #endregion

    }
}