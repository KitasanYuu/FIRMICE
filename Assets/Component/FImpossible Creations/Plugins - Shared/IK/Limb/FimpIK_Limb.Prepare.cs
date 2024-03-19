using System;
using UnityEngine;

namespace FIMSpace.FTools
{
    public partial class FimpIK_Limb : FIK_ProcessorBase
    {
        [HideInInspector][Range(0f, 1f)] public float ManualHintPositionWeight = 0f;
        [HideInInspector] public Vector3 IKManualHintPosition = Vector3.zero;

        protected virtual void Refresh()
        {
            RefreshAnimatorCoords();

            // If limb have more than 3 point bones then we must update some data for main two bones
            if (!everyIsChild)
            {
                //StartIKBone.RefreshOrientations(MiddleIKBone.transform.position, targetElbowNormal);
                MiddleIKBone.RefreshOrientations(EndIKBone.transform.position, targetElbowNormal);
            }
        }

        [NonSerialized] public bool UseEndBoneMapping = true;
        float internalRotationWeightMul = 1f;

        protected virtual void EndBoneRotation()
        {
            float rotWeight = FootRotationWeight * IKWeight * internalRotationWeightMul;

            if (rotWeight > 0f)
            {
                if (UseEndBoneMapping)
                {
                    if (rotWeight < 1f)
                        EndIKBone.transform.rotation = Quaternion.SlerpUnclamped(postIKAnimatorEndBoneRot, IKTargetRotation * EndBoneMapping, rotWeight);
                    else
                        EndIKBone.transform.rotation = IKTargetRotation * EndBoneMapping;
                }
                else
                {
                    if (rotWeight < 1f)
                        EndIKBone.transform.rotation = Quaternion.SlerpUnclamped(postIKAnimatorEndBoneRot, IKTargetRotation, rotWeight);
                    else
                        EndIKBone.transform.rotation = IKTargetRotation;
                }
            }

            lateEndBoneRotation = EndIKBone.transform.rotation;
        }

        public override void PreCalibrate()
        {
            base.PreCalibrate();
            RefreshScaleReference();
        }

        public void RefreshAnimatorCoords()
        {
            StartIKBone.CaptureSourceAnimation();
            MiddleIKBone.CaptureSourceAnimation();
            EndIKBone.CaptureSourceAnimation();
            if (!everyIsChild) { if (MiddleIKBone != EndParentIKBone) EndParentIKBone.CaptureSourceAnimation(); }
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
            return Vector3.Cross(targetElbowPos - StartIKBone.transform.position, EndIKBone.transform.position - StartIKBone.transform.position);
        }


        public void RefreshDefaultFlexNormal()
        {
            Vector3 normal = Vector3.Cross(MiddleIKBone.transform.position - StartIKBone.transform.position, EndIKBone.transform.position - MiddleIKBone.transform.position);
            if (normal != Vector3.zero) targetElbowNormal = normal;
        }


        private Vector3 GetOrientationDirection(Vector3 ikPosition, Vector3 orientationNormal)
        {
            Vector3 direction = ikPosition - StartIKBone.transform.position; // From start bone to target ik position
            if (direction == Vector3.zero) return Vector3.zero;

            float distSqrStartToGoal = direction.sqrMagnitude; // Computing length for bones
            float distStartToGoal = Mathf.Sqrt(distSqrStartToGoal);

            float forwardLen = (distSqrStartToGoal + StartIKBone.sqrMagn - MiddleIKBone.sqrMagn) / 2f / distStartToGoal;
            float upLen = Mathf.Sqrt(Mathf.Clamp(StartIKBone.sqrMagn - forwardLen * forwardLen, 0, Mathf.Infinity));

            Vector3 perpendicularUp = Vector3.Cross(direction / distStartToGoal, orientationNormal);
     
            return Quaternion.LookRotation(direction, perpendicularUp) * new Vector3(0f, upLen, forwardLen);
        }


    }
}
