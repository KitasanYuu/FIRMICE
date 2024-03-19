using System;
using UnityEngine;

namespace FIMSpace.FTools
{
    public partial class FimpIK_Arm
    {
        // Foot/End Bone rotation helper with root reference
        public Quaternion HandIKBoneMapping { get; protected set; }
        public void SetCustomIKRotationMappingOffset(Quaternion mappingCorrection) { HandIKBoneMapping = mappingCorrection; }

        [NonSerialized] public Vector3 HandMiddleOffset;
        private Vector3 shoulderForward;
        private UniRotateBone shoulderRotate;
        private Vector3 initHandRootSpaceFlatTowards;

        internal bool UseRotationMapping = true;

        /// <summary> >= 1.2 is max </summary>
        [NonSerialized] public float MaxStretching = 1.2f;

        /// <summary> Assigning helpful reference to main root transform of body to help IK rotations </summary>
        public virtual void SetRootReference(Transform mainParentTransform)
        {
            Root = mainParentTransform;
            Quaternion preRot = Root.transform.rotation;
            Root.transform.rotation = Quaternion.identity;

            Vector3 handForwardWorld = (HandIKBone.transform.position - ForeArmIKBone.transform.position).normalized;
            Vector3 handLocalForward = HandIKBone.transform.InverseTransformDirection(handForwardWorld);
            Vector3 handLocalRight = mainParentTransform.forward;
            Vector3 handLocalUp = Vector3.Cross(handLocalForward, handLocalRight);

            Vector3 shoulderForwardWorld = (ShoulderIKBone.transform.position - ShoulderIKBone.transform.parent.position).normalized;
            shoulderForward = ShoulderIKBone.transform.InverseTransformDirection(shoulderForwardWorld);

            HandIKBoneMapping = Quaternion.FromToRotation(handLocalRight, Vector3.right);
            HandIKBoneMapping *= Quaternion.FromToRotation(handLocalUp, Vector3.up);
            shoulderRotate = new UniRotateBone(ShoulderTransform, mainParentTransform);
            Root.transform.rotation = preRot;

            initHandRootSpaceFlatTowards = Root.InverseTransformPoint(HandTransform.position);
            initHandRootSpaceFlatTowards.y = 0f;
            initHandRootSpaceFlatTowards.Normalize();
        }



        /// <summary>
        /// Put here any euler rotation (like 0,90,0) which will be mapped for correct hand rotation no matter how bones are rotated in skeleton rig (but root reference needed)
        /// </summary>
        /// <param name="rotation"></param>
        public void SetCustomIKRotation(Quaternion rotation, float blend = 1f, bool fromDefault = false)
        {
            if (blend == 1f)
            {
                if (UseRotationMapping)
                    IKTargetRotation = rotation * HandIKBoneMapping;
                else
                    IKTargetRotation = rotation;
            }
            else
            {
                if (UseRotationMapping)
                {
                    if (fromDefault)
                        IKTargetRotation = Quaternion.LerpUnclamped(IKTargetRotation, rotation * HandIKBoneMapping, blend);
                    else
                        IKTargetRotation = Quaternion.LerpUnclamped(rotation, rotation * HandIKBoneMapping, blend);
                }
                else
                {
                    if (fromDefault)
                        IKTargetRotation = Quaternion.LerpUnclamped(IKTargetRotation, rotation, blend);
                    else
                        IKTargetRotation = Quaternion.LerpUnclamped(rotation, rotation, blend);
                }
            }
        }

        public void CaptureKeyframeAnimation()
        {
            shoulderRotate.CaptureKeyframeAnimation();

            IKBone child = IKBones[0];
            while (child != null)
            {
                child.CaptureSourceAnimation();
                child = (IKBone)child.Child;
            }
        }

        /// <summary> Reference scale for computations - active length from start bone to middle knee </summary>
        public float ScaleReference { get; protected set; }

        public void RefreshLength()
        {
            ScaleReference = (UpperArmIKBone.transform.position - ForeArmIKBone.transform.position).magnitude;
        }

        public void RefreshScaleReference()
        {
            ScaleReference = (UpperArmIKBone.transform.position - ForeArmIKBone.transform.position).magnitude;
        }


        /// <summary> Returning >= 1f when max range for IK point is reached </summary>
        public float GetStretchValue(Vector3 targetPos)
        {
            float toGoal = (UpperArmIKBone.transform.position - targetPos).magnitude;
            return toGoal / limbLength;
        }

        public float GetStretchValueSrc(Vector3 targetPos)
        {
            float toGoal = (UpperArmIKBone.srcPosition - targetPos).magnitude;
            return toGoal / limbLength;
        }

        protected virtual void CalculateLimbLength()
        {
            limbLength = Mathf.Epsilon;

            //if (ShoulderIKBone.transform)
            //{
            //    float shouldLen = (ShoulderIKBone.transform.position - UpperArmIKBone.transform.position).magnitude;
            //    limbLength += shouldLen * ShoulderBlend;
            //}

            limbLength += (UpperArmIKBone.transform.position - ForeArmIKBone.transform.position).magnitude;
            limbLength += (ForeArmIKBone.transform.position - HandIKBone.transform.position).magnitude;
        }

        public bool PreventShoulderThirdQuat { get; set; } = true;
        /// <summary> By default value is 0.75 </summary>
        public float ShoulderSensitivity { get; set; } = 0.75f;
        public float PreventShoulderThirdQuatFactor { get; set; } = 0.01f;
        public float limbLength { get; private set; } = 0.1f;

        // Shoulder -----------------------
        void ComputeShoulder(Vector3 finalIKPos)
        {
            if (!Initialized) return;
            if (ShoulderBlend <= 0f) return;

            Vector3 toGoal = (finalIKPos - shoulderRotate.transform.position);
            Quaternion nRot;

            if (Root)
            {
                //nRot =(Root.rotation) * Quaternion.Euler(rrr);
                //nRot *= shoulderRotate.transform.rotation;

                Quaternion preRot = shoulderRotate.transform.rotation;
                Quaternion q = Quaternion.FromToRotation(Root.InverseTransformDirection(toGoal).normalized, initHandRootSpaceFlatTowards);

                Vector3 mappedRotation = -q.eulerAngles;

                shoulderRotate.RotateXBy(mappedRotation.x);
                shoulderRotate.RotateYBy(mappedRotation.y);
                shoulderRotate.RotateZBy(mappedRotation.z);

                nRot = shoulderRotate.transform.rotation;
                shoulderRotate.transform.rotation = preRot;
            }
            else
            {
                nRot = (ShoulderIKBone.GetRotation(toGoal.normalized, ShoulderIKBone.srcRotation * shoulderRotate.upReference));
            }

            float blend = IKWeight * ShoulderBlend;
            float armStretch = GetStretchValue(finalIKPos);

            armStretch *= 0.85f;
            if (armStretch > 1f) armStretch = 1f;

            blend *= Mathf.InverseLerp(0.6f, 1f, armStretch) * 0.9f;

            ShoulderIKBone.transform.rotation = Quaternion.Slerp(shoulderRotate.transform.rotation, nRot, blend);
        }

    }
}
