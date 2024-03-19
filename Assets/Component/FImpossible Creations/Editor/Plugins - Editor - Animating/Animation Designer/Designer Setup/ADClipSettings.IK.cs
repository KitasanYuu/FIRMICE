using FIMSpace.FEditor;
using FIMSpace.FTools;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace FIMSpace.AnimationTools
{

    [System.Serializable]
    public class ADClipSettings_IK : IADSettings
    {

        public AnimationClip settingsForClip;
        public List<IKSet> LimbIKSetups = new List<IKSet>();

        public ADClipSettings_IK() { }

        /// <summary> Used for identifying different variants when modifying the same AnimationClip multiple times </summary>
        [SerializeField, HideInInspector] private string setId = "";
        [SerializeField, HideInInspector] private int setIdHash = 0;
        public void AssignID(string name) { setId = name; setIdHash = setId.GetHashCode(); }
        public string SetID { get { return setId; } }
        public int SetIDHash { get { return setIdHash; } }
        public AnimationClip SettingsForClip { get { return settingsForClip; } }
        public void OnConstructed(AnimationClip clip, int hash) { settingsForClip = clip; setIdHash = hash; }


        public ADClipSettings_IK Copy(ADClipSettings_IK to, bool noCopy)
        {
            ADClipSettings_IK cpy = to;
            if (noCopy == false) cpy = (ADClipSettings_IK)MemberwiseClone();

            cpy.LimbIKSetups = new List<IKSet>();

            for (int i = 0; i < LimbIKSetups.Count; i++)
            {
                IKSet nIk = LimbIKSetups[i].Copy();
                cpy.LimbIKSetups.Add(nIk);
            }

            cpy.setId = to.setId;
            cpy.setIdHash = to.setIdHash;
            PasteValuesTo(this, cpy);

            return cpy;
        }



        #region Settings container Methods

        internal static void PasteValuesTo(ADClipSettings_IK from, ADClipSettings_IK to)
        {
            for (int i = 0; i < from.LimbIKSetups.Count; i++)
                if (from.LimbIKSetups[i].Index == to.LimbIKSetups[i].Index)
                    IKSet.PasteValuesTo(from.LimbIKSetups[i], to.LimbIKSetups[i]);
        }


        public IKSet GetIKSettingsForLimb(ADArmatureLimb selectedLimb, AnimationDesignerSave setup)
        {
            for (int i = 0; i < LimbIKSetups.Count; i++)
            {
                var ls = LimbIKSetups[i];

                if (ls.Index == selectedLimb.GetIndex)
                {
                    return ls;
                }
            }

            IKSet set = new IKSet(false, selectedLimb.GetName, selectedLimb.GetIndex);
            set.PrepareFor(selectedLimb);
            LimbIKSetups.Add(set);

            if (setup != null) setup._SetDirty();

            return set;
        }

        internal void ResetState()
        {
            for (int i = 0; i < LimbIKSetups.Count; i++)
            {
                var lSet = LimbIKSetups[i];
                if (lSet.IKType == IKSet.EIKType.FootIK)
                {
                    if (lSet.IsLegGroundingMode)
                    {
                        lSet.LegGrounding = IKSet.ELegGroundingView.Processing;
                    }
                }
            }
        }


        #endregion



        #region IK Set


        [System.Serializable]
        public class IKSet : INameAndIndex
        {
            [NonSerialized] ADClipSettings_Main refToMainSet = null;

            /// <summary> Alternative IK execution index for this animation clip setup </summary>
            [HideInInspector] public int AlternateExecutionIndex = -2;
            [HideInInspector] public bool UseAlternateExecutionIndex = false;

            public object LastUsedProcessor = null;

            public int Index;
            public string ID;

            public bool Enabled = false;
            public float Blend = 1f;
            public AnimationCurve BlendEvaluation = AnimationCurve.EaseInOut(0f, 1f, 1f, 1f);

            public enum EIKType { ArmIK, FootIK, ChainIK }
            public EIKType IKType = EIKType.ChainIK;

            public enum EOrder { OverrideElasticity, InheritElasticity }
            public EOrder UpdateOrder = EOrder.OverrideElasticity;

            public enum EIKAutoHintMode { Default, FollowHipsRotation }
            public EIKAutoHintMode AutoHintMode = EIKAutoHintMode.Default;

            [NonSerialized] public EWasAdjusted LatelyAdjusted = EWasAdjusted.Offset;
            public enum EWasAdjusted { Offset, Still }

            public IKSet(bool enabled = false, string id = "", int index = -1, float blend = 1f)
            {
                Enabled = enabled;
                Index = index;
                ID = id;
                Blend = blend;
            }


            public IKSet Copy()
            {
                IKSet cpy = (IKSet)MemberwiseClone();
                return cpy;
            }


            #region Basic IK Params


            public float IKPosOffMul = 1f;
            public AnimationCurve IKPosOffEvaluate = AnimationCurve.EaseInOut(0f, 1f, 1f, 1f);
            public Vector3 IKPositionOffset = Vector3.zero;

            public float IKPosStillMul = 0f;
            public AnimationCurve IKPosStillEvaluate = AnimationCurve.EaseInOut(0f, 1f, 1f, 1f);
            public Vector3 IKStillPosition = Vector3.zero;
            public float IKPosYOriginal = 0f;
            public bool IKStillWorldPos = false;
            public bool IKStillWorldRot = false;


            public Vector3 IKHintOffset = Vector3.zero;
            public Vector3 ThighOffset = Vector3.zero;


            public float IKRotationBlend = 1f;
            public float ShoulderBlend = 0.3f;


            public float IKRotOffMul = 1f;
            public AnimationCurve IKRotOffEvaluate = AnimationCurve.EaseInOut(0f, 1f, 1f, 1f);
            public Vector3 IKRotationOffset = Vector3.zero;

            public float IKRotStillMul = 0f;
            public AnimationCurve IKRotStillEvaluate = AnimationCurve.EaseInOut(0f, 1f, 1f, 1f);
            public Vector3 IKStillRotation = Vector3.zero;

            public float ChainSmoothing = 0f;


            #endregion



            #region Multi still ik points Params

            public bool UseMultiStillPoints = false;
            public List<StillPoint> MultiStillPoints = new List<StillPoint>();
            public static StillPoint _SelectedStillPoint = null;
            public static StillPoint _DraggingStillPoint = null;
            public static bool _DraggingTime = false;
            public static bool _MultiStillDrawExtra = false;

            /// <summary> Sort multi still points by time </summary>
            public void RefreshMultiPointsTiming()
            {
                MultiStillPoints.Sort((a, b) => a.time.CompareTo(b.time));
            }

            [System.Serializable]
            public class StillPoint
            {
                public float time = 0f;
                public Vector3 pos = Vector3.zero;
                public Vector3 rot = Vector3.zero;
                public float blendInDuration = 0.05f;
                public float blendOutDuration = 0.05f;
                public float keepFor = 0.1f;
                public float pointBlend = 1f;
                public float positionBlend = 1f;
                public float rotationBlend = 1f;

                public enum ETransitionSettings
                {
                    None, PositionOffset, RotationOffset, PelvisOffset
                }

                public ETransitionSettings TransitionSettingsDraw = ETransitionSettings.None;

                public Vector3 FootTransitionOffset = Vector3.up;
                public AnimationCurve FootTransitionInOffsetCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 0f);
                public AnimationCurve FootTransitionOutOffsetCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 0f);

                public AnimationCurve FootTransitionInProgressCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
                public AnimationCurve FootTransitionOutProgressCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);


                public Vector3 FootTransitionRotationOffset = Vector3.zero;
                public AnimationCurve FootRotTransitionInOffsetCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 0f);
                public AnimationCurve FootRotTransitionOutOffsetCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 0f);

                public Vector3 HipsTransitionPositionOffset = Vector3.zero;
                public AnimationCurve HipsTransitionInOffsetCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 0f);
                public AnimationCurve HipsTransitionOutOffsetCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 0f);
                public Vector3 HipsTransitionOutPositionOffset = Vector3.zero;
                public enum EOffsetMode { InOutTheSame, InOffset, OutOffset }
                public EOffsetMode HipsOffsetMode = EOffsetMode.InOutTheSame;

                public StillPoint(float animProgr, Vector3 vector3, Quaternion rot1)
                {
                    time = animProgr;
                    pos = vector3;
                    rot = rot1.eulerAngles;
                }

                internal void RefreshCurves()
                {
                    if (FootTransitionInOffsetCurve == null || FootTransitionInOffsetCurve.length == 0)
                    { FootTransitionInOffsetCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 0f); }

                    if (FootTransitionOutOffsetCurve == null || FootTransitionOutOffsetCurve.length == 0)
                    { FootTransitionOutOffsetCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 0f); }

                    if (FootTransitionInProgressCurve == null || FootTransitionInProgressCurve.length == 0)
                    { FootTransitionInProgressCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f); }

                    if (FootTransitionOutProgressCurve == null || FootTransitionOutProgressCurve.length == 0)
                    { FootTransitionOutProgressCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f); }


                    if (FootRotTransitionInOffsetCurve == null || FootRotTransitionInOffsetCurve.length == 0)
                    { FootRotTransitionInOffsetCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 0f); }

                    if (FootRotTransitionOutOffsetCurve == null || FootRotTransitionOutOffsetCurve.length == 0)
                    { FootRotTransitionOutOffsetCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 0f); }


                    if (HipsTransitionInOffsetCurve == null || HipsTransitionInOffsetCurve.length == 0)
                    { HipsTransitionInOffsetCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 0f); }

                    if (HipsTransitionOutOffsetCurve == null || HipsTransitionOutOffsetCurve.length == 0)
                    { HipsTransitionOutOffsetCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 0f); }
                }
            }

            #endregion



            #region Leg Grounding Params

            public enum ELegMode { Grounding, BasicIK }
            public ELegMode LegMode = ELegMode.BasicIK;

            public enum ELegGroundingView { Analyze, Processing }
            public ELegGroundingView LegGrounding = ELegGroundingView.Analyze;

            public float HeelGroundedTreshold = 1f;
            public float FloorGroundedOffset = 0f;
            public LegAnimationClipMotion.EAnalyzeMode FloorGroundingAnalyzeMode = LegAnimationClipMotion.EAnalyzeMode.HeelFeetTight;

            public float ToesGroundedTreshold = 1f;
            public float HoldOnGround = 0f;

            public bool ExportGroundingCurve = false;

            public float FootGroundBlendDuration = 0.175f;
            public AnimationCurve MaxLegStretchEvaluation = AnimationCurve.EaseInOut(0f, 1f, 1f, 1f);
            public float MaxLegStretch = 1.1f;
            public float GroundLater = 0f;
            public float UnGroundLater = 0f;

            public float FootSwingsMultiply = 1f;
            public float FootRaiseMultiply = 0f;
            public float FootMildMotionX = 0f;
            public float FootMildMotionZ = 0f;
            public float FootLinearize = 0f;
            public float FootLinearizeSpeedMul = 1f;
            public Vector3 FootLinearStartAdjust = Vector3.zero;
            public Vector3 FootLinearEndAdjust = Vector3.zero;
            public float FootSnapToLatestGrounded = 0f;
            public AnimationCurve FootSnapToLatestGroundedEvaluation = AnimationCurve.EaseInOut(0f, 1f, 1f, 1f);
            public float FootSnapToGrFrameOffset = 0.5f;
            public AnimationCurve FootSnapToGrFrameOffsetEvaluation = AnimationCurve.EaseInOut(0f, 1f, 1f, 1f);

            public bool UseGroundedFeet = false;
            private List<ADBoneKeyPose> feetBones = null;


            //public Vector3 GroundedFeetRotation = Vector3.zero;

            bool displayProcessingGroundingCurve = false;

            #endregion



            #region Extra Leg Grounding Params

            public float HoldXPosWhenGrounded = 0f;
            public float HoldZPosWhenGrounded = 0f;

            public float IKWhenGroundedPosOffMul = 1f;
            public AnimationCurve IKWhenGroundedPosOffEvaluate = AnimationCurve.EaseInOut(0f, 1f, 1f, 1f);
            public Vector3 IKWhenGroundedPositionOffset = Vector3.zero;

            public float IKWhenUngroundedPosOffMul = 1f;
            public AnimationCurve IKWhenUngroundedPosOffEvaluate = AnimationCurve.EaseInOut(0f, 1f, 1f, 1f);
            public Vector3 IKWhenUngroundedPositionOffset = Vector3.zero;

            public float IKWhenGroundedRotOffMul = 1f;
            public AnimationCurve IKWhenGroundedRotOffEvaluate = AnimationCurve.EaseInOut(0f, 1f, 1f, 1f);
            public Vector3 IKWhenGroundedRotationOffset = Vector3.zero;

            public float IKWhenUngroundedRotOffMul = 1f;
            public AnimationCurve IKWhenUngroundedRotOffEvaluate = AnimationCurve.EaseInOut(0f, 1f, 1f, 1f);
            public Vector3 IKWhenUngroundedRotationOffset = Vector3.zero;

            #endregion



            #region Extra Params - Curves Gen - align to

            public bool UseFootRotationMapping = true;
            public string LatestLimbName = "";
            public bool GenerateFootGroundCurve = false;
            public string GenerateFootStepEvents = "";
            public float GeneratedStepEventsTimeOffset = 0.0f;

            public ADTransformMemory AlignTo = new ADTransformMemory();
            //public string alignToBoneName = "";
            //public Transform alignTo = null;
            public float AlignToBlend = 1f;
            public float AlignRotationToBlend = 0f;

            #endregion



            #region Utility Helper Properties


            public static IKSet CopyingFrom = null;

            public string GetName { get { return ID; } }
            public int GetIndex { get { return Index; } }
            public float GUIAlpha { get { if (Enabled == false) return 0.1f; if (Blend < 0.5f) return 0.2f + Blend; return 1f; } }



            public bool IsLegGroundingMode
            {
                get
                {
                    if (IKType == ADClipSettings_IK.IKSet.EIKType.FootIK)
                        if (LegMode == ADClipSettings_IK.IKSet.ELegMode.Grounding)
                            return true;

                    return false;
                }
            }


            internal void DrawHellFootDetectionMask(ADClipSettings_Main mainSet, ADArmatureLimb limb, float heelGroundedTreshold, float toesGroundedTreshold, float a1 = 1f, float a2 = 1f)
            {
                Color preC = Handles.color;
                Transform animT = mainSet.LatestAnimator.transform;
                Transform footT = limb.LastBone.T;


                Vector3 lowestYPos = animT.position + animT.up * FootDataAnalyze.LowestFootCoords.y;

                // Lowest guide
                Handles.color = new Color(preC.r, preC.g, preC.b, a1);

                Vector3 heelP = GetAnalysisHeelPoint(footT);
                Vector3 toesP = GetAnalysisToesPoint(footT, heelP);
                //Vector3 hell_toesPoint = Vector3.Lerp(heelP, toesP, 0.6f);

                // Toes and heel to lowest
                Handles.color = new Color(preC.r, preC.g, preC.b, a2 * 0.3f);
                Vector3 dPoint = heelP; dPoint.y = lowestYPos.y;
                //Handles.DrawDottedLine(dPoint, heelP, 2f);
                dPoint = toesP; dPoint.y = lowestYPos.y;

                Vector3 heelPT = GetAnalysisTreshHeelPoint(footT, heelGroundedTreshold);
                Vector3 toesPT = GetAnalysisTreshToesPoint(footT, toesGroundedTreshold);

                Handles.DrawDottedLine(heelPT, ChangeY(heelPT, lowestYPos.y), 2f);
                Handles.DrawDottedLine(toesPT, ChangeY(toesPT, lowestYPos.y), 2f);


                Handles.color = new Color(preC.r - 0.3f, preC.g, preC.b - 0.3f, a2 * 0.75f);

                // Guides for heel
                Handles.DrawLine(heelPT, footT.position);
                Handles.DrawLine(heelPT, heelP);
                // Guides for toes
                Handles.color = new Color(preC.r - 0.3f, preC.g, preC.b - 0.3f, a2 * 0.75f);
                Handles.DrawLine(toesPT, footT.position);
                Handles.DrawLine(toesPT, toesP);

                Handles.color = new Color(preC.r, preC.g, preC.b, a2 * 0.3f);

                // Toes and heel tresh points
                float refLen = FootDataAnalyze.GetRefScale(limb.Bones[1].T, footT);
                float treshLen = FootDataAnalyze.GetHeightTresholdScale(limb.Bones[1].T, footT, refLen);

                Handles.color = new Color(Handles.color.r, Handles.color.g, preC.b * 0.2f, a1 * 0.3f);
                dPoint = heelPT;

                // Define Debug Y Pos for foot grounding level
                dPoint.y = animT.position.y + (treshLen * animT.lossyScale.y * 1f) * heelGroundedTreshold;
                Handles.SphereHandleCap(0, dPoint, Quaternion.identity, treshLen * 0.75f, EventType.Repaint);
                Handles.DrawLine(dPoint + Vector3.forward * refLen * 2f, dPoint - Vector3.forward * refLen * 2f);


                dPoint = toesPT;
                dPoint.y = animT.position.y + (treshLen * 1.0f) * toesGroundedTreshold;
                Handles.SphereHandleCap(0, dPoint, Quaternion.identity, treshLen * 1f, EventType.Repaint);
                Handles.DrawLine(dPoint + Vector3.forward * refLen * 2f, dPoint - Vector3.forward * refLen * 2f);

                Handles.color = preC;

                #region Backup

                //Vector3 heelP = GetAnalysisHeelPoint(footT);
                ////heelP = footT.position;

                //// Heel
                //Handles.color = new Color(Handles.color.r, Handles.color.g, Handles.color.b, a1);
                //float treshLen = FootDataAnalyze.GetTresholdLength(limb.Bones[1].T, footT, heelGroundedTreshold);
                //Vector3 toesP = GetAnalysisToesPoint(footT, heelP);
                ////toesP = limb.LastBone.TrPoint(FootDataAnalyze._footToToes);

                //float refLimbLen = limb.GetCurrentLimbLength() * 0.22f;
                //Vector3 off = mainSet.LatestAnimator.transform.right * refLimbLen;

                //_b_hf_detect[0] = (heelP) + off;
                //_b_hf_detect[1] = (toesP) + off;
                //_b_hf_detect[2] = off + (toesP - footT.TransformVector(FootDataAnalyze._footLocalToGround) * treshLen);

                //// Toes
                //treshLen = FootDataAnalyze.GetTresholdLength(limb.Bones[1].T, footT, toesGroundedTreshold);
                //_b_hf_detect[3] = off + heelP - footT.TransformVector(FootDataAnalyze._footLocalToGround) * treshLen;
                //_b_hf_detect[4] = heelP + off;

                //Handles.color = new Color(Handles.color.r, Handles.color.g, Handles.color.b, a2);

                //Vector3 hell_toesPoint = Vector3.Lerp(_b_hf_detect[0], _b_hf_detect[1], 0.4f);
                //Vector3 hell_toesPoint2 = Vector3.Lerp(_b_hf_detect[2], _b_hf_detect[3], 0.4f);
                //Handles.DrawLine(hell_toesPoint, hell_toesPoint2);

                //_b_hf_detect[1] = hell_toesPoint;
                //_b_hf_detect[2] = hell_toesPoint2;
                //Handles.DrawAAPolyLine(2f, _b_hf_detect);

                #endregion

            }


            public bool WasAnalyzed
            {
                get
                {
                    return (FootDataAnalyze != null && FootDataAnalyze.analyzed);
                }
            }


            float clipLength = 1f;
            float clipFramerate = 30f;
            float clipKeyStep = 0.04f;

            #endregion


            #region Utility / Refresh Methods

            Vector3 ChangeY(Vector3 pos, float y)
            {
                Vector3 p = pos;
                pos.y = y;
                return pos;
            }


            public static void PasteValuesTo(IKSet from, IKSet to)
            {
                to.BlendEvaluation = AnimationDesignerWindow.CopyCurve(from.BlendEvaluation);
                to.MaxLegStretchEvaluation = AnimationDesignerWindow.CopyCurve(from.MaxLegStretchEvaluation);
                to.Blend = from.Blend;
                to.Enabled = from.Enabled;

                to.IKRotationBlend = from.IKRotationBlend;

                //to.IKType = from.IKType;
                to.UpdateOrder = from.UpdateOrder;

                to.IKPosOffMul = from.IKPosOffMul;
                to.IKPosOffEvaluate = from.IKPosOffEvaluate;
                to.IKPositionOffset = from.IKPositionOffset;
                to.IKPosStillMul = from.IKPosStillMul;
                to.IKStillWorldPos = from.IKStillWorldPos;
                to.IKStillWorldRot = from.IKStillWorldRot;
                to.IKPosStillEvaluate = from.IKPosStillEvaluate;
                to.IKStillPosition = from.IKStillPosition;
                to.IKPosYOriginal = from.IKPosYOriginal;
                to.IKRotOffMul = from.IKRotOffMul;
                to.IKRotOffEvaluate = from.IKRotOffEvaluate;
                to.IKRotationOffset = from.IKRotationOffset;
                to.IKRotStillMul = from.IKRotStillMul;
                to.IKRotStillEvaluate = from.IKRotStillEvaluate;
                to.IKStillRotation = from.IKStillRotation;
                to.IKHintOffset = from.IKHintOffset;
                to.UseGroundedFeet = from.UseGroundedFeet;

                to.FloorGroundedOffset = from.FloorGroundedOffset;
                to.FloorGroundingAnalyzeMode = from.FloorGroundingAnalyzeMode;
                to.ExportGroundingCurve = from.ExportGroundingCurve;

                to.GenerateFootGroundCurve = from.GenerateFootGroundCurve;

                to.ChainSmoothing = from.ChainSmoothing;

                PasteGroundingValuesTo(from, to);

            }

            public static void PasteGroundingValuesTo(IKSet from, IKSet to)
            {
                to.IKRotationOffset = from.IKRotationOffset;

                to.LegMode = from.LegMode;
                to.LegGrounding = from.LegGrounding;
                to.HeelGroundedTreshold = from.HeelGroundedTreshold;
                to.ToesGroundedTreshold = from.ToesGroundedTreshold;
                to.HoldOnGround = from.HoldOnGround;
                to.FootGroundBlendDuration = from.FootGroundBlendDuration;
                to.MaxLegStretch = from.MaxLegStretch;
                to.GroundLater = from.GroundLater;
                to.UnGroundLater = from.UnGroundLater;
                to.LegGrounding = from.LegGrounding;

                to.FootSwingsMultiply = from.FootSwingsMultiply;
                to.FootRaiseMultiply = from.FootRaiseMultiply;
                to.FootMildMotionX = from.FootMildMotionX;
                to.FootMildMotionZ = from.FootMildMotionZ;
                to.FootLinearize = from.FootLinearize;
                to.FootLinearizeSpeedMul = from.FootLinearizeSpeedMul;
                to.FootLinearStartAdjust = from.FootLinearStartAdjust;
                to.FootLinearEndAdjust = from.FootLinearEndAdjust;
                to.HoldXPosWhenGrounded = from.HoldXPosWhenGrounded;
                to.HoldZPosWhenGrounded = from.HoldZPosWhenGrounded;
                to.UseGroundedFeet = from.UseGroundedFeet;
            }

            public void RefreshMod(AnimationDesignerSave save, ADClipSettings_Main main)
            {
                if (main == null) return;
                if (main.settingsForClip == null) return;

                clipLength = main.settingsForClip.length;
                clipFramerate = main.settingsForClip.frameRate;
                clipKeyStep = 1f / clipFramerate;

                if (AlignTo == null) AlignTo = new ADTransformMemory();
                AlignTo.InitializeReference(save);
                AlignTo.DisplayName = "Align To:";

                //if (alignToBoneName != "")
                //{
                //    if (alignTo == null || alignTo.name != alignToBoneName)
                //    {
                //        alignTo = save.GetBoneByName(alignToBoneName);
                //    }
                //}

                refToMainSet = main;
            }

            public float SecToProgr(float sec)
            {
                return ADClipSettings_Main.SecondsToProgress(sec, clipLength);
            }


            #endregion



            #region GUI Related


            bool ikSetupFoldout = false;
            bool ikCoordsFoldout = false;
            bool ikRotsFoldout = false;
            bool ikAnalyzeExtraFoldot = false;
            static bool mainSetFoldout = false;
            static bool mainSetFoldoutExtra = false;
            bool displayGrInfo = false;

            static bool ikMainGroundFoldout = false;

            //string selectorHelperId = "";

            bool grIkSwingsFoldout = true;
            bool grIkOffsFoldout = false;

            bool grIkOffsConstFoldout = false;
            bool grIkOffsWhenGrFoldout = false;
            bool grIkOffsWhenUngrFoldout = false;

            bool syncGrLegIKSettingsToOtherLegs = false;

            internal bool requestReinitialize = false;


            internal void DrawTopGUI(string title, float progr)
            {
                EditorGUILayout.BeginHorizontal();

                Enabled = EditorGUILayout.Toggle(Enabled, GUILayout.Width(24));

                if (!string.IsNullOrEmpty(title))
                {
                    EditorGUILayout.LabelField(title + " : Inverse Kinematics (IK)", FGUI_Resources.HeaderStyle);
                }

                #region Copy Paste Buttons

                if (CopyingFrom != null)
                {
                    if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("Clipboard").image, "Paste copied values"), FGUI_Resources.ButtonStyle, GUILayout.Width(22), GUILayout.Height(19)))
                    {
                        PasteValuesTo(CopyingFrom, this);
                    }
                }

                if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("TreeEditor.Duplicate").image, "Copy ik parameters values below to paste them into other limb"), FGUI_Resources.ButtonStyle, GUILayout.Width(22), GUILayout.Height(19)))
                {
                    CopyingFrom = this;
                }

                #endregion

                EditorGUILayout.EndHorizontal();

                GUILayout.Space(5);

                if (!Enabled) GUI.enabled = false;

                AnimationDesignerWindow.GUIDrawFloatPercentage(ref Blend, new GUIContent("IK Blend:"));
                AnimationDesignerWindow.DrawSliderProgress(Blend * BlendEvaluation.Evaluate(progr), 52);
                AnimationDesignerWindow.DrawCurve(ref BlendEvaluation, "Blend Along Clip Time");
                AnimationDesignerWindow.DrawCurveProgress(progr);


                GUI.enabled = true;
            }


            internal void DrawParamsGUI(float animProgr, ADArmatureLimb limb, ADClipSettings_Main clipMain, AnimationDesignerSave save, ADClipSettings_IK ikSets)
            {
                if (refToMainSet == null) refToMainSet = clipMain;

                if (!Enabled) GUI.enabled = false;
                Color preC = GUI.color;
                Color preBC = GUI.backgroundColor;

                FGUI_Inspector.DrawUILine(0.3f, 0.5f, 1, 15, 0.975f);

                #region Lately Adjusted Switches

                if (IKPositionOffset == Vector3.zero)
                {
                    if (IKPosStillMul > 0.01f)
                        if (IKStillPosition.sqrMagnitude > 0.0001f)
                            LatelyAdjusted = EWasAdjusted.Still;
                }

                if (IKPosOffMul < 0.01f)
                {
                    if (IKPosStillMul > 0.01f)
                    { LatelyAdjusted = EWasAdjusted.Still; }
                }


                if (UseMultiStillPoints) LatelyAdjusted = EWasAdjusted.Offset;

                #endregion


                #region IK Setup Mode


                Texture2D ikIcon = AnimationDesignerWindow.Tex_Leg;
                if (IKType == EIKType.ArmIK) ikIcon = AnimationDesignerWindow.Tex_Arm;
                else if (IKType == EIKType.ChainIK) ikIcon = AnimationDesignerWindow.Tex_Chain;


                GUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(new GUIContent("  " + FGUI_Resources.GetFoldSimbol(ikSetupFoldout, 10, "►") + "  IK Setup", ikIcon), FGUI_Resources.FoldStyle, GUILayout.Height(22))) ikSetupFoldout = !ikSetupFoldout;

                IKType = (EIKType)EditorGUILayout.EnumPopup(IKType);

                if (IKType == EIKType.FootIK)
                {
                    EditorGUIUtility.labelWidth = 80;
                    LegMode = (ELegMode)EditorGUILayout.EnumPopup("Foot IK Mode:", LegMode, GUILayout.Width(160));
                    EditorGUIUtility.labelWidth = 0;
                }

                EditorGUILayout.EndHorizontal();

                if (ikSetupFoldout)
                {

                    GUILayout.Space(6);
                    EditorGUILayout.BeginHorizontal();
                    EditorGUIUtility.labelWidth = 84;


                    //IKType = (EIKType)EditorGUILayout.EnumPopup(new GUIContent("   IK Algorithm:", "Algorithm used for computing IK.\nChain IK is dedicated for limbs with more than 3 bones."), IKType);
                    //GUILayout.Space(6);
                    //UpdateOrder = (EOrder)EditorGUILayout.EnumPopup(new GUIContent("Update Order:", "Overriding will make IK position more stiff and in-place, executing before elasticness will allow elasticness applying it's effect over IK position"), UpdateOrder);

                    EditorGUIUtility.labelWidth = 0;
                    EditorGUILayout.EndHorizontal();


                    #region Check Limb Count Correctness

                    if (IKType == EIKType.ArmIK)
                    {
                        if (limb.Bones.Count != 4)
                        {
                            GUILayout.Space(4);
                            EditorGUILayout.HelpBox("Arm IK requires limb with 4 bones to work!", MessageType.Warning);
                            GUILayout.Space(4);
                        }
                    }
                    else if (IKType == EIKType.FootIK)
                    {
                        if (limb.Bones.Count != 3)
                        {
                            GUILayout.Space(4);
                            EditorGUILayout.HelpBox("Foot IK requires limb with 3 bones to work!", MessageType.Warning);
                            GUILayout.Space(4);
                        }
                    }
                    else if (IKType == EIKType.ChainIK)
                    {
                        if (limb.Bones.Count <= 2)
                        {
                            GUILayout.Space(4);
                            EditorGUILayout.HelpBox("Chain IK requires limb with more than 2 bones to work!", MessageType.Warning);
                            GUILayout.Space(4);
                        }
                    }

                    #endregion


                    if (IKType == EIKType.ChainIK || (IKType == EIKType.FootIK && LegMode == ELegMode.Grounding))
                    {
                        GUILayout.Space(8);
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        EditorGUILayout.LabelField("Foot/Chain IK Requires correctly setted T-Pose!\nMake sure it's setted correctly under the Top Tab (click on the displayed object name)", EditorStyles.centeredGreyMiniLabel, GUILayout.Height(26));
                        EditorGUILayout.EndVertical();

                        if (IKType == EIKType.ChainIK)
                        {
                            GUILayout.Space(4);
                            AnimationDesignerWindow.GUIDrawFloatPercentage(ref ChainSmoothing, new GUIContent("Chain Smoothing: "));
                        }
                    }

                    GUILayout.Space(4);
                    if (requestReinitialize) GUI.color = new Color(0.6f, 0.6f, 0.6f, 1f);
                    if (GUILayout.Button(new GUIContent("Re-Initialize IK Processor", "Should be called after correcting T-Pose of the model or after changing Limb bones hierarchy"))) { requestReinitialize = true; }
                    GUI.color = preC;

                    if (IKType == EIKType.FootIK || IKType == EIKType.ArmIK)
                    {
                        // IK Auto-Hint Mode
                        GUILayout.Space(4);
                        EditorGUILayout.BeginHorizontal();
                        AutoHintMode = (EIKAutoHintMode)EditorGUILayout.EnumPopup(new GUIContent("Auto Hint Mode:", "Auto Hint mode for Knee/Elbow rotation"), AutoHintMode);
                        if (IKHintOffset == Vector3.zero) EditorGUILayout.HelpBox("Different effect when using Hint Offset value", MessageType.None);
                        EditorGUILayout.EndHorizontal();

                        if (IKType == EIKType.FootIK)
                            ThighOffset = EditorGUILayout.Vector3Field(new GUIContent("Thigh Offset:", "If thigh bone seems to be rotated in comparison to original animation, you should be able to fix it with this parameter"), ThighOffset);
                        else if (IKType == EIKType.ArmIK)
                            ThighOffset = EditorGUILayout.Vector3Field(new GUIContent("Upperarm Offset:", "If upperarm bone seems to be rotated in comparison to original animation, you should be able to fix it with this parameter"), ThighOffset);
                    }

                    GUILayout.Space(6);

                    if (UseAlternateExecutionIndex == false)
                    {
                        if (GUILayout.Button("Enable Alternate Execution Order", GUILayout.Width(280)))
                        {
                            UseAlternateExecutionIndex = true;
                            AlternateExecutionIndex = -1;
                            save._SetDirty();
                        }
                    }
                    else
                    {
                        EditorGUILayout.BeginHorizontal();

                        EditorGUI.BeginChangeCheck();
                        AlternateExecutionIndex = EditorGUILayout.IntSlider("Execution Order: ", AlternateExecutionIndex, -1, save.Limbs.Count);

                        if (GUILayout.Button("Restore Order"))
                        {
                            UseAlternateExecutionIndex = false;
                            AlternateExecutionIndex = -2;
                        }

                        if (EditorGUI.EndChangeCheck()) save._SetDirty();

                        EditorGUILayout.EndHorizontal();
                    }

                    GUILayout.Space(6);
                }

                EditorGUILayout.EndVertical();

                #endregion


                #region Leg Grounding GUI

                if (IKType == EIKType.FootIK && LegMode == ELegMode.Grounding)
                {
                    if (limb != null) LatestLimbName = limb.GetName;

                    GUILayout.Space(10);
                    EditorGUIUtility.labelWidth = 160;
                    EditorGUILayout.BeginHorizontal();
                    LegGrounding = (ELegGroundingView)EditorGUILayout.EnumPopup("View IK Grounding Stage:", LegGrounding);

                    if (displayGrInfo) GUI.color = Color.white * 0.6f;
                    if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Info), FGUI_Resources.ButtonStyle, GUILayout.Width(24), GUILayout.Height(18))) displayGrInfo = !displayGrInfo;
                    if (displayGrInfo) GUI.color = preC;

                    EditorGUILayout.EndHorizontal();
                    EditorGUIUtility.labelWidth = 0;


                    #region Info Display

                    if (displayGrInfo)
                    {
                        if (LegGrounding == ELegGroundingView.Analyze)
                        {
                            EditorGUILayout.HelpBox("Prepare settings for algorithm which will analyze source animation in order to collect useful data and detect foot touching ground moments, then use it for foot animation correction in the 'Processing' stage.", MessageType.Info);
                        }
                        else
                        {
                            EditorGUILayout.HelpBox("Using data gathered during 'Analyze' stage for applying active foot animation correction with parameters below.", MessageType.Info);
                        }
                    }

                    #endregion


                    GUILayout.Space(7);


                    #region Grounding GUI Panels

                    EditorGUILayout.BeginVertical(FGUI_Resources.ViewBoxStyle);
                    EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxBlankStyle);

                    if (LegGrounding == ELegGroundingView.Analyze)
                    {

                        #region Analyze View

                        if (SceneView.lastActiveSceneView != null)
                            if (SceneView.lastActiveSceneView.camera != null)
                            {
                                GUILayout.Space(3);
                                if (SceneView.lastActiveSceneView.camera.orthographic)
                                {
                                    if (AnimationDesignerWindow.ContainsProjectFileSave())
                                        if (GUILayout.Button(new GUIContent("  Switch Camera Back to Perspective", EditorGUIUtility.IconContent("Camera Icon").image), GUILayout.Height(22)))
                                        {
                                            SceneView.lastActiveSceneView.orthographic = false;
                                        }
                                }
                                else
                                {
                                    EditorGUILayout.HelpBox("It's easier to analyze with Scene Isometric Camera", MessageType.None);

                                    if (AnimationDesignerWindow.ContainsProjectFileSave())
                                        if (GUILayout.Button(new GUIContent(" Switch Scene Camera to Isometric", EditorGUIUtility.IconContent("Camera Icon").image), GUILayout.Height(22)))
                                        {
                                            SceneView.lastActiveSceneView.orthographic = true;
                                            Transform refT = refToMainSet.LatestAnimator.transform;
                                            float refScale = save.ScaleRef;
                                            SceneView.lastActiveSceneView.pivot = refT.TransformPoint(Vector3.right * 0.5f + Vector3.up * refScale * 0.2f) * (refScale) * 0.5f;
                                            SceneView.lastActiveSceneView.rotation = Quaternion.LookRotation(-refT.TransformPoint(Vector3.right));
                                            if (AnimationDesignerWindow.Get) AnimationDesignerWindow.Get.FrameTarget(refToMainSet.LatestAnimator.gameObject);
                                        }

                                }
                            }


                        GUILayout.Space(5);
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.HelpBox("During 'Analyze' stage, the IK is disabled for debugging purpose, switch to 'Processing' to see IK effect", MessageType.Info);
                        if (GUILayout.Button("Go to Processing", GUILayout.Height(26))) { LegGrounding = ELegGroundingView.Processing; }
                        EditorGUILayout.EndHorizontal();
                        GUILayout.Space(5);

                        SyncFootIKToggle(preC);
                        GUILayout.Space(3);

                        HeelGroundedTreshold = EditorGUILayout.Slider("Heel Grounding Threshold", HeelGroundedTreshold, -2f, 1f);
                        ToesGroundedTreshold = EditorGUILayout.Slider("Toes Grounding Threshold ", ToesGroundedTreshold, -2f, 2f);
                        //GUILayout.Space(4);
                        //HoldOnGround = EditorGUILayout.Slider("Hold On Ground", HoldOnGround, 0f, 0.25f);
                        GUILayout.Space(8);


                        bool changed = false;
                        if (HeelGroundedTreshold != FootDataAnalyze._latestHeelTesh) changed = true;
                        if (ToesGroundedTreshold != FootDataAnalyze._latestToesTesh) changed = true;
                        if (HoldOnGround != FootDataAnalyze._latestHoldOnGround) changed = true;
                        if (FloorGroundedOffset != FootDataAnalyze._latestFloorOffset) changed = true;
                        if (FloorGroundingAnalyzeMode != FootDataAnalyze._latestAnalyzeMode) changed = true;


                        if (FootDataAnalyze == null || FootDataAnalyze.analyzed == false)
                        {
                            GUI.backgroundColor = Color.green;
                            if (GUILayout.Button(new GUIContent("  Analyze animation clip using settings above", FGUI_Resources.Tex_Refresh))) { GenerateFootAnalyzeData(save, limb, clipMain.settingsForClip, ikSets, clipMain, HeelGroundedTreshold, ToesGroundedTreshold, HoldOnGround, clipMain.ResetRootPosition); }
                            GUI.backgroundColor = preBC;
                        }
                        else
                        {

                            EditorGUIUtility.labelWidth = 70;
                            AnimationDesignerWindow.DrawCurve(ref FootDataAnalyze.GroundingCurve, "Grounding:");
                            AnimationDesignerWindow.DrawCurveProgress(animProgr, 74);
                            EditorGUIUtility.labelWidth = 0;


                            if (changed)
                            {
                                GUI.backgroundColor = new Color(1f, 0.9f, 0.6f, 1f);
                                if (GUILayout.Button(new GUIContent("  Re-Analyze animation clip using settings above", FGUI_Resources.Tex_Refresh))) { GenerateFootAnalyzeData(save, limb, clipMain.settingsForClip, ikSets, clipMain, HeelGroundedTreshold, ToesGroundedTreshold, HoldOnGround, clipMain.ResetRootPosition); }
                                GUI.backgroundColor = preBC;
                                EditorGUILayout.HelpBox("Analysis parameters changed: to see difference, hit button above", MessageType.None);
                            }
                        }

                        GUILayout.Space(4);

                        if (FootDataAnalyze == null || FootDataAnalyze.analyzed == false)
                        {
                            EditorGUILayout.HelpBox("Not analyzed yet!", MessageType.Warning);
                        }
                        else
                        {
                            if (changed == false)
                            {
                                GUILayout.Space(4);
                                EditorGUILayout.HelpBox("Analyze Report: Average Foot Forward Velocity On Ground = " + (-FootDataAnalyze.approximateFootPushDirection.z) + "  Sides: " + (FootDataAnalyze.approximateFootPushDirection.x) + "  Dominant Axis: " + (-FootDataAnalyze.approximateFootPushDirectionDominant), MessageType.None);
                                GUILayout.Space(4);
                            }

                            FGUI_Inspector.FoldHeaderStart(ref ikAnalyzeExtraFoldot, "Extra Operations", FGUI_Resources.BGInBoxStyle);

                            if (ikAnalyzeExtraFoldot)
                            {

                                if (AnimationDesignerWindow.Get)
                                    if (AnimationDesignerWindow.Get.TargetClip)
                                    {
                                        GUILayout.Space(3);
                                        FloorGroundedOffset = EditorGUILayout.Slider("Grounding Floor Offset", FloorGroundedOffset, -0.1f, 0.1f);
                                        FloorGroundingAnalyzeMode = (LegAnimationClipMotion.EAnalyzeMode)EditorGUILayout.EnumPopup("Analyze Mode", FloorGroundingAnalyzeMode);
                                        GUILayout.Space(3);

                                        if (GUILayout.Button(new GUIContent("Grounding Curves to Pelvis Motion", "Extracting all analyzed legs grounding curves to pelvis motion which can be modified under 'Main & Hips IK Settings for a Current Clip' foldout below")))
                                        {
                                            AnimationCurve syncCurves = new AnimationCurve();

                                            int samples = Mathf.RoundToInt(refToMainSet.SettingsForClip.length * refToMainSet.SettingsForClip.frameRate) + 1;
                                            float stepProgr = 1f / (float)(samples);

                                            for (int f = 0; f <= samples; f++)
                                            {
                                                float averageValue = 0f;
                                                float progr = f * stepProgr;

                                                for (int i = 0; i < save.Limbs.Count; i++)
                                                {
                                                    var set = ikSets.GetIKSettingsForLimb(save.Limbs[i], save);
                                                    if (set == null) continue;
                                                    if (set.IsLegGroundingMode == false) continue;
                                                    //set.syncGrLegIKSettingsToOtherLegs = false;
                                                    averageValue = Mathf.Lerp(averageValue, set.FootDataAnalyze.GroundingCurve.Evaluate(progr), 0.5f);
                                                }

                                                syncCurves.AddKey(progr, 1f - averageValue);
                                            }

                                            syncCurves = AnimationGenerateUtils.ReduceKeyframes(syncCurves, 0.05f);
                                            refToMainSet.PelvisOffsetYEvaluate = syncCurves;
                                        }

                                        GUILayout.Space(2);

                                        ExportGroundingCurve = EditorGUILayout.Toggle(new GUIContent("Export Grounding Curve", "It can be useful for developers programming procedural leg grounding algorithms"), ExportGroundingCurve);

                                        if (ExportGroundingCurve)
                                        {
                                            DrawGroundingAddParamsButton(this);
                                            //EditorGUILayout.HelpBox("Add '" + GetName + "-H' float variable to the animator to get values with this animation playing", MessageType.None);
                                            //if (GUI.Button(GUILayoutUtility.GetLastRect(), "", EditorStyles.label)) GUIUtility.systemCopyBuffer = GetName + "-H";
                                        }

                                        GUILayout.Space(2);
                                    }
                            }

                            GUILayout.EndVertical();
                            GUILayout.Space(4);
                        }


                        GUILayout.Space(6);
                        if (AnimationDesignerWindow.Get) AnimationDesignerWindow.Get.DrawPlaybackPanel();

                        #endregion


                        SyncSettingsIfSwitched(save, limb, ikSets);

                    }
                    else if (LegGrounding == ELegGroundingView.Processing)
                    {

                        for (int i = 0; i < ikSets.LimbIKSetups.Count; i++)
                        {
                            if (ikSets.LimbIKSetups[i] == this) continue;
                            ikSets.LimbIKSetups[i].LegGrounding = ELegGroundingView.Processing;
                        }

                        #region Processing View

                        GUILayout.Space(3);
                        if (syncGrLegIKSettingsToOtherLegs) GUI.color = new Color(0.7f, 1f, 0.7f);

                        AnimationDesignerWindow.GUIDrawFloatSeconds("Ground IK Blend Duration", ref FootGroundBlendDuration, 0f, 0.3f, "sec", "How long should be transition from ungrounded to grounded state of the leg animation basing on the prepared grounding curve during analyze stage");

                        EditorGUILayout.BeginHorizontal();
                        MaxLegStretch = EditorGUILayout.Slider(new GUIContent("Max Leg Stretch", "Limiting maximum leg limb stretch, so it will never snap to totally straight pose."), MaxLegStretch, 0.5f, 1.1f);
                        AnimationDesignerWindow.DrawCurve(ref MaxLegStretchEvaluation, "", 50);
                        EditorGUILayout.EndHorizontal();
                        Rect r = AnimationDesignerWindow.DrawSliderProgress(Mathf.InverseLerp(0.5f, 1.1f, MaxLegStretch) * MaxLegStretchEvaluation.Evaluate(animProgr), 148f, 108f);
                        AnimationDesignerWindow.DrawCurveProgressOnR(animProgr, 136f, 56f, r);

                        SyncFootIKToggle(preC);

                        FGUI_Inspector.DrawUILine(0.4f, 0.4f, 1, 10, 0.975f);

                        if (syncGrLegIKSettingsToOtherLegs) GUI.color = new Color(0.7f, 1f, 0.7f);
                        AnimationDesignerWindow.GUIDrawFloatPercentage(ref HoldXPosWhenGrounded, new GUIContent("Hold X When Grounded", "Holding static, animation average X Position when foot is beign grounded, to remove foot sliding to sides when putting steps.\nIT SHOULD BE SET TO ZERO WHEN DESIGNING STRAFE WALK/RUN ANIMATIONS!"));
                        AnimationDesignerWindow.GUIDrawFloatPercentage(ref HoldZPosWhenGrounded, new GUIContent("Hold Z When Grounded", "Holding static, animation average Z Position when foot is beign grounded, to remove foot sliding to sides when putting steps.\nIT SHOULD BE SET TO ZERO WHEN DESIGNING WALK/RUN FORWARD/BACKWARD ANIMATIONS!"));

                        if (FootLinearize <= 0f) GUI.color = new Color(1f, 1f, 1f, preC.a * 0.6f);
                        AnimationDesignerWindow.GUIDrawFloatPercentage(ref FootLinearize, new GUIContent("Linear Z Movement", "Trying to set constant foot movement speed when grounded to make possible character movement translation look perfectly synced with legs.\n\nSetting HoldX or HoldZ values higher helps blending this feature!"));

                        if (FootLinearize > 0f)
                        {
                            //FootLinearizeSpeedMul = EditorGUILayout.FloatField("Linear Speed Multiplier:", FootLinearizeSpeedMul);
                            FootLinearStartAdjust = EditorGUILayout.Vector3Field("Linear Start Adjust:", FootLinearStartAdjust);
                            if (FootLinearStartAdjust != Vector3.zero) FootLinearEndAdjust = EditorGUILayout.Vector3Field("Linear End Adjust:", FootLinearEndAdjust);
                            if (FootLinearizeSpeedMul < 0.1f) FootLinearizeSpeedMul = 1f; else if (FootLinearizeSpeedMul > 2f) FootLinearizeSpeedMul = 2f;
                        }

                        GUI.color = preC;

                        GUILayout.Space(6);

                        if (syncGrLegIKSettingsToOtherLegs) GUI.color = new Color(0.7f, 1f, 0.7f);
                        FGUI_Inspector.FoldHeaderStart(ref grIkSwingsFoldout, " Leg Swing Animation Modify", FGUI_Resources.BGInBoxStyle);

                        if (grIkSwingsFoldout)
                        {
                            GUILayout.Space(4);
                            AnimationDesignerWindow.GUIDrawFloatPercentage(ref FootSwingsMultiply, new GUIContent("Swings Blend", "Blend amount for all of three parameters below"));
                            GUILayout.Space(4);
                            FootRaiseMultiply = EditorGUILayout.Slider(new GUIContent("Foot Raise Multiply", "Increasing / decreasing slightly height of foots when legs is ungrounded"), FootRaiseMultiply, -1f, 2f);
                            FootMildMotionZ = EditorGUILayout.Slider(new GUIContent("Foot Mild Forward Swings", "Increasing / decreasing slightly forward/backward foot motion"), FootMildMotionZ, -0.5f, 1f);
                            FootMildMotionX = EditorGUILayout.Slider(new GUIContent("Foot Mild Sides Swings", "Increasing / decreasing slightly left/right side foot motion"), FootMildMotionX, -0.5f, 1f);
                            GUILayout.Space(4);
                        }

                        EditorGUILayout.EndVertical();

                        if (syncGrLegIKSettingsToOtherLegs) GUI.color = preC;

                        GUILayout.Space(6);

                        if (LegMode == ELegMode.Grounding)
                            FGUI_Inspector.FoldHeaderStart(ref grIkOffsFoldout, " Extra IK Offsets", FGUI_Resources.BGInBoxStyle);
                        else
                            FGUI_Inspector.FoldHeaderStart(ref grIkOffsFoldout, " Extra IK Position Offsets", FGUI_Resources.BGInBoxStyle);

                        if (grIkOffsFoldout)
                        {
                            GUILayout.Space(6);
                            FGUI_Inspector.FoldHeaderStart(ref grIkOffsConstFoldout, " Constant IK Offsets", FGUI_Resources.BGInBoxStyle);
                            if (grIkOffsConstFoldout)
                            {
                                GUILayout.Space(4);
                                DrawIkBasicPositionOffsetsGUI(animProgr, ref IKPositionOffset, ref IKPosOffMul, ref IKPosOffEvaluate, "Knee", 24f);

                                DrawIkBasicRotationOffsetsGUI(animProgr, ref IKRotationOffset, ref IKRotOffMul, ref IKRotOffEvaluate);

                                GUILayout.Space(-3);
                                UseFootRotationMapping = EditorGUILayout.Toggle(new GUIContent("Foot Rotation Mapping:", "Automatically mapping foot rotation to be aligned with ground, if it fails, you can disable it, but in 90% situations it should work correctly"), UseFootRotationMapping);
                                GUILayout.Space(6);
                            }
                            EditorGUILayout.EndVertical();



                            GUILayout.Space(8);
                            FGUI_Inspector.FoldHeaderStart(ref grIkOffsWhenGrFoldout, " When Grounded IK Offsets", FGUI_Resources.BGInBoxStyle);
                            if (grIkOffsWhenGrFoldout)
                            {
                                GUILayout.Space(4);
                                DrawIkBasicPositionOffsetsGUI(animProgr, ref IKWhenGroundedPositionOffset, ref IKWhenGroundedPosOffMul, ref IKWhenGroundedPosOffEvaluate, "", 24f);
                                //DrawIkBasicRotationOffsetsGUI(ref IKWhenGroundedRotationOffset, ref IKWhenGroundedRotOffMul, ref IKWhenUngroundedRotOffEvaluate);

                                GUILayout.Space(2);
                                EditorGUILayout.BeginHorizontal();
                                AnimationDesignerWindow.GUIDrawFloatPercentage(ref FootSnapToLatestGrounded, new GUIContent("Snap To Latest Grounded", "Glueing foot to latest grounded position (dictated by the grounding curve)"));
                                AnimationDesignerWindow.DrawCurve(ref FootSnapToLatestGroundedEvaluation, "", 50);
                                EditorGUILayout.EndHorizontal();

                                if (FootSnapToLatestGrounded > 0f)
                                {
                                    EditorGUIUtility.labelWidth = 180;
                                    EditorGUILayout.BeginHorizontal();
                                    FootSnapToGrFrameOffset = EditorGUILayout.Slider(new GUIContent("Grounded Pos Frame Offset", "Using foot position just when foot was grounded (0) or when ended (1) or in the middle (0.5).\n\nIf you encounter foot stuttering caused by foot glue ground snap, changing this value can help it!"), FootSnapToGrFrameOffset, 0f, 1f);
                                    AnimationDesignerWindow.DrawCurve(ref FootSnapToGrFrameOffsetEvaluation, "", 50);
                                    EditorGUILayout.EndHorizontal();
                                }

                                GUILayout.Space(4);

                                UseGroundedFeet = EditorGUILayout.Toggle(new GUIContent("Fix Feet Rotation", "Transitioning leg child bones rotations to the T-Pose local rotation when foot is being grounded to solve rare case of feet animation when foot is on the ground"), UseGroundedFeet);
                                GUILayout.Space(4);
                            }
                            EditorGUILayout.EndVertical();



                            GUILayout.Space(8);
                            FGUI_Inspector.FoldHeaderStart(ref grIkOffsWhenUngrFoldout, " When Ungrounded IK Offsets", FGUI_Resources.BGInBoxStyle);
                            if (grIkOffsWhenUngrFoldout)
                            {
                                GUILayout.Space(4);
                                DrawIkBasicPositionOffsetsGUI(animProgr, ref IKWhenUngroundedPositionOffset, ref IKWhenUngroundedPosOffMul, ref IKWhenUngroundedPosOffEvaluate, "", 24f);
                                //DrawIkBasicRotationOffsetsGUI(ref IKWhenUngroundedRotationOffset, ref IKWhenUngroundedRotOffMul, ref IKWhenUngroundedRotOffEvaluate);
                            }

                            EditorGUILayout.EndVertical();

                        }

                        EditorGUILayout.EndVertical();


                        if (FootDataAnalyze != null)
                        {
                            GUILayout.Space(6);

                            if (displayProcessingGroundingCurve == false)
                            {
                                if (GUILayout.Button("^ Quick View for Grounding Curve ^", EditorStyles.centeredGreyMiniLabel)) displayProcessingGroundingCurve = !displayProcessingGroundingCurve;
                            }
                            else
                            {
                                if (GUILayout.Button(" Quick View for Grounding Curve ", EditorStyles.centeredGreyMiniLabel)) displayProcessingGroundingCurve = !displayProcessingGroundingCurve;
                                AnimationDesignerWindow.DrawCurve(ref FootDataAnalyze.GroundingCurve, "");
                                AnimationDesignerWindow.DrawCurveProgress(animProgr, 2, 60);

                            }
                        }



                        //GUILayout.Space(9);
                        //EditorGUILayout.LabelField("TODO: Impulse for Spring On Grounded", EditorStyles.centeredGreyMiniLabel);


                        if (AnimationDesignerWindow.Get)
                        {
                            FGUI_Inspector.DrawUILine(0.4f, 0.4f, 1, 18, 0.975f);
                            AnimationDesignerWindow.Get.DrawPlaybackPanel();
                        }


                        #endregion

                        SyncSettingsIfSwitched(save, limb, ikSets);

                    }


                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndVertical();


                    #endregion



                    GUILayout.Space(9);

                    EditorGUIUtility.labelWidth = 220;
                    //GenerateFootGroundCurve = EditorGUILayout.Toggle(new GUIContent("Export Foot Grounding Height Curve:", "Including foot height animation curve (analyze tab - Grounding Curve) during generating clip file with name of the limb. ('" + limb.GetName + "')"), GenerateFootGroundCurve);

                    GenerateFootStepEvents = EditorGUILayout.TextField(new GUIContent("Generate foot-step events:", "[Left empty to not use this feature]\nGenerating footstep events in animation clip basing on the 'Grounding' curve from the Analyze Tab.\nYou can check this events when you double-click on the generated AnimationClip file in the project browser."), GenerateFootStepEvents);
                    EditorGUIUtility.labelWidth = 0;

                    if (string.IsNullOrEmpty(GenerateFootStepEvents) == false)
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(16);
                        EditorGUILayout.LabelField("Events Time Offset:", GUILayout.Width(124));
                        GeneratedStepEventsTimeOffset = GUILayout.HorizontalSlider(GeneratedStepEventsTimeOffset, -0.2f, 0.2f);
                        GUILayout.Space(6);
                        EditorGUILayout.LabelField(System.Math.Round(GeneratedStepEventsTimeOffset, 2) + " sec", GUILayout.Width(60));
                        EditorGUILayout.EndHorizontal();
                    }

                    GUILayout.Space(4);


                }

                #endregion

                else
                {

                    #region Basic IK Position / Rotation Settings


                    if (IKType == EIKType.ArmIK)
                    {
                        GUILayout.Space(10);
                        AnimationDesignerWindow.GUIDrawFloatPercentage(ref ShoulderBlend, new GUIContent("   IK Shoulders Blend:", FGUI_Resources.Tex_Default, "When ik target position is too far, shoulders rotation can be triggered to ease reaching far elements animation."));
                        GUILayout.Space(4);

                        EditorGUILayout.BeginHorizontal();
                        MaxLegStretch = EditorGUILayout.Slider(new GUIContent("Max Arm Stretch", "Limiting maximum arm limb stretch, so it will never snap to totally straight pose."), MaxLegStretch, 0.5f, 1.1f);
                        AnimationDesignerWindow.DrawCurve(ref MaxLegStretchEvaluation, "", 50);
                        EditorGUILayout.EndHorizontal();
                        GUILayout.Space(4);
                    }
                    else if (IKType == EIKType.FootIK)
                    {
                        GUILayout.Space(4);
                        EditorGUILayout.BeginHorizontal();
                        MaxLegStretch = EditorGUILayout.Slider("Max Leg Stretch", MaxLegStretch, 0.5f, 1.1f);
                        AnimationDesignerWindow.DrawCurve(ref MaxLegStretchEvaluation, "", 50);
                        EditorGUILayout.EndHorizontal();
                        GUILayout.Space(4);


                        if (ExportGroundingCurve)
                        {
                            DrawGroundingAddParamsButton(this);
                        }
                    }

                    GUILayout.Space(6);

                    FGUI_Inspector.FoldHeaderStart(ref ikCoordsFoldout, "  IK Positions Modify", FGUI_Resources.BGInBoxStyle);

                    if (ikCoordsFoldout)
                    {
                        GUILayout.Space(4);

                        string hintTitle = "";
                        if (IKType == EIKType.ArmIK) hintTitle = "Elbow"; else if (IKType == EIKType.FootIK) hintTitle = "Knee";
                        DrawIkBasicPositionOffsetsGUI(animProgr, ref IKPositionOffset, ref IKPosOffMul, ref IKPosOffEvaluate, hintTitle);

                        EditorGUILayout.BeginHorizontal();


                        if (UseMultiStillPoints) GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1f);
                        if (GUILayout.Button(FGUI_Resources.Tex_Movement, FGUI_Resources.ButtonStyle, GUILayout.Width(22), GUILayout.Height(19)))
                        {
                            UseMultiStillPoints = !UseMultiStillPoints;
                            if (UseMultiStillPoints) IKPosStillMul = 1f;
                        }
                        GUI.backgroundColor = preBC;

                        if (UseMultiStillPoints)
                        {
                            if (MultiStillPoints == null) MultiStillPoints = new List<StillPoint>();
                            EditorGUILayout.LabelField("Multi-Still Points Mode", EditorStyles.centeredGreyMiniLabel);
                            GUI.backgroundColor = new Color(0.7f, 1f, 0.7f, 1f);

                            if (MultiStillPoints.Count == 0)
                            {
                                if (GUILayout.Button("+ Add Point", FGUI_Resources.ButtonStyle, GUILayout.Width(78), GUILayout.Height(19)))
                                {
                                    StillPoint p = new StillPoint(animProgr, GatherStillPosition(limb.LastBone.T), limb.LastBone.rot);
                                    MultiStillPoints.Add(p);
                                }
                            }
                            else
                            {
                                if (GUILayout.Button("+", FGUI_Resources.ButtonStyle, GUILayout.Width(22), GUILayout.Height(19)))
                                {
                                    StillPoint p = new StillPoint(animProgr, GatherStillPosition(limb.LastBone.T), limb.LastBone.rot);
                                    MultiStillPoints.Add(p);
                                }
                            }
                            GUI.backgroundColor = preBC;

                        }
                        else
                        {
                            EditorGUI.BeginChangeCheck();

                            EditorGUIUtility.labelWidth = 104;
                            IKStillPosition = EditorGUILayout.Vector3Field(" IK Still Position:", IKStillPosition);
                            EditorGUIUtility.labelWidth = 0;

                            //IKStillPosition = EditorGUILayout.Vector3Field(new GUIContent("  IK Still Position:", FGUI_Resources.Tex_Movement), IKStillPosition);
                            if (IKPosStillMul < 0.1f) { if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Refresh, "Copy current animator hand position and paste into stil position"), EditorStyles.label, GUILayout.Width(20), GUILayout.Height(17))) { IKStillPosition = GatherStillPosition(limb.LastBone.T); IKPosStillMul = 1f; } }
                            //if (IKPosStillMul < 0.1f) { if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Refresh, "Copy current animator hand position and paste into stil position"), EditorStyles.label, GUILayout.Width(20), GUILayout.Height(17))) { if (IKStillWorldPos) { if (refToMainSet.LatestAnimator.parent == null) IKStillPosition = limb.LastBone.pos; else IKStillPosition = refToMainSet.LatestAnimator.parent.InverseTransformPoint(limb.LastBone.pos); } else IKStillPosition = refToMainSet.LatestAnimator.transform.InverseTransformPoint(limb.LastBone.pos); IKPosStillMul = 1f; } }

                            if (EditorGUI.EndChangeCheck()) LatelyAdjusted = EWasAdjusted.Still;
                        }

                        GUILayout.Space(6);
                        EditorGUIUtility.labelWidth = 22;
                        IKStillWorldPos = EditorGUILayout.Toggle(new GUIContent(FGUI_Resources.Tex_Website, "Enable ik align to world position (works only if edited character is placed in zero position/using root motion)"), IKStillWorldPos, GUILayout.Width(38));
                        EditorGUIUtility.labelWidth = 0;

                        EditorGUILayout.EndHorizontal();

                        EditorGUI.BeginChangeCheck();

                        GUILayout.Space(4);
                        EditorGUILayout.BeginHorizontal();
                        AnimationDesignerWindow.GUIDrawFloatPercentage(ref IKPosStillMul, new GUIContent("Still Blend:", "How much still ik target position should be applied."));
                        AnimationDesignerWindow.DrawCurve(ref IKPosStillEvaluate, "", 70);
                        EditorGUILayout.EndHorizontal();

                        if (EditorGUI.EndChangeCheck()) if (!UseMultiStillPoints) LatelyAdjusted = EWasAdjusted.Still;

                        Rect r = AnimationDesignerWindow.DrawSliderProgress(IKPosStillMul * IKPosStillEvaluate.Evaluate(animProgr), 68f, 127f);
                        AnimationDesignerWindow.DrawCurveProgressOnR(animProgr, 140f, 60f, r);

                        if (!UseMultiStillPoints)
                        {
                            GUILayout.Space(9);
                            AnimationDesignerWindow.GUIDrawFloatPercentage(ref IKPosYOriginal, new GUIContent("Still Y Original:", "Keeping Y position of original animation wrist position for the ik target position. It can be useful for carrying animations. If switching it offsets your hand ik position too low or too high, try adjusting it with 'Position Offset' parameter above."));
                            GUILayout.Space(3);
                        }
                        else
                        {
                            GUILayout.Space(8);

                            #region Multi still points panel

                            Rect lRect = EditorGUILayout.GetControlRect(GUILayout.Height(16));

                            float minX = lRect.position.x;
                            float maxX = lRect.position.x + lRect.width;

                            lRect.position += new Vector2(0, -6);

                            #region Draw timeline rect

                            GUI.Box(lRect, GUIContent.none, EditorStyles.helpBox);

                            float wd = lRect.width;
                            float wdprgr = wd * animProgr;

                            GUI.color = new Color(1f, .5f, .5f, 0.4f);
                            Rect timeRect = new Rect(lRect.x + wdprgr - 2, lRect.y, 4, 16);
                            GUI.DrawTexture(timeRect, FGUI_Resources.Tex_Rectangle);
                            GUI.color = preC;
                            bool someSelected = false;
                            Vector3 mousePos = Vector3.zero;

                            #endregion

                            bool mouseDrag = false;
                            bool allowTimeSlide = true;

                            #region Start end drag

                            if (Event.current != null)
                            {
                                mousePos = Event.current.mousePosition;

                                if (Event.current.button == 0)
                                {
                                    if (Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag)
                                        mouseDrag = true;
                                }

                                if (Event.current.type == EventType.MouseUp)
                                {
                                    _DraggingStillPoint = null;
                                    _DraggingTime = false;
                                }
                            }

                            #endregion



                            for (int i = 0; i < MultiStillPoints.Count; i++)
                            {
                                var point = MultiStillPoints[i];

                                #region Draw magnet buttton and detect dragging

                                Rect pointDraw = new Rect(lRect.x + wd * point.time - 8, lRect.y - 5, 14, 14);

                                if (_SelectedStillPoint == point)
                                {
                                    someSelected = true;
                                    GUI.color = Color.white;
                                }
                                else GUI.color = new Color(0.7f, 0.7f, 0.7f, 0.85f);

                                var evt = Event.current.type;
                                if (GUI.Button(pointDraw, AnimationDesignerWindow.Tex_Magnet, EditorStyles.label))
                                {
                                    mouseDrag = false;
                                    allowTimeSlide = false;
                                    _DraggingTime = false;

                                    if (_SelectedStillPoint == point)
                                    {
                                        if (_DraggingStillPoint == null) _SelectedStillPoint = null;
                                    }
                                    else
                                    {
                                        _SelectedStillPoint = point;
                                        someSelected = true;
                                    }
                                }
                                else
                                {
                                    if (mouseDrag && !_DraggingTime)
                                    {
                                        pointDraw.position -= new Vector2(6, 0);
                                        pointDraw.width += 12;
                                        pointDraw.height += 8;

                                        if (pointDraw.Contains(mousePos))
                                        {
                                            allowTimeSlide = false;
                                            if (evt == EventType.MouseDrag)
                                                if (_DraggingStillPoint == null) { _DraggingStillPoint = point; Event.current.Use(); }
                                        }
                                    }
                                }

                                #endregion

                                GUI.color = new Color(0.7f, 0.7f, 0.7f, 0.9f);
                                float range = wd * point.blendInDuration;
                                Rect lRange = new Rect(pointDraw.x - range + 7, pointDraw.y + 15, range, 1);
                                if (lRange.x < minX) { lRange.width -= (minX - lRange.x); lRange.x = minX; }
                                GUI.DrawTexture(lRange, AnimationDesignerWindow.Tex_Pixel);

                                range = wd * (point.blendOutDuration);
                                lRange = new Rect(pointDraw.x + wd * point.keepFor + 7, pointDraw.y + 17, range, 1);
                                if (lRange.x + lRange.width > maxX) { float diff = ((lRange.x + lRange.width) - maxX); lRange.width -= diff; }
                                if (lRange.x < maxX) GUI.DrawTexture(lRange, AnimationDesignerWindow.Tex_Pixel);

                                GUI.color = preC;
                                range = wd * (point.keepFor);
                                lRange = new Rect(pointDraw.x + 7, pointDraw.y + 17, range, 1);
                                if (lRange.x + lRange.width > maxX) { float diff = ((lRange.x + lRange.width) - maxX); lRange.width -= diff; }
                                GUI.DrawTexture(lRange, AnimationDesignerWindow.Tex_Pixel);
                            }


                            #region Dragging point or time

                            if (mouseDrag)
                            {
                                Rect detectRect = new Rect(lRect);
                                lRect.position += new Vector2(0, -40);
                                lRect.height += 80;

                                if (lRect.Contains(mousePos))
                                {
                                    bool timeSlide = false;
                                    if (allowTimeSlide)
                                    {
                                        if (detectRect.Contains(mousePos))
                                            if (Event.current.type == EventType.MouseDown)
                                            {
                                                _DraggingTime = true;
                                                timeSlide = true;
                                            }

                                        if (_DraggingTime)
                                        {
                                            _DraggingStillPoint = null;
                                            AnimationDesignerWindow.Get.SetAnimationProgressManually((mousePos.x - 32) / lRect.width);
                                            timeSlide = true;
                                        }
                                    }

                                    if (timeSlide == false)
                                    {
                                        if (_DraggingStillPoint != null) _DraggingStillPoint.time = Mathf.Clamp01((mousePos.x - 32) / lRect.width);
                                    }
                                }
                            }

                            #endregion


                            GUI.color = preC;

                            GUILayout.Space(6);

                            if (someSelected == false) _SelectedStillPoint = null;

                            #region Point menu

                            #region Point menu info

                            if (MultiStillPoints.Count == 0)
                            {
                                EditorGUILayout.LabelField("Add some still points first", EditorStyles.centeredGreyMiniLabel);
                            }
                            else
                            {
                                if (_SelectedStillPoint == null)
                                {
                                    EditorGUILayout.LabelField("Select some still point to display settings!", EditorStyles.centeredGreyMiniLabel);
                                }
                                else
                                #endregion
                                {

                                    RefreshMultiPointsTiming();

                                    GUILayout.BeginHorizontal();
                                    _SelectedStillPoint.time = EditorGUILayout.Slider("Point time: ", _SelectedStillPoint.time, 0f, 1f);
                                    if (GUILayout.Button(FGUI_Resources.GUIC_Remove, FGUI_Resources.ButtonStyle, GUILayout.Width(24), GUILayout.Height(19))) { MultiStillPoints.Remove(_SelectedStillPoint); }
                                    GUILayout.EndHorizontal();
                                    GUILayout.Space(2);
                                    _SelectedStillPoint.blendInDuration = EditorGUILayout.Slider("Fade in duration: ", _SelectedStillPoint.blendInDuration, 0f, 0.4f);
                                    _SelectedStillPoint.blendOutDuration = EditorGUILayout.Slider("Fade out duration: ", _SelectedStillPoint.blendOutDuration, 0f, 0.4f);
                                    GUILayout.Space(4);
                                    _SelectedStillPoint.keepFor = EditorGUILayout.Slider("Keep for: ", _SelectedStillPoint.keepFor, 0f, 1f);
                                    GUILayout.Space(8);
                                    GUILayout.BeginHorizontal();
                                    _SelectedStillPoint.pos = EditorGUILayout.Vector3Field("IK Position: ", _SelectedStillPoint.pos);
                                    GUILayout.Space(3);
                                    if (GUILayout.Button(FGUI_Resources.Tex_Refresh, EditorStyles.label, GUILayout.Width(24), GUILayout.Height(16))) { _SelectedStillPoint.pos = GatherStillPosition(limb.LastBone.T); _SelectedStillPoint.rot = limb.LastBone.rot.eulerAngles; }

                                    if (_MultiStillDrawExtra) GUI.backgroundColor = new Color(0.5f, 1f, 0.5f, 1f);

                                    if (GUILayout.Button(FGUI_Resources.Tex_Tweaks, FGUI_Resources.ButtonStyle, GUILayout.Width(24), GUILayout.Height(17))) { _MultiStillDrawExtra = !_MultiStillDrawExtra; }
                                    GUILayout.Space(1);
                                    GUILayout.EndHorizontal();

                                    if (_MultiStillDrawExtra) EditorGUILayout.BeginVertical(FGUI_Resources.HeaderBoxStyle);
                                    else
                                        _SelectedStillPoint.rot = EditorGUILayout.Vector3Field("IK Rotation: ", _SelectedStillPoint.rot);

                                    if (_MultiStillDrawExtra)
                                    {
                                        _SelectedStillPoint.positionBlend = EditorGUILayout.Slider("Position Blend: ", _SelectedStillPoint.positionBlend, 0f, 1f);
                                        GUILayout.Space(4);
                                    }

                                    if (_MultiStillDrawExtra)
                                        _SelectedStillPoint.rotationBlend = EditorGUILayout.Slider("Rotation Blend: ", _SelectedStillPoint.rotationBlend, 0f, 1f);


                                    if (_MultiStillDrawExtra)
                                    {
                                        GUILayout.Space(4);

                                        EditorGUIUtility.labelWidth = 220;
                                        _SelectedStillPoint.TransitionSettingsDraw = (StillPoint.ETransitionSettings)EditorGUILayout.EnumPopup("Draw Extra Transition Settings:", _SelectedStillPoint.TransitionSettingsDraw);
                                        EditorGUIUtility.labelWidth = 0;


                                        if (_SelectedStillPoint.TransitionSettingsDraw == StillPoint.ETransitionSettings.PositionOffset)
                                        {
                                            GUILayout.Space(5);
                                            _SelectedStillPoint.FootTransitionOffset = EditorGUILayout.Vector3Field("Transition Pos Offset:", _SelectedStillPoint.FootTransitionOffset);

                                            EditorGUILayout.BeginHorizontal();

                                            EditorGUILayout.LabelField("In:", GUILayout.Width(24));
                                            AnimationDesignerWindow.DrawCurve(ref _SelectedStillPoint.FootTransitionInOffsetCurve, "", 70);

                                            GUILayout.Space(16);
                                            EditorGUILayout.LabelField("Out:", GUILayout.Width(28));
                                            AnimationDesignerWindow.DrawCurve(ref _SelectedStillPoint.FootTransitionOutOffsetCurve, "", 70);

                                            EditorGUILayout.EndHorizontal();

                                            EditorGUILayout.LabelField("Curve which defines for example IK foot elevate on transition", EditorStyles.centeredGreyMiniLabel);
                                            GUILayout.Space(3);
                                        }
                                        else if (_SelectedStillPoint.TransitionSettingsDraw == StillPoint.ETransitionSettings.RotationOffset)
                                        {
                                            GUILayout.Space(5);
                                            _SelectedStillPoint.FootTransitionRotationOffset = EditorGUILayout.Vector3Field("Transition Angles Offset:", _SelectedStillPoint.FootTransitionRotationOffset);

                                            EditorGUILayout.BeginHorizontal();

                                            EditorGUILayout.LabelField("In:", GUILayout.Width(24));
                                            AnimationDesignerWindow.DrawCurve(ref _SelectedStillPoint.FootRotTransitionInOffsetCurve, "", 70, 0, -1);

                                            GUILayout.Space(16);
                                            EditorGUILayout.LabelField("Out:", GUILayout.Width(28));
                                            AnimationDesignerWindow.DrawCurve(ref _SelectedStillPoint.FootRotTransitionOutOffsetCurve, "", 70, 0, -1);

                                            EditorGUILayout.EndHorizontal();

                                            EditorGUILayout.LabelField("Curve which defines extra IK rotation motion on transition", EditorStyles.centeredGreyMiniLabel);
                                            GUILayout.Space(3);
                                        }
                                        else if (_SelectedStillPoint.TransitionSettingsDraw == StillPoint.ETransitionSettings.PelvisOffset)
                                        {
                                            GUILayout.Space(5);

                                            if (_SelectedStillPoint.HipsOffsetMode != StillPoint.EOffsetMode.OutOffset)
                                                _SelectedStillPoint.HipsTransitionPositionOffset = EditorGUILayout.Vector3Field("Transition Hips Offset:", _SelectedStillPoint.HipsTransitionPositionOffset);
                                            else
                                                _SelectedStillPoint.HipsTransitionOutPositionOffset = EditorGUILayout.Vector3Field("Out Hips Offset:", _SelectedStillPoint.HipsTransitionOutPositionOffset);

                                            EditorGUILayout.BeginHorizontal();

                                            EditorGUILayout.LabelField("In:", GUILayout.Width(24));
                                            AnimationDesignerWindow.DrawCurve(ref _SelectedStillPoint.HipsTransitionInOffsetCurve, "", 70, 0, -1);

                                            GUILayout.Space(16);
                                            EditorGUILayout.LabelField("Out:", GUILayout.Width(28));
                                            AnimationDesignerWindow.DrawCurve(ref _SelectedStillPoint.HipsTransitionOutOffsetCurve, "", 70, 0, -1);

                                            GUILayout.Space(26);
                                            _SelectedStillPoint.HipsOffsetMode = (StillPoint.EOffsetMode)EditorGUILayout.EnumPopup(_SelectedStillPoint.HipsOffsetMode);

                                            EditorGUILayout.EndHorizontal();

                                            EditorGUILayout.LabelField("Curve which defines extra hips motion on transition", EditorStyles.centeredGreyMiniLabel);
                                            GUILayout.Space(3);
                                        }


                                        if (/*_SelectedStillPoint.TransitionSettingsDraw == StillPoint.ETransitionSettings.PositionOffset || */_SelectedStillPoint.TransitionSettingsDraw == StillPoint.ETransitionSettings.None)
                                        {
                                            GUILayout.Space(5);
                                            EditorGUILayout.BeginHorizontal();

                                            EditorGUILayout.LabelField("Progress In:", GUILayout.Width(68));
                                            AnimationDesignerWindow.DrawCurve(ref _SelectedStillPoint.FootTransitionInProgressCurve, "", 50);

                                            GUILayout.Space(16);
                                            EditorGUILayout.LabelField("Progress Out:", GUILayout.Width(76));
                                            AnimationDesignerWindow.DrawCurve(ref _SelectedStillPoint.FootTransitionOutProgressCurve, "", 50);

                                            EditorGUILayout.EndHorizontal();

                                            EditorGUILayout.LabelField("Curve which defines IK transition to target position", EditorStyles.centeredGreyMiniLabel);
                                            GUILayout.Space(3);
                                        }

                                    }

                                    if (_MultiStillDrawExtra)
                                    {
                                        EditorGUILayout.EndVertical();
                                        GUI.backgroundColor = preBC;
                                    }

                                }
                            }

                            #endregion

                            #endregion

                            GUILayout.Space(4);
                        }


                        FGUI_Inspector.DrawUILine(0.3f, 0.5f, 1, 15, 0.975f);
                        //EditorGUILayout.BeginHorizontal();
                        //GUI.color = new Color(1f, 1f, 1f, 0.7f);

                        if (AlignTo != null)
                        {
                            //EditorGUILayout.BeginVertical();
                            AlignTo.DrawGUI();
                            //EditorGUILayout.EndVertical();
                        }

                        //alignTo = (Transform)EditorGUILayout.ObjectField(new GUIContent("Align IK With:", "You can assign some custom transform reference to for ik target to follow. Can be useful for carrying animation - then you can put here counter arm bone and adjust positioning with ik position offset parameter above."), alignTo, typeof(Transform), true);
                        GUI.color = preC;
                        //GUILayout.Space(6);

                        //if (Searchable.IsSetted)
                        //    if (selectorHelperId != "")
                        //        if (selectorHelperId == "algn" + GetIndex)
                        //        {
                        //            object g = Searchable.Get();
                        //            if (g != null) if (g is Transform)
                        //                    AlignTo.DefineNewBoneArmatureRelation(g as Transform);

                        //            selectorHelperId = "";
                        //        }


                        //if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_DownFold), EditorStyles.label, GUILayout.Width(20), GUILayout.Height(18)))
                        //{
                        //    selectorHelperId = "algn" + GetIndex;
                        //    AnimationDesignerWindow.ShowBonesSelector("Choose Your Character Model Bone", save.GetAllArmatureBonesList, AnimationDesignerWindow.GetMenuDropdownRect(), true);
                        //}

                        //EditorGUILayout.EndHorizontal();

                        if (AlignTo.IsSet)
                        {
                            AnimationDesignerWindow.GUIDrawFloatPercentage(ref AlignToBlend, new GUIContent("Align To Blend:"));
                            if (IKPosStillMul > 0.6f) if (AlignToBlend > 0.1f)
                                {
                                    EditorGUILayout.HelpBox("IK Still Position Blend is overriding 'Aling To' feature!", MessageType.None);
                                }
                        }

                        if (AlignTo.IsSet)
                            AnimationDesignerWindow.GUIDrawFloatPercentage(ref AlignRotationToBlend, new GUIContent("Align Rotation Blend:"));

                        GUILayout.Space(3);

                        if (IKType == EIKType.ArmIK)
                        {
                            if (refToMainSet != null)
                                if (refToMainSet.OffsetHandsIKBlend < 0.5f) { EditorGUILayout.HelpBox("Offset Y Pos With Pelvis is multiplied by Offset Hands Blend in the bottom tab of this IK settings!", MessageType.None); }
                            AnimationDesignerWindow.GUIDrawFloatPercentage(ref OffsetWithPelvisBlend, new GUIContent("Offset Y Pos With Pelvis", "Offsetting hand ik position with additional pelvis motion applied under 'Main & Hips IK Settings for a Current Clip' tab.\n\n!!! This value is multiplied by 'Offset Hands Blend' value in the mentioned tab."));
                            GUILayout.Space(3);
                        }
                    }

                    EditorGUILayout.EndVertical();

                    GUILayout.Space(8);

                    if (IKType != EIKType.ChainIK)
                    {

                        FGUI_Inspector.FoldHeaderStart(ref ikRotsFoldout, "  IK Rotations Modify", FGUI_Resources.BGInBoxStyle);

                        if (ikRotsFoldout)
                        {

                            DrawIkBasicRotationOffsetsGUI(animProgr, ref IKRotationOffset, ref IKRotOffMul, ref IKRotOffEvaluate);


                            EditorGUILayout.BeginHorizontal();
                            IKStillRotation = EditorGUILayout.Vector3Field(new GUIContent("  IK Still Rotation:", FGUI_Resources.Tex_Rotation), IKStillRotation);
                            if (IKRotStillMul < 0.1f) { if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Refresh, "Copy current animation wrist rotation and paste into still rotation"), EditorStyles.label, GUILayout.Width(20), GUILayout.Height(17))) { IKStillRotation = GatherStillRotation(limb.LastBone.T); IKRotStillMul = 1f; } }

                            GUILayout.Space(6);
                            EditorGUIUtility.labelWidth = 22;
                            IKStillWorldRot = EditorGUILayout.Toggle(new GUIContent(FGUI_Resources.Tex_Website, "Enable ik align to world rotation (works only if edited character is placed in zero position/using root motion)"), IKStillWorldRot, GUILayout.Width(38));
                            EditorGUIUtility.labelWidth = 0;


                            EditorGUILayout.EndHorizontal();
                            GUILayout.Space(6);
                            EditorGUILayout.BeginHorizontal();
                            AnimationDesignerWindow.GUIDrawFloatPercentage(ref IKRotStillMul, new GUIContent("Still Blend:"));
                            AnimationDesignerWindow.DrawCurve(ref IKRotStillEvaluate, "", 70);
                            EditorGUILayout.EndHorizontal();


                            Rect r = AnimationDesignerWindow.DrawSliderProgress(IKRotStillMul * IKRotStillEvaluate.Evaluate(animProgr), 68, 127);
                            AnimationDesignerWindow.DrawCurveProgressOnR(animProgr, 140f, 60f, r);

                            GUILayout.Space(8);

                            if (IKType != EIKType.ChainIK)
                                AnimationDesignerWindow.GUIDrawFloatPercentage(ref IKRotationBlend, new GUIContent("Overall IK Rotation Blend", "Blend IK Foot/Hand rotation to choosed target rotation %"));
                            GUILayout.Space(4);
                        }

                        EditorGUILayout.EndVertical();

                    }


                    #endregion


                }


                GUILayout.Space(3);
                GUI.enabled = true;

            }


            public Vector3 GatherStillPosition(Transform lastBone)
            {
                Vector3 still = IKStillPosition;

                if (IKStillWorldPos)
                {
                    if (refToMainSet.LatestAnimator.parent == null) still = lastBone.position;
                    else still = refToMainSet.LatestAnimator.parent.InverseTransformPoint(lastBone.position);
                }
                else
                {
                    still = refToMainSet.LatestAnimator.transform.InverseTransformPoint(lastBone.position);
                }

                return still;
            }

            public Vector3 GatherStillRotation(Transform lastBone)
            {
                Vector3 still = IKStillRotation;

                if (IKStillWorldRot)
                {
                    still = lastBone.eulerAngles;
                }
                else
                {
                    still = FEngineering.QToLocal(refToMainSet.LatestAnimator.rotation, lastBone.rotation).eulerAngles;
                    //if (refToMainSet.LatestAnimator.parent == null) still = FEngineering.QToLocal(refToMainSet.LatestAnimator.rotation, lastBone.rotation).eulerAngles;
                    //else still = FEngineering.QToLocal(refToMainSet.LatestAnimator.parent.rotation, lastBone.rotation).eulerAngles;
                }

                return still;
            }


            #region Additional GUI Panels


            public static void DrawGroundingAddParamsButton(IKSet set)
            {
                if (AnimationDesignerWindow.Get)
                {
                    if (AnimationDesignerWindow.Get.GetMecanim)
                    {
                        UnityEditor.Animations.AnimatorController contr = AnimationGenerateUtils.GetStoredHumanoidIKPreviousController;
                        if (contr == null) contr = (UnityEditor.Animations.AnimatorController)AnimationDesignerWindow.Get.GetMecanim.runtimeAnimatorController;

                        if (contr != null)
                            if (AnimationDesignerWindow.AnimatorHasParam(contr, set.GetName + "-H") == false)
                                if (GUILayout.Button(new GUIContent("Add '" + set.GetName + "-H' Parameter to the Animator", EditorGUIUtility.IconContent("AnimatorController Icon").image), GUILayout.Height(18)))
                                {
                                    contr.AddParameter(new AnimatorControllerParameter() { defaultFloat = 0f, name = set.GetName + "-H", type = AnimatorControllerParameterType.Float });
                                    EditorUtility.SetDirty(AnimationDesignerWindow.Get.GetMecanim);
                                }
                    }
                }
            }

            public static void DrawIKHipsParameters(float progr, ADClipSettings_Main clipMain, AnimationDesignerSave save)
            {
                Color preguiC = GUI.color;

                string ff = FGUI_Resources.GetFoldSimbol(mainSetFoldout, false);

                if (GUILayout.Button(ff + "  Main & Hips IK Settings for a Current Clip  " + ff, FGUI_Resources.HeaderStyle))
                {
                    mainSetFoldout = !mainSetFoldout;
                }

                if (mainSetFoldout)
                {
                    int displays = clipMain.GUI_OnDisplay(true, 0, save);

                    GUILayout.Space(8);
                    EditorGUILayout.HelpBox("Settings below are affecting all Foot IK limbs with Grounding mode", MessageType.None);
                    GUILayout.Space(9);

                    if (displays < 3) GUI.color = new Color(1f, 1f, 0.7f, 1f);
                    clipMain.Pelvis = (Transform)EditorGUILayout.ObjectField("Pelvis Transform:", clipMain.GetPelvis(save, save.ReferencePelvis.parent), typeof(Transform), true);

                    if (displays < 4)
                    {
                        EditorGUILayout.HelpBox("Make sure 'Pelvis' have assigned correct PELVIS bone", displays < 2 ? MessageType.Info : MessageType.None);
                        Rect rr = GUILayoutUtility.GetLastRect();
                        if (GUI.Button(rr, GUIContent.none, GUIStyle.none)) { clipMain.GUI_OnDisplay(true, 5); }
                    }
                    GUI.color = preguiC;

                    GUILayout.Space(6);


                    #region Axis curves for Pelvis

                    EditorGUILayout.BeginVertical(FGUI_Resources.ViewBoxStyle);
                    EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxBlankStyle);

                    AnimationDesignerWindow.GUIDrawFloatPercentage(ref clipMain.PelvisOffsetsBlend, new GUIContent("Pelvis Offsets Blend:"));
                    GUILayout.Space(4);

                    EditorGUIUtility.labelWidth = 180;
                    EditorGUILayout.BeginHorizontal();
                    clipMain.PelvisConstantYOffset = EditorGUILayout.FloatField("Pelvis Constant Y Offset:", clipMain.PelvisConstantYOffset);

                    if (clipMain.PelvisCurves01Mode) GUI.backgroundColor = Color.green;
                    if (GUILayout.Button("Curve 01", GUILayout.Width(70))) clipMain.PelvisCurves01Mode = !clipMain.PelvisCurves01Mode;
                    if (clipMain.PelvisCurves01Mode) GUI.backgroundColor = Color.white;

                    float minVal = clipMain.PelvisCurves01Mode ? 0f : -1f;

                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUIUtility.labelWidth = 150;
                    clipMain.PelvisYOffset = EditorGUILayout.FloatField("Pelvis Y Offset:", clipMain.PelvisYOffset);
                    EditorGUIUtility.labelWidth = 0;
                    AnimationDesignerWindow.DrawCurve(ref clipMain.PelvisOffsetYEvaluate, "", 50, 0f, minVal, 1f, 1f);
                    EditorGUILayout.EndHorizontal();

                    float rrOffset = 94f;

                    AnimationDesignerWindow.DrawCurveProgressOnR(progr, rrOffset);


                    EditorGUILayout.BeginHorizontal();
                    EditorGUIUtility.labelWidth = 150;
                    clipMain.PelvisXOffset = EditorGUILayout.FloatField("Pelvis Sides (x) Offset:", clipMain.PelvisXOffset);
                    EditorGUIUtility.labelWidth = 0;
                    AnimationDesignerWindow.DrawCurve(ref clipMain.PelvisOffsetXEvaluate, "", 50, 0f, minVal, 1f, 1f);
                    EditorGUILayout.EndHorizontal();

                    AnimationDesignerWindow.DrawCurveProgressOnR(progr, rrOffset);


                    EditorGUILayout.BeginHorizontal();
                    EditorGUIUtility.labelWidth = 150;
                    clipMain.PelvisZOffset = EditorGUILayout.FloatField("Pelvis Forward (z) Offset:", clipMain.PelvisZOffset);
                    EditorGUIUtility.labelWidth = 0;
                    AnimationDesignerWindow.DrawCurve(ref clipMain.PelvisOffsetZEvaluate, "", 50, 0f, minVal, 1f, 1f);
                    EditorGUILayout.EndHorizontal();

                    AnimationDesignerWindow.DrawCurveProgressOnR(progr, rrOffset);


                    EditorGUILayout.EndVertical();

                    if (AnimationDesignerWindow.Get)
                    {
                        EditorGUILayout.BeginHorizontal();
                        AnimationDesignerWindow.Get.DrawPlaybackButton();
                        GUILayout.Space(5);
                        AnimationDesignerWindow.Get.DrawPlaybackTimeSlider();
                        EditorGUILayout.EndHorizontal();
                    }

                    EditorGUILayout.EndVertical();

                    #endregion


                    GUILayout.Space(6);

                    EditorGUILayout.BeginHorizontal();

                    //???
                    EditorGUILayout.BeginVertical(GUILayout.Width(18)); GUILayout.Space(4);
                    if (GUILayout.Button(FGUI_Resources.GetFoldSimbol(ikMainGroundFoldout, true), EditorStyles.label, GUILayout.Width(18))) ikMainGroundFoldout = !ikMainGroundFoldout;
                    EditorGUILayout.EndVertical();

                    clipMain.IKGroundLevel = EditorGUILayout.FloatField("Floor Y Height Level:", clipMain.IKGroundLevel);
                    EditorGUILayout.EndHorizontal();

                    if (ikMainGroundFoldout)
                    {
                        EditorGUI.indentLevel++;
                        clipMain.IKGroundNormal = EditorGUILayout.Vector3Field("Ground Plane Normal:", clipMain.IKGroundNormal).normalized;
                        EditorGUI.indentLevel--;
                    }

                    GUILayout.Space(6);
                    clipMain.OffsetHandsIKBlend = EditorGUILayout.Slider(new GUIContent("Offset Hands Blend", "Add pelvis position offset to hand ik"), clipMain.OffsetHandsIKBlend, 0f, 1f);


                    GUILayout.Space(5);
                    FGUI_Inspector.FoldHeaderStart(ref mainSetFoldoutExtra, "Extra Settings", FGUI_Resources.BGInBoxStyle);

                    if (mainSetFoldoutExtra)
                    {
                        clipMain.ElasticnesSettings.Enabled = EditorGUILayout.Toggle("Use Hips Elasticity", clipMain.ElasticnesSettings.Enabled);

                        if (clipMain.ElasticnesSettings.Enabled)
                        {
                            GUILayout.Space(4);
                            AnimationDesignerWindow.DrawCurve(ref clipMain.PelvisElasticityEvaluate, "Hips Elasticity Blend");
                            AnimationDesignerWindow.DrawCurveProgress(progr);
                            GUILayout.Space(4);

                            if (clipMain.SettingsForClip)
                            {
                                if (clipMain.SettingsForClip.hasRootCurves)
                                {
                                    bool drawRt = true;
                                    if (clipMain.LatestAnimator)
                                    {
                                        Animator anim = clipMain.LatestAnimator.GetAnimator();
                                        if (anim) if (anim.applyRootMotion) drawRt = false;

                                        if (drawRt)
                                        {
                                            bool rootOrig = false;

                                            if (clipMain != null) if (clipMain.SettingsForClip != null)
                                                {
                                                    rootOrig = ADRootMotionBakeHelper.ClipContainsAnyRootCurves(clipMain.SettingsForClip);
                                                }

                                            if (rootOrig)
                                            {
                                                if (anim) EditorGUILayout.BeginHorizontal();
                                                EditorGUILayout.HelpBox("To avoid Elasticity loop-bounce, enable on your animator 'Apply Root Motion' (but it will move your model to zero scene position) and remember to set 'Motion Influence' to ZERO!", MessageType.Warning);
                                                if (anim)
                                                {
                                                    if (GUILayout.Button("Apply\nRoot Motion", GUILayout.Height(40)))
                                                    {
                                                        anim.applyRootMotion = true;
                                                        if (clipMain.ElasticnesSettings != null) clipMain.ElasticnesSettings.MotionInfluence = 0f;
                                                        EditorUtility.SetDirty(anim);
                                                    }
                                                    EditorGUILayout.EndHorizontal();
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            GUILayout.Space(6);
                            clipMain.ElasticnesSettings.DrawParamsGUI(clipMain.PelvisElasticityEvaluate.Evaluate(progr), false, true, false);
                        }

                        GUILayout.Space(3);

                    }

                    EditorGUILayout.EndVertical();



                    GUILayout.Space(-4);
                }
                else
                {
                    clipMain.GUI_OnDisplay(false);
                }


                GUILayout.Space(10);
            }


            void DrawIkBasicPositionOffsetsGUI(float progr, ref Vector3 IKPositionOffset, ref float IKPosOffMul, ref AnimationCurve IKPosOffEvaluate, string hintName = "Elbow", float xOff = 0f)
            {
                EditorGUI.BeginChangeCheck();

                IKPositionOffset = EditorGUILayout.Vector3Field(new GUIContent("  IK Position Offset:", FGUI_Resources.Tex_Movement), IKPositionOffset);
                GUILayout.Space(4);

                EditorGUILayout.BeginHorizontal();
                AnimationDesignerWindow.GUIDrawFloatPercentage(ref IKPosOffMul, new GUIContent("Offset Blend:"));
                AnimationDesignerWindow.DrawCurve(ref IKPosOffEvaluate, "", 70);
                EditorGUILayout.EndHorizontal();

                Rect r = AnimationDesignerWindow.DrawSliderProgress(IKPosOffMul * IKPosOffEvaluate.Evaluate(progr), 68, 127);
                AnimationDesignerWindow.DrawCurveProgressOnR(progr, 140f + xOff, 60f, r);

                if (hintName != "")
                {
                    GUILayout.Space(12);
                    IKHintOffset = EditorGUILayout.Vector3Field(new GUIContent("   " + hintName + " Hint Offset:", FGUI_Resources.TexTargetingIcon, "Hint offset which drives 'knee'/'elbow' positioning for ik.\n\nIf correction required, for elbow in most cases you will set Z value to negative + some X offsets depending if it's right or left limb. For knees you will set Z value positive + some X axis offsets."), IKHintOffset);
                }

                GUILayout.Space(12);

                if (EditorGUI.EndChangeCheck()) LatelyAdjusted = EWasAdjusted.Offset;
            }


            void DrawIkBasicRotationOffsetsGUI(float progr, ref Vector3 IKRotationOffset, ref float IKRotOffMul, ref AnimationCurve IKRotOffEvaluate)
            {
                GUILayout.Space(4);
                IKRotationOffset = EditorGUILayout.Vector3Field(new GUIContent("  IK Rotation Offset:", FGUI_Resources.Tex_Rotation), IKRotationOffset);
                GUILayout.Space(6);
                EditorGUILayout.BeginHorizontal();
                AnimationDesignerWindow.GUIDrawFloatPercentage(ref IKRotOffMul, new GUIContent("Offset Blend:"));
                AnimationDesignerWindow.DrawCurve(ref IKRotOffEvaluate, "", 70);
                EditorGUILayout.EndHorizontal();

                Rect r = AnimationDesignerWindow.DrawSliderProgress(IKRotOffMul * IKRotOffEvaluate.Evaluate(progr), 68, 127);
                AnimationDesignerWindow.DrawCurveProgressOnR(progr, 140f, 60f, r);

                GUILayout.Space(12);
            }


            void SyncFootIKToggle(Color preCol)
            {
                EditorGUIUtility.labelWidth = 198;
                if (syncGrLegIKSettingsToOtherLegs) GUI.color = new Color(0.5f, 1f, 0.2f, 1f);
                syncGrLegIKSettingsToOtherLegs = EditorGUILayout.Toggle("Sync other legs settings with this", syncGrLegIKSettingsToOtherLegs);
                if (syncGrLegIKSettingsToOtherLegs) GUI.color = preCol;
                EditorGUIUtility.labelWidth = 0;
            }


            #endregion



            #endregion


            public float OffsetWithPelvisBlend = 1f;

            internal Vector3 GetHintPosition(Vector3 defHintPos, Transform root, float progr)
            {

                if (refToMainSet != null)
                {
                    if (IKType == EIKType.ArmIK)
                        defHintPos += refToMainSet.LatestAnimator.transform.TransformVector(refToMainSet.GetHipsOffset(progr, false) * (refToMainSet.OffsetHandsIKBlend * OffsetWithPelvisBlend));

                    if (IKType != EIKType.ChainIK)
                        if (AutoHintMode == EIKAutoHintMode.FollowHipsRotation)
                        {
                            if (refToMainSet.Pelvis)
                                return defHintPos + refToMainSet.Pelvis.TransformVector(IKHintOffset);
                        }
                }

                return defHintPos + root.TransformVector(IKHintOffset);
            }


            bool ComputeGroundedIn(float animationProgress)
            {
                bool grounded = FootDataAnalyze.GroundedIn(animationProgress);

                #region Backup

                //if (LegGrounding == ELegGroundingView.Processing)
                //{
                //    if (!grounded) // if not yet grounded
                //    {
                //        if (GroundLater < 0f) // if we want to ground sooner
                //        {
                //            grounded = FootDataAnalyze.GroundedIn(animationProgress - (GroundLater));
                //        }

                //        if (UnGroundLater > 0f) // Keep grounded a bit longer
                //        {
                //            grounded = FootDataAnalyze.GroundedIn(animationProgress - (UnGroundLater));
                //        }
                //    }
                //    else // if grounded
                //    {
                //        if (GroundLater > 0f) // if we want to ground later
                //        {
                //            grounded = FootDataAnalyze.GroundedIn(animationProgress - (GroundLater));
                //        }

                //        if (UnGroundLater > 0f) // Unground sooner
                //        {
                //            grounded = FootDataAnalyze.GroundedIn(animationProgress - (UnGroundLater));
                //        }
                //    }
                //}
                #endregion

                return grounded;
            }

            /// <summary> If null then mixed </summary>
            bool? _computeBlendInToGrounded = false;
            float ComputeGroundedBlend(float animationProgress)
            {
                bool grounded = ComputeGroundedIn(animationProgress);

                if (grounded)
                {
                    _computeBlendInToGrounded = true;
                    return 1f;
                }

                // Predict next grounding with sampling
                float nextGroundingInProgr = -1f;
                float blendDurInProgr = SecToProgr(FootGroundBlendDuration);

                int toSampleCount = Mathf.CeilToInt(blendDurInProgr * clipFramerate);

                if (toSampleCount > 0)
                {
                    float nextFade = 0f;

                    //UnityEngine.Debug.Log( "sec=" + FootGroundBlendDuration + " cliplen=" + clipLength +  "  durToProgr= " + SecToProgr(FootGroundBlendDuration) + "  clipKeyStep = " + clipKeyStep + "   /  = " + (blendDurInProgr * clipFramerate) + "   /  = " + (blendDurInProgr * clipKeyStep) + "   toSample = " + toSample);
                    float progrStep = blendDurInProgr / (float)toSampleCount;
                    float boostDensity = 32f;

                    for (int i = 1; i <= toSampleCount * boostDensity; i++)
                    {
                        float progr = animationProgress + progrStep * ((float)i / boostDensity);
                        float cycProgr = progr;
                        if (cycProgr > 1f) cycProgr -= 1f;

                        if (ComputeGroundedIn(cycProgr))
                        {
                            nextGroundingInProgr = progr;
                            _computeBlendInToGrounded = true;
                            break;
                        }
                    }

                    float preFade = 0f;

                    if (nextGroundingInProgr != -1f)
                    {
                        float diff = nextGroundingInProgr - animationProgress;
                        //UnityEngine.Debug.Log("diff = " + diff + "    diff / dur = " + diff / blendDurInProgr);
                        nextFade = 1f - (diff / blendDurInProgr);//ret
                    }
                    else
                    {
                        _computeBlendInToGrounded = false;
                    }

                    //else // Postdicting if was grounded in previous frames
                    {
                        //_computeBlendInToGrounded = false;
                        float wasGroundingInPreviousProgr = -1f;

                        for (int i = 1; i <= toSampleCount * boostDensity; i++)
                        {
                            float progr = animationProgress - progrStep * ((float)i / boostDensity);
                            float cycProgr = progr;
                            if (cycProgr < 0f) cycProgr += 1f;
                            if (ComputeGroundedIn(cycProgr))
                            {
                                wasGroundingInPreviousProgr = progr;
                                break;
                            }
                        }

                        if (wasGroundingInPreviousProgr != -1f)
                        {
                            float diff = animationProgress - wasGroundingInPreviousProgr;
                            //UnityEngine.Debug.Log("diff = " + diff + "    diff / dur = " + diff / blendDurInProgr);
                            preFade = 1f - (diff / blendDurInProgr);
                        }

                    }

                    if (preFade == 0f) { return nextFade; }
                    if (nextFade == 0f) { return preFade; }

                    //if ( nextFade > preFade) 
                    _computeBlendInToGrounded = null;
                    //else
                    //    if (nextFade < preFade) _computeBlendInToGrounded = false;

                    return Mathf.Lerp(preFade, 1f, nextFade);
                }

                return 0f;
            }


            //Vector2? _memoGroundSnapMargins = null;
            internal void ComputeFootIKGrounding(ref Vector3 newIKPos, ref Quaternion newIKRot, Transform root, float progr, FIK_IKProcessor processor, ADClipSettings_Main main)
            {
                if (LegGrounding == ELegGroundingView.Analyze) return;

                Matrix4x4 rootMx = Matrix4x4.TRS(root.position + main.LatestInternalRootMotionOffset, root.rotation, root.lossyScale);
                Matrix4x4 rootMxInv = rootMx.inverse;

                float groundingBlendIn = ComputeGroundedBlend(progr);
                //UnityEngine.Debug.Log("blendin = " + groundingBlendIn);

                Vector3 ikMotionEdit = newIKPos;

                float raise = FootRaiseMultiply * FootSwingsMultiply;

                if (raise != 0f || FootMildMotionZ * FootSwingsMultiply != 0f || FootMildMotionX * FootSwingsMultiply != 0f)
                {
                    Vector3 motionEditLocal = rootMxInv.MultiplyPoint(ikMotionEdit);

                    if (raise < 0f) motionEditLocal.y = Mathf.LerpUnclamped(motionEditLocal.y, FootDataAnalyze._groundToFootHeight, -raise * 0.5f);
                    else if (raise > 0f) motionEditLocal.y = Mathf.LerpUnclamped(FootDataAnalyze._groundToFootHeight, motionEditLocal.y, 1f + raise * 0.5f);

                    motionEditLocal.z = Mathf.LerpUnclamped(motionEditLocal.z, 0f, FootMildMotionZ * FootSwingsMultiply);
                    motionEditLocal.x = Mathf.LerpUnclamped(motionEditLocal.x, 0f, FootMildMotionX * FootSwingsMultiply);

                    ikMotionEdit = rootMx.MultiplyPoint(motionEditLocal);
                }

                Vector3 constOff = root.TransformVector(IKPositionOffset) * IKPosOffMul * IKPosOffEvaluate.Evaluate(progr);

                if (IKWhenUngroundedPosOffMul > 0f) ikMotionEdit = Vector3.LerpUnclamped(ikMotionEdit, ikMotionEdit + root.TransformVector(IKWhenUngroundedPositionOffset), IKWhenUngroundedPosOffMul * IKWhenUngroundedPosOffEvaluate.Evaluate(progr) * (1f - groundingBlendIn));

                newIKPos = ikMotionEdit;

                if (groundingBlendIn <= 0f) newIKPos += constOff;

                #region Leg stretching limiting when ungrounded

                float maxStr = MaxLegStretch * MaxLegStretchEvaluation.Evaluate(progr);

                if (maxStr < 1.1f)
                {
                    float stretch = processor.GetStretchValue(newIKPos);

                    if (stretch > maxStr)
                    {
                        float len = (maxStr * processor.GetLimbLength());
                        newIKPos = processor.StartIKBone.srcPosition + (newIKPos - processor.StartIKBone.srcPosition).normalized * len;
                    }
                }

                #endregion


                if (groundingBlendIn > 0f)
                {
                    Vector3 groundIKPos = newIKPos;


                    #region Foot Analyze - from Local to World

                    Vector3 localPos = rootMxInv.MultiplyPoint(groundIKPos);

                    float staticX = FootDataAnalyze.approximateFootLocalXPosDuringPush;
                    if (LegGrounding != ELegGroundingView.Analyze) localPos.x = Mathf.LerpUnclamped(localPos.x, staticX, HoldXPosWhenGrounded);

                    float staticZ = FootDataAnalyze.approximateFootLocalZPosDuringPush;
                    if (LegGrounding != ELegGroundingView.Analyze) localPos.z = Mathf.LerpUnclamped(localPos.z, staticZ, HoldZPosWhenGrounded);

                    groundIKPos = rootMx.MultiplyPoint(localPos);

                    #endregion


                    #region Foot on ground plane

                    Vector3 localGroundPlanePos = rootMxInv.MultiplyPoint(groundIKPos);
                    Vector3 posOnGroundPlane = Vector3.ProjectOnPlane(localGroundPlanePos, (refToMainSet.IKGroundNormal));
                    localGroundPlanePos.y = posOnGroundPlane.y + FootDataAnalyze._groundToFootHeight + refToMainSet.IKGroundLevel;
                    groundIKPos = rootMx.MultiplyPoint(localGroundPlanePos);

                    #endregion


                    #region Foot Linearinze

                    if (FootLinearize > 0f)
                    {

                        float preUngr, nextUngr;
                        Vector3 newLinPos = rootMxInv.MultiplyPoint(groundIKPos);
                        bool transitioning = false;
                        bool transitioningIn = false;

                        if (!ComputeGroundedIn(progr)) // Not Grounded Yet - Transitioning
                        {
                            transitioning = true;
                            preUngr = FindPreviousUngroundedProgr(progr, true);
                            nextUngr = FindNextUngroundedProgr(progr, true);

                            float nearerProgr = preUngr;
                            if (Mathf.Abs(preUngr - progr) > Mathf.Abs(nextUngr - progr))
                            {
                                nearerProgr = nextUngr;
                                transitioningIn = false;
                            }
                            else
                            {
                                transitioningIn = true;
                            }

                            Vector3 transitionInPos = FootDataAnalyze.GetFootLocalPosition(nearerProgr);
                            Vector3 localCurr = rootMxInv.MultiplyPoint(groundIKPos);
                            transitionInPos.y = localCurr.y;

                            if (HoldXPosWhenGrounded > 0.1f)
                            {
                                float mild = 1f - FootMildMotionZ;
                                transitionInPos.z = Mathf.Lerp(localCurr.z, transitionInPos.z * mild, groundingBlendIn);
                            }
                            else
                                transitionInPos.z = localCurr.z;

                            if (HoldZPosWhenGrounded > 0.1f)
                            {
                                float mild = 1f - FootMildMotionX;
                                transitionInPos.x = Mathf.Lerp(localCurr.x, transitionInPos.x * mild, groundingBlendIn);
                            }
                            else
                                transitionInPos.x = localCurr.x;

                            newLinPos = Vector3.Lerp(localCurr, transitionInPos, groundingBlendIn);
                        }
                        else
                        {
                            preUngr = FindPreviousUngroundedProgr(progr, false);
                            nextUngr = FindNextUngroundedProgr(progr, false);

                            Vector3 startPos = (FootDataAnalyze.GetFootLocalPosition(preUngr));

                            Vector3 endPos = (FootDataAnalyze.GetFootLocalPosition(nextUngr));

                            float progrBetween = Mathf.InverseLerp(preUngr, nextUngr, progr);
                            //UnityEngine.Debug.Log("Pre " + preUngr + "  next  " + nextUngr + "  curr: " + progr + " between: " + progrBetween);

                            if (HoldXPosWhenGrounded > 0.1f)
                            {
                                float mild = 1f - FootMildMotionZ;
                                newLinPos.z = Mathf.Lerp(startPos.z * mild * FootLinearizeSpeedMul, endPos.z * mild * FootLinearizeSpeedMul, progrBetween);
                            }

                            if (HoldZPosWhenGrounded > 0.1f)
                            {
                                float mild = 1f - FootMildMotionX;
                                newLinPos.x = Mathf.Lerp(startPos.x * mild * FootLinearizeSpeedMul, endPos.x * mild * FootLinearizeSpeedMul, progrBetween);
                            }

                            if (HoldXPosWhenGrounded < 0.02f && HoldZPosWhenGrounded < 0.02f)
                            {
                                newLinPos.x = Mathf.Lerp(startPos.x * FootLinearizeSpeedMul, endPos.x * FootLinearizeSpeedMul, progrBetween);
                                newLinPos.z = Mathf.Lerp(startPos.z * FootLinearizeSpeedMul, endPos.z * FootLinearizeSpeedMul, progrBetween);
                            }

                            newLinPos += Vector3.Lerp(FootLinearEndAdjust, FootLinearStartAdjust, progrBetween); // * progrBetween;
                        }

                        newLinPos = rootMx.MultiplyPoint(newLinPos);

                        groundIKPos = Vector3.Lerp(groundIKPos, newLinPos, FootLinearize * groundingBlendIn);

                        if (transitioning)
                        {
                            if (transitioningIn)
                                groundIKPos += rootMx.MultiplyVector(FootLinearStartAdjust * FootLinearize * groundingBlendIn);
                            else
                                groundIKPos += rootMx.MultiplyVector(FootLinearEndAdjust * FootLinearize * groundingBlendIn);
                        }

                        #region Backup

                        //if (!ComputeGroundedIn(progr)) // Not Grounded Yet - Transitioning
                        //{
                        //    preUngr = FindPreviousUngroundedProgr(progr, true);
                        //    nextUngr = FindNextUngroundedProgr(progr, true);
                        //    //preUngr = (progr - blendDurInProgr * grOneMin);
                        //    //nextUngr = (progr + blendDurInProgr * grOneMin);

                        //    float nearerProgr = preUngr;
                        //    if (Mathf.Abs(preUngr - progr) > Mathf.Abs(nextUngr - progr)) nearerProgr = nextUngr;

                        //    Vector3 transitionInPos = root.TransformPoint(FootDataAnalyze.GetFootLocalPosition(nearerProgr));
                        //    groundIKPos = Vector3.Lerp(groundIKPos, transitionInPos, FootLinearize * groundingBlendIn);
                        //}
                        //else
                        //{
                        //    float blendDurInProgr = SecToProgr(FootGroundBlendDuration) * 0.6f;
                        //    float grOneMin = 1f - groundingBlendIn;

                        //    preUngr = FindPreviousUngroundedProgr(progr, false);
                        //    nextUngr = FindNextUngroundedProgr(progr, false);

                        //    Vector3 startPos = FootDataAnalyze.GetFootLocalPosition(preUngr);
                        //    Vector3 endPos = FootDataAnalyze.GetFootLocalPosition(nextUngr);

                        //    float progrBetween = Mathf.InverseLerp(preUngr, nextUngr, progr);

                        //    Vector3 newLinPos = root.InverseTransformPoint(groundIKPos);

                        //    if (HoldXPosWhenGrounded > 0.1f)
                        //    {
                        //        float mild = 1f - FootMildMotionZ;
                        //        newLinPos.z = Mathf.Lerp(startPos.z * mild, endPos.z * mild, progrBetween);
                        //    }

                        //    if (HoldZPosWhenGrounded > 0.1f)
                        //    {
                        //        float mild = 1f - FootMildMotionX;
                        //        newLinPos.x = Mathf.Lerp(startPos.x * mild, endPos.x * mild, progrBetween);
                        //    }

                        //    newLinPos = root.TransformPoint(newLinPos);

                        //    groundIKPos = Vector3.Lerp(groundIKPos, newLinPos, FootLinearize * groundingBlendIn);
                        //}
                        #endregion


                    }

                    #endregion


                    #region Foot Rotation

                    Quaternion footRot;

                    if (UseFootRotationMapping)
                    {
                        //Quaternion footRot = FootDataAnalyze._initFootRot;
                        Vector3 projectedPlaneNormal = root.TransformDirection(refToMainSet.IKGroundNormal);
                        footRot = Quaternion.LookRotation(Vector3.ProjectOnPlane(root.forward, projectedPlaneNormal), projectedPlaneNormal) * FootDataAnalyze._footRotMapping;
                    }
                    else
                    {
                        footRot = processor.IKTargetRotation;
                    }


                    if (IKRotOffMul > 0f) footRot = Quaternion.SlerpUnclamped(footRot, footRot * Quaternion.Euler(IKRotationOffset), IKRotOffMul * IKRotOffEvaluate.Evaluate(progr));


                    #region Grounding feet rotations 

                    if (UseGroundedFeet)
                    {
                        if (groundingBlendIn > 0f)
                            if (processor != null)
                                if (processor.EndIKBone != null)
                                    if (processor.EndIKBone.transform != null)
                                    {
                                        Transform foot = processor.EndIKBone.transform;

                                        #region Reset list of feet child bones

                                        if (feetBones == null || feetBones.Count == 0 || feetBones[0] == null || feetBones[0].TempTransform == null)
                                        {
                                            feetBones = new List<ADBoneKeyPose>();
                                            var bones = AnimationDesignerWindow.Get.S.TPose.BonesCoords;
                                            if (bones != null)
                                                foreach (Transform item in foot.GetComponentsInChildren<Transform>())
                                                {
                                                    if (item == foot) continue;
                                                    for (int i = 0; i < bones.Count; i++)
                                                    {
                                                        if (bones[i].BoneName == item.name) { bones[i].TempTransform = item; feetBones.Add(bones[i]); break; }
                                                    }
                                                }
                                        }

                                        #endregion

                                        if (feetBones != null)
                                            if (feetBones.Count > 0)
                                            {
                                                for (int f = 0; f < feetBones.Count; f++)
                                                {
                                                    var bn = feetBones[f];
                                                    bn.TempTransform.localRotation = Quaternion.Lerp(bn.TempTransform.localRotation, bn.LocalRotation, groundingBlendIn);
                                                }
                                            }
                                    }
                    }

                    #endregion


                    #endregion



                    #region Snap to latest grounded

                    if (FootSnapToLatestGroundedEvaluation == null) FootSnapToLatestGroundedEvaluation = AnimationCurve.EaseInOut(0f, 1f, 1f, 1f);
                    if (FootSnapToGrFrameOffsetEvaluation == null) FootSnapToGrFrameOffsetEvaluation = AnimationCurve.EaseInOut(0f, 1f, 1f, 1f);

                    float footSnapToLatestGroundedBlend = FootSnapToLatestGroundedEvaluation.Evaluate(progr) * FootSnapToLatestGrounded;

                    if (footSnapToLatestGroundedBlend > 0f)
                    {
                        float snapAt = FootSnapToGrFrameOffsetEvaluation.Evaluate(progr) * FootSnapToGrFrameOffset;
                        float progrRatio = SecToProgr(0.05f);

                        // Compute ground state margins
                        bool nowGrounded = FootDataAnalyze.GroundedIn(progr);

                        // Grounded between left and right
                        float leftMargin = 0f;
                        float rightMargin = 1f;

                        float sampleOff = 0f;

                        #region Compute margins

                        if (nowGrounded)
                        {
                            // Find ungrounded on the left and ungrounded on the right
                            leftMargin = FindUngroundedProgr(progr, false);
                            rightMargin = FindUngroundedProgr(progr, true);
                        }
                        else // Fade out / or fade in   in progress
                        {
                            if (_computeBlendInToGrounded == true) // Fade in to grounded
                            {
                                // left is first grounded on the right   right is ungrounded after
                                leftMargin = FindUngroundedProgr(progr, true, true);
                                rightMargin = FindUngroundedProgr(leftMargin, true);
                                sampleOff = progrRatio;
                            }
                            else if (_computeBlendInToGrounded == false) // Fade out to ungrounded or just ungrounded
                            {
                                // right is first grounded on the left   left is ungrounded before 
                                rightMargin = FindUngroundedProgr(progr, false, true);
                                leftMargin = FindUngroundedProgr(rightMargin, false, false);
                                sampleOff = -progrRatio;
                            }
                            else // TRansitioning between ungrounding and grounding
                            {
                                float leftMarginA = FindUngroundedProgr(progr, true, true);
                                float rightMarginA = FindUngroundedProgr(leftMargin, true);

                                float rightMarginB = FindUngroundedProgr(progr, false, true);
                                float leftMarginB = FindUngroundedProgr(rightMargin, false, false);

                                leftMargin = Mathf.Lerp(leftMarginA, leftMarginB, groundingBlendIn);
                                rightMargin = Mathf.Lerp(rightMarginA, rightMarginB, groundingBlendIn);
                            }

                        }

                        //UnityEngine.Debug.Log("L: " + leftMargin + "  R: " + rightMargin + " FadeToGr: " + _computeBlendInToGrounded);

                        #endregion

                        float targetSample = Mathf.Lerp(leftMargin, rightMargin, FootSnapToGrFrameOffset) + sampleOff;

                        FootDataAnalyze.RefreshInterpolationIndexes(targetSample);
                        int lowa = FootDataAnalyze.i_lowerIndex;
                        int hia = FootDataAnalyze.i_higherIndex;
                        FootDataAnalyze.RefreshInterpolationIndexes(targetSample + progrRatio);
                        int lowb = FootDataAnalyze.i_lowerIndex;
                        int hib = FootDataAnalyze.i_higherIndex;
                        FootDataAnalyze.RefreshInterpolationIndexes(targetSample - progrRatio);
                        int lowc = FootDataAnalyze.i_lowerIndex;
                        int hic = FootDataAnalyze.i_higherIndex;


                        int avgLow = Mathf.FloorToInt((lowa + lowb + lowc) / 3);
                        int avgHi = Mathf.CeilToInt((hia + hib + hic) / 3);
                        int avg = Mathf.RoundToInt(Mathf.Lerp(avgLow, avgHi, 0.5f));

                        LegAnimationClipMotion.MotionSample sample = FootDataAnalyze.sampledData[avg]; //FootDataAnalyze.GetSampleInProgress(targetSample);
                        //LegAnimationClipMotion.MotionSample sampleB = FootDataAnalyze.GetSampleInProgress(targetSample, true);
                        //sample.sampledAnkleRoot = Vector3.Lerp(sample.sampledAnkleRoot, sampleB.sampledAnkleRoot, 0.5f);

                        Vector3 targetGlueSnapPos = root.TransformPoint(sample.sampledAnkleRoot);

                        Animator an = root.GetAnimator();
                        if (an) if (an.applyRootMotion) { targetGlueSnapPos = Vector3.Scale(an.transform.lossyScale, sample.sampledAnkleRoot); }

                        //Debug.DrawRay(targetGlueSnapPos, Vector3.up, Color.yellow, 0.05f);
                        //Debug.DrawRay(targetGlueSnapPos - Vector3.forward * 0.5f, Vector3.forward, Color.yellow, 0.05f);
                        //Debug.DrawRay(targetGlueSnapPos - Vector3.right * 0.5f, Vector3.right, Color.yellow, 0.05f);

                        groundIKPos = Vector3.Lerp(groundIKPos, targetGlueSnapPos, footSnapToLatestGroundedBlend * groundingBlendIn);
                    }

                    #endregion


                    // Multiply offsets
                    if (IKWhenGroundedPosOffMul > 0f) groundIKPos = Vector3.LerpUnclamped(groundIKPos, groundIKPos + root.TransformVector(IKWhenGroundedPositionOffset), IKWhenGroundedPosOffMul * IKWhenGroundedPosOffEvaluate.Evaluate(progr));


                    // Stretch when grounded:


                    if (maxStr < 1.1f)
                    {
                        float stretch = processor.GetStretchValue(groundIKPos);

                        if (stretch > maxStr)
                        {
                            float len = (maxStr * processor.GetLimbLength());
                            groundIKPos = processor.StartIKBone.srcPosition + (groundIKPos - processor.StartIKBone.srcPosition).normalized * len;
                        }
                    }


                    newIKPos = Vector3.Lerp(newIKPos, groundIKPos, groundingBlendIn) + constOff;
                    newIKRot = Quaternion.Slerp(newIKRot, footRot, groundingBlendIn);
                }
                // Grounding blend in end

            }


            float FindUngroundedProgr(float from, bool checkToRight, bool findGrounded = false)
            {
                float step = 1f / clipFramerate;
                float boost = 8f;
                float direction = checkToRight ? 1f : -1f;

                for (int i = 1; i < clipFramerate * clipLength * boost; i++)
                {
                    float targetProgr = from + step * (i / boost) * direction;
                    if (targetProgr < 0f) { return 0f; }
                    if (targetProgr > 1f) { return 1f; }

                    bool grounded = FootDataAnalyze.GroundedIn(targetProgr);
                    if (grounded == findGrounded) return targetProgr;
                }


                return from;
            }


            float FindPreviousUngroundedProgr(float from, bool findGrounded, float? sampleSecs = null)
            {
                float blendDurInProgr = SecToProgr(FootGroundBlendDuration);

                if (sampleSecs != null) blendDurInProgr = SecToProgr(sampleSecs.Value);

                int toSampleCount = Mathf.CeilToInt(blendDurInProgr * clipFramerate);

                float progrStep = blendDurInProgr / (float)toSampleCount;
                float boostDensity = 32f;

                for (int i = 1; i <= toSampleCount * boostDensity * 256; i++)
                {
                    float progr = from + progrStep * ((float)i / boostDensity);
                    float cycProgr = progr;
                    if (progr > 1.5f) return -1f;
                    if (cycProgr > 1f) cycProgr -= 1f;

                    if (ComputeGroundedIn(cycProgr) == findGrounded)
                    {
                        return progr;
                    }
                }

                return -1f;
            }


            float FindNextUngroundedProgr(float from, bool findGrounded, float? sampleSecs = null)
            {
                float blendDurInProgr = SecToProgr(FootGroundBlendDuration);

                if (sampleSecs != null) blendDurInProgr = SecToProgr(sampleSecs.Value);

                int toSampleCount = Mathf.CeilToInt(blendDurInProgr * clipFramerate);

                float progrStep = blendDurInProgr / (float)toSampleCount;
                float boostDensity = 32f;

                for (int i = 1; i <= toSampleCount * boostDensity * 256; i++)
                {
                    float progr = from - progrStep * ((float)i / boostDensity);
                    float cycProgr = progr;
                    if (progr < -0.5f) return -1f;
                    if (cycProgr < 0f) cycProgr += 1f;
                    if (ComputeGroundedIn(cycProgr) == findGrounded)
                    {
                        return progr;
                    }
                }

                return -1f;
            }

            public Vector3 LastTargetIKPosition { get; private set; }
            internal Vector3 GetTargetIKPosition(Vector3 newIKPos, Transform root, float progr, FimpIK_Arm armIK, FIK_IKProcessor footIK)
            {

                Vector3 initIk = newIKPos;

                if (AlignTo.IsSet) if (AlignToBlend > 0f) newIKPos = Vector3.LerpUnclamped(newIKPos, AlignTo.Position, AlignToBlend);
                if (IKPosStillMul > 0f)
                {

                    if (UseMultiStillPoints == false)
                    {
                        Vector3 stillPos;
                        if (IKStillWorldPos) { if (root.parent == null) stillPos = IKStillPosition; else stillPos = root.parent.TransformPoint(IKStillPosition); }
                        else stillPos = root.TransformPoint(IKStillPosition);

                        newIKPos = Vector3.LerpUnclamped(newIKPos, stillPos, IKPosStillMul * IKPosStillEvaluate.Evaluate(progr));
                    }
                    else
                    {
                        newIKPos = ComputeMultiStillPosition(newIKPos, root, progr);
                    }

                }

                if (IKPosOffMul > 0f) newIKPos = Vector3.LerpUnclamped(newIKPos, newIKPos + root.TransformVector(IKPositionOffset), IKPosOffMul * IKPosOffEvaluate.Evaluate(progr));
                if (IKPosYOriginal > 0f) newIKPos.y = Mathf.LerpUnclamped(newIKPos.y, initIk.y + IKPositionOffset.y * IKPosOffMul * IKPosOffEvaluate.Evaluate(progr), IKPosYOriginal);



                if (IKType == EIKType.ArmIK)
                {
                    LastUsedProcessor = armIK;

                    if (refToMainSet != null)
                    {
                        newIKPos += refToMainSet.LatestAnimator.transform.TransformVector(refToMainSet.GetHipsOffset(progr) * (refToMainSet.OffsetHandsIKBlend * OffsetWithPelvisBlend));
                    }


                    #region Arm stretching limiting 

                    if (armIK != null)
                    {
                        float maxStr = MaxLegStretch * MaxLegStretchEvaluation.Evaluate(progr);

                        if (maxStr < 1.1f)
                        {
                            float stretch = armIK.GetStretchValue(newIKPos);

                            if (stretch > maxStr)
                            {
                                float len = (maxStr * armIK.limbLength);
                                newIKPos = armIK.UpperArmIKBone.transform.position + (newIKPos - armIK.UpperArmIKBone.transform.position).normalized * len;
                            }
                        }
                    }

                    #endregion

                }


                if (IKType == EIKType.FootIK)
                {
                    LastUsedProcessor = footIK;
                }
                else if (IKType == EIKType.ArmIK)
                {
                    LastUsedProcessor = armIK;
                }

                if (IKType == EIKType.FootIK && LegMode == ELegMode.BasicIK)
                    if (footIK != null)
                    {
                        LastUsedProcessor = footIK;

                        #region Leg stretching limiting when ungrounded

                        float maxStr = MaxLegStretch * MaxLegStretchEvaluation.Evaluate(progr);

                        if (maxStr < 1.1f)
                        {
                            float stretch = footIK.GetStretchValue(newIKPos);

                            if (stretch > maxStr)
                            {
                                float len = (maxStr * footIK.GetLimbLength());
                                newIKPos = footIK.StartIKBone.srcPosition + (newIKPos - footIK.StartIKBone.srcPosition).normalized * len;
                            }
                        }

                        #endregion
                    }


                LastTargetIKPosition = newIKPos;
                return newIKPos;
            }

            internal Quaternion GetTargetIKRotation(Quaternion newIKRot, Transform root, float progr)
            {
                Quaternion initIk = newIKRot;
                if (AlignTo.IsSet) if (AlignRotationToBlend > 0f) newIKRot = Quaternion.SlerpUnclamped(newIKRot, AlignTo.Rotation, AlignRotationToBlend);

                if (UseMultiStillPoints)
                {
                    newIKRot = ComputeMultiStillRotation(newIKRot, root, progr);
                }

                if (IKRotStillMul > 0f)
                {

                    Quaternion stillRot;
                    if (IKStillWorldRot) { stillRot = Quaternion.Euler(IKStillRotation); }
                    else stillRot = FEngineering.QToWorld(root.rotation, Quaternion.Euler(IKStillRotation));

                    newIKRot = Quaternion.SlerpUnclamped(newIKRot, stillRot, IKRotStillMul * IKRotStillEvaluate.Evaluate(progr));
                    //newIKRot = Quaternion.SlerpUnclamped(newIKRot, root.rotation * Quaternion.Euler(IKStillRotation), IKRotStillMul * IKRotStillEvaluate.Evaluate(progr));
                }

                if (IKRotOffMul > 0f) newIKRot = Quaternion.SlerpUnclamped(newIKRot, newIKRot * Quaternion.Euler(IKRotationOffset), IKRotOffMul * IKRotOffEvaluate.Evaluate(progr));

                return newIKRot;
            }


            #region Multi still points algorithms

            public Vector3 ComputeMultiStillPosition(Vector3 newIKPos, Transform root, float progr)
            {
                StillPoint targetPoint = null;

                for (int p = 0; p < MultiStillPoints.Count; p++)
                {
                    var point = MultiStillPoints[p];

                    if (progr > point.time - point.blendInDuration) // clip time is above left margin
                    {
                        if (progr < point.time + point.keepFor + point.blendOutDuration) // clip time is below right margin
                        {
                            // Point in time range
                            targetPoint = point;

                            if (targetPoint != null)
                            {
                                targetPoint.RefreshCurves();

                                float blendMul = IKPosStillMul * IKPosStillEvaluate.Evaluate(progr) * targetPoint.pointBlend * targetPoint.positionBlend;
                                float timeBlend = 0f;

                                Vector3 transitionOffset = Vector3.zero;
                                int blending = 0; // -1 is in 0 is full blend 1 is blending out

                                if (progr < targetPoint.time) // Blending IN
                                {
                                    blending = -1;
                                    timeBlend = Mathf.InverseLerp(targetPoint.time - targetPoint.blendInDuration, targetPoint.time, progr);

                                    float transitionAmount = targetPoint.FootTransitionInOffsetCurve.Evaluate(timeBlend);
                                    if (transitionAmount > 0.0001f)
                                    {
                                        transitionOffset = targetPoint.FootTransitionOffset * transitionAmount;
                                    }

                                    float pelvisTrAmount = targetPoint.HipsTransitionInOffsetCurve.Evaluate(timeBlend);
                                    if (pelvisTrAmount != 0f)
                                    {
                                        refToMainSet.ApplyHipsOffset(pelvisTrAmount * targetPoint.HipsTransitionPositionOffset);
                                    }
                                }
                                else
                                {
                                    if (progr < targetPoint.time + targetPoint.keepFor) // Inside full blend range
                                    {
                                        timeBlend = 1f;
                                    }
                                    else // Blending OUT
                                    {
                                        blending = 1;
                                        timeBlend = 1f - Mathf.InverseLerp(targetPoint.time + targetPoint.keepFor, targetPoint.time + targetPoint.keepFor + targetPoint.blendOutDuration, progr);

                                        float transitionAmount = targetPoint.FootTransitionOutOffsetCurve.Evaluate(1f - timeBlend);
                                        if (transitionAmount > 0.0001f)
                                        {
                                            transitionOffset = targetPoint.FootTransitionOffset * transitionAmount;
                                        }

                                        float pelvisTrAmount = targetPoint.HipsTransitionOutOffsetCurve.Evaluate(1f - timeBlend);
                                        if (pelvisTrAmount != 0f)
                                        {
                                            if (targetPoint.HipsOffsetMode != StillPoint.EOffsetMode.OutOffset)
                                                refToMainSet.ApplyHipsOffset(pelvisTrAmount * targetPoint.HipsTransitionPositionOffset);
                                            else
                                                refToMainSet.ApplyHipsOffset(pelvisTrAmount * targetPoint.HipsTransitionOutPositionOffset);
                                        }
                                    }
                                }


                                Vector3 stillPos;
                                if (IKStillWorldPos) { if (root.parent == null) stillPos = targetPoint.pos; else stillPos = root.parent.TransformPoint(targetPoint.pos); }
                                else stillPos = root.TransformPoint(targetPoint.pos);

                                //if (IKStillWorldPos) { if (root.parent == null) stillPos = targetPoint.pos + transitionOffset; else stillPos = root.parent.TransformPoint(targetPoint.pos + transitionOffset); }
                                //else stillPos = root.TransformPoint(targetPoint.pos + transitionOffset);

                                if (transitionOffset != Vector3.zero)
                                {
                                    if (IKStillWorldPos) { if (root.parent != null) transitionOffset = root.parent.TransformDirection(transitionOffset); }
                                    else transitionOffset = root.TransformDirection(transitionOffset);
                                }

                                float progressBlend;
                                if (blending == 0 || targetPoint.FootTransitionInProgressCurve == null || targetPoint.FootTransitionInProgressCurve.keys.Length < 2)
                                { progressBlend = timeBlend * blendMul; }
                                else if (blending == -1)
                                { progressBlend = targetPoint.FootTransitionInProgressCurve.Evaluate(timeBlend) * blendMul; }
                                else //if (blending == 1)
                                { progressBlend = targetPoint.FootTransitionOutProgressCurve.Evaluate(timeBlend) * blendMul; }


                                newIKPos = Vector3.LerpUnclamped(newIKPos, stillPos + transitionOffset, progressBlend);

                            }

                            //break;
                        }
                    }
                }

                return newIKPos;
            }


            public Quaternion ComputeMultiStillRotation(Quaternion newIKRot, Transform root, float progr)
            {
                StillPoint targetPoint = null;

                for (int p = 0; p < MultiStillPoints.Count; p++)
                {
                    var point = MultiStillPoints[p];

                    if (progr > point.time - point.blendInDuration) // clip time is above left margin
                    {
                        if (progr < point.time + point.keepFor + point.blendOutDuration) // clip time is below right margin
                        {
                            // Point in time range
                            targetPoint = point;
                            int blending = 0;

                            Quaternion transitionOffset = Quaternion.identity;
                            bool transitionRotIsZero = targetPoint.FootTransitionOffset == Vector3.zero;

                            if (targetPoint != null)
                            {
                                blending = -1;
                                float blendMul = IKPosStillMul * IKPosStillEvaluate.Evaluate(progr) * targetPoint.pointBlend * targetPoint.rotationBlend;
                                float timeBlend = 0f;

                                if (progr < targetPoint.time)
                                {
                                    // Blending In
                                    timeBlend = Mathf.InverseLerp(targetPoint.time - targetPoint.blendInDuration, targetPoint.time, progr);

                                    if (!transitionRotIsZero)
                                    {
                                        float transitionAmount = targetPoint.FootRotTransitionInOffsetCurve.Evaluate(timeBlend);
                                        if (transitionAmount != 0f) transitionOffset = Quaternion.Euler(targetPoint.FootTransitionRotationOffset * transitionAmount);
                                    }
                                }
                                else
                                {
                                    if (progr < targetPoint.time + targetPoint.keepFor)
                                    { timeBlend = 1f; }
                                    else // Blending Out
                                    {
                                        blending = 1;
                                        timeBlend = 1f - Mathf.InverseLerp(targetPoint.time + targetPoint.keepFor, targetPoint.time + targetPoint.keepFor + targetPoint.blendOutDuration, progr);

                                        if (!transitionRotIsZero)
                                        {
                                            float transitionAmount = targetPoint.FootRotTransitionOutOffsetCurve.Evaluate(1f - timeBlend);
                                            if (transitionAmount != 0f) transitionOffset = Quaternion.Euler(targetPoint.FootTransitionRotationOffset * transitionAmount);
                                        }
                                    }
                                }

                                Quaternion stillRot;
                                if (IKStillWorldRot) stillRot = Quaternion.Euler(targetPoint.rot);
                                else stillRot = root.rotation * Quaternion.Euler(targetPoint.rot);

                                float progressBlend;

                                if (blending == 0 || targetPoint.FootTransitionInProgressCurve == null || targetPoint.FootTransitionInProgressCurve.keys.Length < 2)
                                { progressBlend = timeBlend * blendMul; }
                                else if (blending == -1)
                                { progressBlend = targetPoint.FootTransitionInProgressCurve.Evaluate(timeBlend) * blendMul; }
                                else //if (blending == 1)
                                { progressBlend = targetPoint.FootTransitionOutProgressCurve.Evaluate(timeBlend) * blendMul; }

                                newIKRot = Quaternion.SlerpUnclamped(newIKRot, stillRot * transitionOffset, progressBlend);
                            }



                            //break;
                        }
                    }
                }



                return newIKRot;
            }

            #endregion


            #region Foot Grounding Analyze


            [NonSerialized] public Vector3[] _b_ground = new Vector3[2];
            [NonSerialized] public Vector3[] _b_groundX = new Vector3[2];
            [NonSerialized] public Vector3[] _b_foot = new Vector3[4];

            //[NonSerialized] public Vector3[] _b_hf_detect = new Vector3[5];
            //[NonSerialized] public Vector3[] _b_heel = new Vector3[2];
            //[NonSerialized] public Vector3[] _b_hold = new Vector3[2];

            public void RefreshGizmosBuffers()
            {
                if (_b_foot == null) _b_foot = new Vector3[4];
                if (_b_ground == null) _b_ground = new Vector3[2];
                if (_b_groundX == null) _b_groundX = new Vector3[2];
                //if (_b_hf_detect == null) _b_hf_detect = new Vector3[5];
                //if (_b_heel == null) _b_heel = new Vector3[2];
                //if (_b_hold == null) _b_hold = new Vector3[2];
            }


            public LegAnimationClipMotion FootDataAnalyze = null;

            private void SyncSettingsIfSwitched(AnimationDesignerSave save, ADArmatureLimb limb, ADClipSettings_IK ikSets)
            {
                if (syncGrLegIKSettingsToOtherLegs)
                {
                    for (int i = 0; i < save.Limbs.Count; i++)
                    {
                        if (save.Limbs[i] == limb) continue;
                        var set = ikSets.GetIKSettingsForLimb(save.Limbs[i], save);
                        if (set == null) continue;
                        if (set.IsLegGroundingMode == false) continue;
                        set.syncGrLegIKSettingsToOtherLegs = false;
                        PasteGroundingValuesTo(this, set);
                    }
                }
            }


            private void GenerateFootAnalyzeData(AnimationDesignerSave save, ADArmatureLimb limb, AnimationClip clip, ADClipSettings_IK ikSets, ADClipSettings_Main main, float footTreshold, float toesTreshold, float holdOnGround, bool removeRootMotion)
            {
                GetClipFootAnalyze(save, limb, clip, main, footTreshold, toesTreshold, holdOnGround, removeRootMotion);

                if (syncGrLegIKSettingsToOtherLegs)
                {
                    for (int i = 0; i < save.Limbs.Count; i++)
                    {
                        if (save.Limbs[i] == limb) continue;
                        var set = ikSets.GetIKSettingsForLimb(save.Limbs[i], save);
                        if (set == null) continue;
                        if (set.IsLegGroundingMode == false) continue;
                        set.GetClipFootAnalyze(save, save.Limbs[i], clip, main, footTreshold, toesTreshold, holdOnGround, removeRootMotion);
                    }
                }
            }

            /// <summary> Character must be in T-Pose when calling this method! </summary>
            private void GetClipFootAnalyze(AnimationDesignerSave save, ADArmatureLimb limb, AnimationClip clip, ADClipSettings_Main main, float heelTreshold, float toesTreshold, float holdOnGround, bool removeRootMotion)
            {
                AnimationDesignerWindow.ForceTPose();

                FootDataAnalyze = new LegAnimationClipMotion(clip);

                FootDataAnalyze.AnalyzeClip(save.LatestAnimator.transform, save.ReferencePelvis.transform, limb.Bones[0].T, limb.Bones[1].T, limb.Bones[2].T, null,
                    save.LatestAnimator, Mathf.RoundToInt(clip.frameRate * clip.length),
                    heelTreshold, toesTreshold, holdOnGround,
                    true, removeRootMotion, FloorGroundedOffset, FloorGroundingAnalyzeMode, Mathf.RoundToInt(main.GetStartFrame(false)), Mathf.RoundToInt(main.GetClipFramesCount(false) - main.GetEndFrame(false) + 1));

            }


            public Vector3 GetAnalysisTreshHeelPoint(Transform foot, float heelTresh)
            {
                return FootDataAnalyze.GetSamplingHeelPoint(foot, heelTresh);
            }

            public Vector3 GetAnalysisTreshToesPoint(Transform foot, float toesTresh)
            {
                return FootDataAnalyze.GetSamplingToesPoint(foot, toesTresh);
            }

            public Vector3 GetAnalysisHeelPoint(Transform foot)
            {
                return foot.position + foot.TransformVector(FootDataAnalyze._footLocalToGround);
            }

            public Vector3 GetAnalysisToesPoint(Transform foot, Vector3 heelPoint)
            {
                return heelPoint + foot.TransformVector(FootDataAnalyze._footToToesForw);
            }



            internal void DrawFootGroundingAnalyzeGizmos(float animationProgress, ADArmatureLimb limb, float alphaMul = 1f)
            {
                RefreshGizmosBuffers();

                bool grounded = ComputeGroundedIn(animationProgress);

                if (grounded)
                    Handles.color = new Color(0.1f, 1f, 0.1f, alphaMul);
                else
                    Handles.color = new Color(1f, 0.7f, 0.1f, 0.4f * alphaMul);

                Vector3 footP = GetAnalysisHeelPoint(limb.LastBone.T);
                _b_foot[0] = limb.LastBone.pos;
                _b_foot[1] = footP;
                _b_foot[2] = GetAnalysisToesPoint(limb.LastBone.T, footP);
                _b_foot[3] = limb.LastBone.pos;

                Handles.DrawAAPolyLine(grounded ? 5f : 2.5f, _b_foot);

                if (refToMainSet != null)
                    if (refToMainSet.LatestAnimator != null)
                        if (LegGrounding == ELegGroundingView.Processing)
                        {
                            Vector3 floorOff = Vector3.up * refToMainSet.IKGroundLevel;
                            Vector3 cross = Vector3.Cross(refToMainSet.LatestAnimator.transform.forward, refToMainSet.IKGroundNormal).normalized;
                            Vector3 cross2 = Vector3.Cross(refToMainSet.LatestAnimator.transform.right, refToMainSet.IKGroundNormal).normalized;

                            _b_ground[0] = floorOff + (-cross);
                            _b_ground[1] = floorOff + (cross);

                            _b_groundX[0] = floorOff + (-cross2);
                            _b_groundX[1] = floorOff + (cross2);

                            Handles.matrix = refToMainSet.LatestAnimator.transform.localToWorldMatrix;
                            Handles.color = new Color(0.7f, 0.4f, 0.1f, alphaMul);
                            Handles.DrawAAPolyLine(3f, _b_ground);
                            Handles.DrawAAPolyLine(3f, _b_groundX);
                            Handles.matrix = Matrix4x4.identity;
                        }

            }

            internal void PrepareFor(ADArmatureLimb selectedLimb)
            {
                if (selectedLimb == null) return;

                if (selectedLimb.LimbType == ADArmatureLimb.ELimbType.Arm)
                {
                    if (selectedLimb.Bones.Count == 4) IKType = EIKType.ArmIK;
                }
                else if (selectedLimb.LimbType == ADArmatureLimb.ELimbType.Leg)
                {
                    if (selectedLimb.Bones.Count == 3) IKType = EIKType.FootIK;
                }
                else if (selectedLimb.LimbType == ADArmatureLimb.ELimbType.Spine)
                {
                    IKType = EIKType.ChainIK;
                }
                else if (selectedLimb.Bones.Count == 3) IKType = EIKType.FootIK;
                else if (selectedLimb.Bones.Count == 4) IKType = EIKType.ArmIK;
                else IKType = EIKType.ChainIK;
            }


            #endregion



            #region Animation Clip Additional Data Generating



            internal void GenerateGroundingEventsFor(AnimationClip newGeneratedClip, List<AnimationEvent> aEvents)
            {
                if (string.IsNullOrEmpty(GenerateFootStepEvents)) return;
                if (newGeneratedClip.frameRate < 2) return;
                if (newGeneratedClip.length < 0.01f) return;
                if (FootDataAnalyze == null) return;

                int iters = 0;
                float elapsed = 0f;
                bool? wasGrounded = null;
                while (elapsed < newGeneratedClip.length)
                {
                    elapsed += 0.01f; // 0.01sec 

                    float progr = elapsed / newGeneratedClip.length;
                    bool isGrounded = ComputeGroundedIn(progr);

                    if (wasGrounded == false)
                    {
                        if (isGrounded)
                        {
                            wasGrounded = true;
                            float eventTime = elapsed + GeneratedStepEventsTimeOffset;
                            if (eventTime < 0f) eventTime = 0f;
                            if (eventTime >= newGeneratedClip.length) eventTime = newGeneratedClip.length - 0.001f;

                            AnimationEvent ev = new AnimationEvent();
                            ev.functionName = GenerateFootStepEvents;
                            ev.time = eventTime;
                            aEvents.Add(ev);
                        }
                    }
                    else
                    {
                        if (isGrounded == false) wasGrounded = false;
                    }

                    iters += 1;
                    if (iters > 10000) break;
                }


            }

            #endregion


        }


        #endregion



    }
}