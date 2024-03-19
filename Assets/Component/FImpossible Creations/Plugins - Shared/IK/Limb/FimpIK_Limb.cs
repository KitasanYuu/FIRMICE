using System;
using UnityEngine;

namespace FIMSpace.FTools
{
    // TODO -> Limiting, Weights, Goal Modes

    /// <summary>
    /// FC: Class for processing IK logics for 3-bones inverse kinematics
    /// </summary>
    [System.Serializable]
    public partial class FimpIK_Limb : FIK_ProcessorBase
    {
        [NonSerialized][Tooltip("3-Bones limb array")] private IKBone[] IKBones;
        [Tooltip("Blend value for goal position")][Space(4)][Range(0f, 1f)] public float IKPositionWeight = 1f;
        [Tooltip("Blend value for end bone rotation")][Range(0f, 1f)] public float FootRotationWeight = 1f;
        [Tooltip("Flex style algorithm for different limbs")] public FIK_HintMode AutoHintMode = FIK_HintMode.MiddleForward;


        private Vector3 targetElbowNormal = Vector3.right;
        private Quaternion lateEndBoneRotation;
        private Quaternion postIKAnimatorEndBoneRot;

        /// <summary> For custom slight adjustements of the IK knee/elbow hints </summary>
        public Vector3 ExtraHintAdjustementOffset = Vector3.zero;
        /// <summary> Inverse direction of default calculated hint position </summary>
        public bool InverseHint = false;

        /// <summary> Updating processor with 3-bones oriented inverse kinematics </summary>
        public override void Update()
        {
            if (!Initialized) return;

            Refresh();

            // Foot IK Position ---------------------------------------------------

            float posWeight = IKPositionWeight * IKWeight;
            StartIKBone.sqrMagn = (MiddleIKBone.transform.position - StartIKBone.transform.position).sqrMagnitude;
            MiddleIKBone.sqrMagn = (EndIKBone.transform.position - MiddleIKBone.transform.position).sqrMagnitude;

            targetElbowNormal = GetDefaultFlexNormal();
            if (ExtraHintAdjustementOffset != Vector3.zero)
            {
                targetElbowNormal = Vector3.Lerp( targetElbowNormal, CalculateElbowNormalToPosition(EndIKBone.transform.position + EndIKBone.transform.rotation * ExtraHintAdjustementOffset), ExtraHintAdjustementOffset.magnitude).normalized;
            }

            Vector3 orientationDirection = GetOrientationDirection(IKTargetPosition, InverseHint ? -targetElbowNormal : targetElbowNormal);
            if (orientationDirection == Vector3.zero) orientationDirection = MiddleIKBone.transform.position - StartIKBone.transform.position;

            if (posWeight > 0f)
            {
                Quaternion sBoneRot = StartIKBone.GetRotation(orientationDirection, targetElbowNormal) * StartBoneRotationOffset;
                if (posWeight < 1f) sBoneRot = Quaternion.LerpUnclamped(StartIKBone.srcRotation, sBoneRot, posWeight);
                StartIKBone.transform.rotation = sBoneRot;

                Quaternion sMidBoneRot = MiddleIKBone.GetRotation(IKTargetPosition - MiddleIKBone.transform.position, MiddleIKBone.GetCurrentOrientationNormal());
                if (posWeight < 1f) sMidBoneRot = Quaternion.LerpUnclamped(MiddleIKBone.srcRotation, sMidBoneRot, posWeight);
                MiddleIKBone.transform.rotation = sMidBoneRot;
            }

            postIKAnimatorEndBoneRot = EndIKBone.transform.rotation;

            EndBoneRotation();
        }


        /// <summary>
        /// Calculating IK pole position normal for desired flexing bend
        /// </summary>
        private Vector3 GetAutomaticFlexNormal()
        {
            Vector3 bendNormal = StartIKBone.GetCurrentOrientationNormal();


            switch (AutoHintMode)
            {
                case FIK_HintMode.Leg:
                    Vector3 offsets = IKTargetRotation * (EndIKBone.forward * internalRotationWeightMul * 2f);

                    if (hasRoot)
                    {
                        offsets += Root.forward * 0.06f;
                        Vector3 toGoal = Root.InverseTransformPoint( IKTargetPosition);
                        toGoal.y = 0f;
                        offsets += (Root.TransformPoint(toGoal) - Root.position) * 0.025f;
                    }

                    float refScale = Vector3.Distance(MiddleIKBone.transform.position, EndIKBone.transform.position) * 0.1f;
                    Vector3 legHint = CalculateElbowNormalToPosition(MiddleIKBone.srcPosition + offsets * refScale);
                    return Vector3.LerpUnclamped(bendNormal.normalized, legHint, 0.85f);

                case FIK_HintMode.MiddleForward: return Vector3.LerpUnclamped(bendNormal.normalized, MiddleIKBone.srcRotation * MiddleIKBone.right, 0.5f);
                case FIK_HintMode.MiddleBack: return MiddleIKBone.srcRotation * -MiddleIKBone.right;

                case FIK_HintMode.EndForward:

                    Vector3 hintPos = MiddleIKBone.srcPosition + EndIKBone.srcRotation * EndIKBone.forward;
                    Vector3 normal = Vector3.Cross(hintPos - StartIKBone.srcPosition, IKTargetPosition - StartIKBone.srcPosition);
                    if (normal == Vector3.zero) return bendNormal;

                    return normal;

                case FIK_HintMode.OnGoal: return Vector3.LerpUnclamped(bendNormal, lateEndBoneRotation * EndIKBone.right, 0.5f);
            }

            return bendNormal;
        }



        // Drawing helper gizmos for identifying IK process and setup
        public void OnDrawGizmos()
        {
            if (!Initialized) return;
        }


    }
}
