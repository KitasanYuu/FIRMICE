using System;
using UnityEngine;

namespace FIMSpace.FTools
{
    public partial class FimpIK_Arm
    {
        /*[HideInInspector] */
        [Range(0f, 1f)] public float ManualHintPositionWeight = 0f;
        [HideInInspector] public Vector3 IKManualHintPosition = Vector3.zero;

        protected virtual void Refresh()
        {
            RefreshAnimatorCoords();

            // If limb have more than 3 point bones then we must update some data for main two bones
            if (!everyIsChild)
            {
                UpperArmIKBone.RefreshOrientations(ForeArmIKBone.transform.position, TargetElbowNormal);
                ForeArmIKBone.RefreshOrientations(HandIKBone.transform.position, TargetElbowNormal);
            }
        }


        protected virtual void HandBoneRotation()
        {
            float rotWeight = HandRotationWeight * IKWeight * _internalIKWeight;

            if (rotWeight > 0f)
            {
                if (rotWeight < 1f)
                    HandIKBone.transform.rotation = Quaternion.LerpUnclamped(HandIKBone.transform.rotation, IKTargetRotation, rotWeight);
                else
                    HandIKBone.transform.rotation = IKTargetRotation;
            }
        }


        public void RefreshAnimatorCoords()
        {
            if (ShoulderIKBone != null) ShoulderIKBone.CaptureSourceAnimation();
            UpperArmIKBone.CaptureSourceAnimation();
            ForeArmIKBone.CaptureSourceAnimation();
            HandIKBone.CaptureSourceAnimation();
        }


        private Vector3 GetDefaultFlexNormal()
        {
            if (ManualHintPositionWeight > 0f)
            {
                if (ManualHintPositionWeight >= 1f)
                    return CalculateElbowNormalToPosition(IKManualHintPosition);
                else
                    return Vector3.LerpUnclamped(GetAutomaticFlexNormal().normalized, CalculateElbowNormalToPosition(IKManualHintPosition), ManualHintPositionWeight);
            }
            else
                return GetAutomaticFlexNormal();
        }


        public Vector3 CalculateElbowNormalToPosition(Vector3 targetElbowPos)
        {
            return Vector3.Cross(targetElbowPos - UpperArmIKBone.transform.position, HandIKBone.transform.position - UpperArmIKBone.transform.position);
        }


        public void RefreshDefaultFlexNormal()
        {
            Vector3 normal = Vector3.Cross(ForeArmIKBone.transform.position - UpperArmIKBone.transform.position, HandIKBone.transform.position - ForeArmIKBone.transform.position);
            if (normal != Vector3.zero) TargetElbowNormal = normal;
        }


        private Vector3 GetOrientationDirection(Vector3 ikPosition, Vector3 orientationNormal)
        {
            Vector3 direction = ikPosition - UpperArmIKBone.transform.position; // From start bone to target ik position
            if (direction == Vector3.zero) return Vector3.zero;

            float distSqrStartToGoal = direction.sqrMagnitude; // Computing length for bones
            float distStartToGoal = Mathf.Sqrt(distSqrStartToGoal);

            float forwardLen = (distSqrStartToGoal + UpperArmIKBone.sqrMagn - ForeArmIKBone.sqrMagn) / 2f / distStartToGoal;
            float upLen = Mathf.Sqrt(Mathf.Clamp(UpperArmIKBone.sqrMagn - forwardLen * forwardLen, 0, Mathf.Infinity));

            Vector3 perpendicularUp = Vector3.Cross(direction / distStartToGoal, orientationNormal);
            return Quaternion.LookRotation(direction, perpendicularUp) * new Vector3(0f, upLen, forwardLen);
        }

        private float sd_targetIKRotation = 0f;
        public void IKHandRotationWeightFadeTo(float to, float duration, float delta)
        {
            HandRotationWeight = Mathf.SmoothDamp(HandRotationWeight, to, ref sd_targetIKRotation, duration, Mathf.Infinity, delta);
        }

        private float sd_positionWeight = 0f;

        public bool IsCorrect
        {
            get
            {
                if (Initialized == false) return false;
                if (UpperarmTransform == null) return false;
                if (LowerarmTransform == null) return false;
                if (shoulderRotate == null) return false;
                if (shoulderRotate.transform == null) return false;
                return true;
            }
        }

        public void IKHandPositionWeightFadeTo(float to, float duration, float delta)
        {
            IKPositionWeight = Mathf.SmoothDamp(IKPositionWeight, to, ref sd_positionWeight, duration, Mathf.Infinity, delta);
        }

        /// <summary>
        /// IK position offsetted with hand middle position
        /// </summary>
        public Vector3 GetMiddleHandPosition(Vector3 tgt)
        {
            Matrix4x4 mx = Matrix4x4.TRS(IKTargetPosition, IKTargetRotation, HandIKBone.transform.lossyScale);
            return tgt - mx.MultiplyVector(HandMiddleOffset);
        }

        public Vector3 GetLimitedIKPosToMax(Vector3 targetIKPos, float lengthFactor = 1f)
        {
            Vector3 dir = targetIKPos - UpperArmIKBone.transform.position;
            return UpperArmIKBone.transform.position + dir.normalized * lengthFactor * limbLength;
        }
    }
}
