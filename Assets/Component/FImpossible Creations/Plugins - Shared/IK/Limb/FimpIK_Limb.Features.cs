using System;
using UnityEngine;

namespace FIMSpace.FTools
{
    public partial class FimpIK_Limb : FIK_ProcessorBase
    {
        // Foot/End Bone rotation helper with root reference
        public Quaternion EndBoneMapping { get; protected set; }
        public IKBone FeetIKBone { get { return IKBones[3]; } }


        /// <summary> Assigning helpful reference to main root transform of body to help IK rotations </summary>
        public virtual void SetRootReference(Transform mainParentTransform)
        {
            Root = mainParentTransform;
            EndBoneMapping = Quaternion.FromToRotation(EndIKBone.right, Vector3.right);
            EndBoneMapping *= Quaternion.FromToRotation(EndIKBone.up, Vector3.up);
            if (mainParentTransform) hasRoot = true;
        }


        /// <summary> Reference scale for computations - active length from start bone to middle knee </summary>
        public float ScaleReference { get; protected set; }

        public void RefreshLength()
        {
            ScaleReference = (StartIKBone.transform.position - MiddleIKBone.transform.position).magnitude;
        }

        public void RefreshScaleReference()
        {
            ScaleReference = (StartIKBone.transform.position - MiddleIKBone.transform.position).magnitude;
        }

        float GetCurrentLegToAnkleLength()
        {
            float fullLength = Mathf.Epsilon;
            fullLength += (StartIKBone.transform.position - MiddleIKBone.transform.position).magnitude;
            fullLength += (MiddleIKBone.transform.position - EndIKBone.transform.position).magnitude;
            return fullLength;
        }

        /// <summary> Returning >= 1f when max range for IK point is reached </summary>
        public float GetStretchValue(Vector3 targetPos)
        {
            float toGoal = (StartIKBone.transform.position - targetPos).magnitude;
            return toGoal / GetCurrentLegToAnkleLength();
        }

        public Vector3 GetNotStretchedPositionTowards(Vector3 targetPos, float maxStretch)
        {
            Vector3 toGoal = (targetPos - StartIKBone.transform.position);
            return StartIKBone.transform.position + toGoal.normalized * (GetCurrentLegToAnkleLength() * maxStretch);
        }

        public void ApplyMaxStretchingPreprocessing(float maxStretch, float allowIKRotationFadeout = 2f)
        {
            if (maxStretch < 1.1f)
            {

                float toGoal = (StartIKBone.transform.position - IKTargetPosition).magnitude;
                float limbUnitLength = GetCurrentLegToAnkleLength();
                float stretch = toGoal / limbUnitLength;

                if (stretch > maxStretch)
                {

                    if (hasFeet && FeetStretchWeight > 0f)
                    {

                        #region Feet stretch helper

                        if (maxFeetAngle > 0f)
                        {
                            // Feet angle factor helpers
                            Vector3 thighToTarget = IKTargetPosition - StartIKBone.transform.position;
                            thighToTarget.Normalize();
                            Vector3 ankleToFeet = FeetIKBone.transform.position - EndIKBone.transform.position;
                            ankleToFeet.Normalize();

                            float feetDot = Vector3.Dot(thighToTarget, ankleToFeet);
                            feetDot = Mathf.Clamp01(feetDot);

                            // Feet bone rotation helpers
                            float feetLength = (FeetIKBone.transform.position - EndIKBone.transform.position).magnitude;
                            float stretchDiff = toGoal - limbUnitLength * Mathf.Min(maxStretch, 1f);
                            stretchDiff /= (feetLength * FeetFadeQuicker);
                            float stretchDiff2 = stretchDiff;
                            stretchDiff *= maxFeetAngleFactor * FeetStretchSensitivity;
                            if (stretchDiff > 1f) stretchDiff = 1f;

                            if (stretchDiff2 < 1f) stretchDiff2 = 1f; else { if (stretchDiff2 > 2f) stretchDiff2 = 2f; stretchDiff2 -= 1f; stretchDiff2 *= stretchDiff2; stretchDiff2 = 1f - stretchDiff2; }

                            // Apply
                            float heelFactor = Mathf.Min(FeetStretchLimit, (1f - feetDot) * (90f / maxFeetAngle) * stretchDiff * FeetStretchWeight);

                            if (stretch > 1.09f)
                            {
                                stretchDiff2 *= 1f - Mathf.InverseLerp(1.09f, 1.23f, stretch);
                            }

                            if (heelFactor != 0f) OffsetHeel(heelFactor, stretchDiff2);

                            // Recompute
                            toGoal = (StartIKBone.transform.position - IKTargetPosition).magnitude;
                            stretch = toGoal / limbUnitLength;
                        }

                        #endregion

                        if (stretch > maxStretch)
                        {
                            float len = (maxStretch * limbUnitLength);
                            IKTargetPosition = StartIKBone.transform.position + (IKTargetPosition - StartIKBone.transform.position).normalized * len;
                        }

                    }
                    else
                    {
                        float len = (maxStretch * limbUnitLength);
                        IKTargetPosition = StartIKBone.transform.position + (IKTargetPosition - StartIKBone.transform.position).normalized * len;
                    }

                    if (allowIKRotationFadeout > 0f)
                    {
                        float stretchDiff = stretch - maxStretch;
                        stretchDiff = Mathf.Clamp01(stretchDiff * allowIKRotationFadeout);
                        internalRotationWeightMul = (1f - stretchDiff);
                    }
                }
                else
                {
                    internalRotationWeightMul = 1f;
                }
            }
        }


        #region Prepare Feet

        [NonSerialized] public float FeetStretchWeight = 1f;
        [NonSerialized] public float FeetStretchSensitivity = 1f;
        [NonSerialized] public float FeetStretchLimit = 1f;
        [NonSerialized] public float FeetFadeQuicker = 1f;
        [NonSerialized] public bool disableFeet = false;
        float maxFeetAngle = 0f;
        float maxFeetAngleFactor = 0f;
        Vector3 ankleToFeet;
        void PrepareFeet()
        {
            Vector3 kneeToAnkle = EndIKBone.transform.position - MiddleIKBone.transform.position;
            kneeToAnkle.Normalize();

            ankleToFeet = FeetIKBone.transform.position - EndIKBone.transform.position;
            ankleToFeet.Normalize();

            maxFeetAngle = Vector3.Angle(ankleToFeet, kneeToAnkle);
            maxFeetAngleFactor = 90f / maxFeetAngle;

            //if ( Root == null)
            //{
            //    UnityEngine.Debug.Log("[IK] Feet requires Root Transform defined!");
            //    hasFeet = false;
            //}
        }

        #endregion


        internal void OffsetHeel(float heelRot, float feetCompensate = 1f)
        {
            if (hasFeet == false) return;
            if (disableFeet) return;

            Quaternion preAnkleRot = IKTargetRotation;
            Vector3 toFeet = (FeetIKBone.transform.position - EndIKBone.transform.position);

            Vector3 rotatedOffset = Quaternion.Inverse(preAnkleRot) * (toFeet);

            Vector3 rightAxis;
            if (UseEndBoneMapping) rightAxis = IKTargetRotation * Vector3.right;
            else rightAxis = IKTargetRotation * (EndIKBone.right);// Root.right;

            Quaternion rotationOffset = Quaternion.AngleAxis(heelRot * maxFeetAngle, rightAxis);
            Quaternion newAnkleRot = rotationOffset * preAnkleRot;

            IKTargetRotation = newAnkleRot;

            Vector3 newOffset = newAnkleRot * rotatedOffset;
            rotatedOffset = newOffset - toFeet;

            if (feetCompensate > 0f)
            {
                Quaternion newFeetRot = Quaternion.Inverse(rotationOffset) * (FeetIKBone.transform.rotation);

                if (feetCompensate >= 1f)
                    FeetIKBone.transform.rotation = newFeetRot;
                else
                    FeetIKBone.transform.rotation = Quaternion.Lerp(FeetIKBone.transform.rotation, newFeetRot, feetCompensate);
            }

            IKTargetPosition -= rotatedOffset;
        }


    }
}
