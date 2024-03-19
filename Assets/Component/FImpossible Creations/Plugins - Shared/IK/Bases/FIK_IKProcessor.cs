using System;
using UnityEngine;

namespace FIMSpace.FTools
{
    /// <summary>
    /// FC: Class for processing IK logics for 3-bones inverse kinematics
    /// </summary>
    [System.Serializable]
    public class FIK_IKProcessor : FIK_ProcessorBase
    {
        // TODO -> Limiting, Weights, Goal Modes

        #region Processor


        #region Bone Refs etc.

        public IKBone StartIKBone { get { return IKBones[0]; } }
        public IKBone MiddleIKBone { get { return IKBones[1]; } }
        public IKBone EndIKBone { get { return IKBones[2]; } }

        public IKBone GetBone(int index) { return IKBones[index]; }
        public int BonesCount { get { return IKBones.Length; } }

        #endregion


        [Space(4)] [SerializeField] private IKBone[] IKBones;
        [Space(4)] [Range(0f, 1f)] public float PositionWeight = 1f;
        [Range(0f, 1f)] public float RotationWeight = 1f;

        [HideInInspector] public bool UseEnsuredRotation = false;


        #region IK Hint

        public enum FIK_HintMode { Default, MiddleForward, MiddleBack, OnGoal, EndForward, Cross }

        public FIK_HintMode AutoHintMode = FIK_HintMode.MiddleForward;

        [Range(0f, 1f)] public float ManualHintPositionWeight = 0f;
        public Vector3 IKManualHintPosition = Vector3.zero;

        #endregion

        /// <summary> Reference scale for computations - active length from start bone to middle knee </summary>
        public float ScaleReference { get; protected set; }

        private Transform rootTransform;
        private bool everyIsChild;

        private Vector3 targetElbowNormal = Vector3.right;
        private Quaternion lastEndBoneRotation;
        private Quaternion postIKAnimatorEndBoneRot;

        Quaternion preS = Quaternion.identity;
        Quaternion preM = Quaternion.identity;
        Quaternion preE = Quaternion.identity;

        private float limbLengthRootScale;
        private float limbLength;
        private float limbMidLength;


        /// <summary> Assigning bones for IK processor with limb IK logics (3-bones) </summary>
        public FIK_IKProcessor(Transform startBone, Transform midBone, Transform endBone)
        {
            SetBones(startBone, midBone, endBone);
            IKTargetPosition = endBone.position; IKTargetRotation = endBone.rotation;
        }


        #region Bones Assignement and Bone datas refresh

        public void SetBones(Transform startBone, Transform midBone, Transform endBone)
        {

            IKBones = new IKBone[3];
            IKBones[0] = new IKBone(startBone);
            IKBones[1] = new IKBone(midBone);
            IKBones[2] = new IKBone(endBone);

            Bones = new FIK_IKBoneBase[3] { IKBones[0], IKBones[1], IKBones[2] };

            IKBones[0].SetChild(IKBones[1]);
            IKBones[1].SetChild(IKBones[2]);
        }

        public void SetBones(Transform startBone, Transform endBone)
        {
            SetBones(startBone, endBone.parent, endBone);
        }


        public override void PreCalibrate()
        {
            base.PreCalibrate();
            RefreshScaleReference();
        }

        /// <summary> Distance between first and middle bone </summary>
        public void RefreshScaleReference()
        {
            ScaleReference = (StartBone.transform.position - MiddleIKBone.transform.position).magnitude;
        }

        #endregion

        [NonSerialized] public bool AllowEditModeInit = false;

        public override void Init(Transform root)
        {
            if (Initialized) return;

            rootTransform = root;

            Vector3 normal = Vector3.Cross(MiddleIKBone.transform.position - StartBone.transform.position, EndBone.transform.position - MiddleIKBone.transform.position);
            if (normal != Vector3.zero) targetElbowNormal = normal;

            fullLength = 0f;

            StartIKBone.Init(root, MiddleIKBone.transform.position, targetElbowNormal, UseEnsuredRotation);
            MiddleIKBone.Init(root, EndBone.transform.position, targetElbowNormal, UseEnsuredRotation);
            EndIKBone.Init(root, EndBone.transform.position + (EndBone.transform.position - MiddleIKBone.transform.position), targetElbowNormal, UseEnsuredRotation);

            fullLength = Bones[0].BoneLength + Bones[1].BoneLength;

            RefreshOrientationNormal();

            limbLengthRootScale = root.lossyScale.x;
            limbLength = Vector3.Distance(StartIKBone.transform.position, MiddleIKBone.transform.position);
            limbLength += Vector3.Distance(EndIKBone.transform.position, MiddleIKBone.transform.position);


            // Checking if bones hierarchy is fully connected and straight forward direct
            if (EndBone.transform.parent != MiddleIKBone.transform)
            {
                everyIsChild = false;
                limbMidLength = Vector3.Distance(EndIKBone.transform.position, MiddleIKBone.transform.position);
            }
            else
            if (MiddleIKBone.transform.parent != StartBone.transform) everyIsChild = false;
            else everyIsChild = true;

            if (AllowEditModeInit) Initialized = true; else if (Application.isPlaying) Initialized = true;
        }

        public void RefreshAnimatorCoords()
        {
            StartIKBone.CaptureSourceAnimation();
            MiddleIKBone.CaptureSourceAnimation();
            EndIKBone.CaptureSourceAnimation();
        }


        /// <summary> Updating processor with 3-bones oriented inverse kinematics </summary>
        public override void Update()
        {
            if (!Initialized) return;

            RefreshAnimatorCoords();

            // If limb have more than 3 point bones then we must update some data for main two bones
            if (!everyIsChild)
            {
                //StartIKBone.RefreshOrientations(MiddleBone.transform.position, targetElbowNormal);
                MiddleIKBone.RefreshOrientations(EndBone.transform.position, targetElbowNormal);
            }

            // Foot IK Position ---------------------------------------------------

            float posWeight = PositionWeight * IKWeight;
            StartBone.sqrMagn = (MiddleIKBone.transform.position - StartBone.transform.position).sqrMagnitude;
            MiddleIKBone.sqrMagn = (EndBone.transform.position - MiddleIKBone.transform.position).sqrMagnitude;

            targetElbowNormal = GetOrientationNormal();

            Vector3 orientationDirection = GetOrientationDirection(IKTargetPosition, targetElbowNormal);

            if (orientationDirection == Vector3.zero) orientationDirection = MiddleIKBone.transform.position - StartBone.transform.position;

            if (posWeight > 0f)
            {
                Quaternion sBoneRot = StartIKBone.GetRotation(orientationDirection, targetElbowNormal) * StartBoneRotationOffset;
                if (posWeight < 1f) sBoneRot = Quaternion.LerpUnclamped(StartIKBone.srcRotation, sBoneRot, posWeight);
                StartBone.transform.rotation = sBoneRot;

                if (UseEnsuredRotation)
                {
                    StartBone.transform.rotation = AnimationTools.AnimationGenerateUtils.EnsureQuaternionContinuity(preS, StartBone.transform.rotation);
                    preS = StartBone.transform.rotation;
                }

                Quaternion sMidBoneRot = MiddleIKBone.GetRotation(IKTargetPosition - MiddleIKBone.transform.position, MiddleIKBone.GetCurrentOrientationNormal());
                if (posWeight < 1f) sMidBoneRot = Quaternion.LerpUnclamped(MiddleIKBone.srcRotation, sMidBoneRot, posWeight);
                MiddleIKBone.transform.rotation = sMidBoneRot;


                if (UseEnsuredRotation)
                {
                    MiddleIKBone.transform.rotation = AnimationTools.AnimationGenerateUtils.EnsureQuaternionContinuity(preM, MiddleIKBone.transform.rotation);
                    preM = MiddleIKBone.transform.rotation;
                }
            }

            postIKAnimatorEndBoneRot = EndBone.transform.rotation;


            // Foot IK Rotation ---------------------------------------------------

            float rotWeight = RotationWeight * IKWeight;

            if (rotWeight > 0f)
            {
                if (rotWeight < 1f)
                    EndBone.transform.rotation = Quaternion.LerpUnclamped(postIKAnimatorEndBoneRot, IKTargetRotation, rotWeight);
                else
                    EndBone.transform.rotation = IKTargetRotation;

                if (UseEnsuredRotation)
                {
                    EndBone.transform.rotation = AnimationTools.AnimationGenerateUtils.EnsureQuaternionContinuity(preE, EndBone.transform.rotation);
                    preE = EndBone.transform.rotation;
                }
            }

            lastEndBoneRotation = EndBone.transform.rotation;
        }


        public float GetLimbLength()
        {
            if (rootTransform.lossyScale.x == 0f) return 0f;

            float scaleFactor = (rootTransform.lossyScale.x / limbLengthRootScale);

            if (!everyIsChild)
            {
                float midDiff = (limbMidLength * scaleFactor) - Vector3.Distance(EndIKBone.srcPosition, MiddleIKBone.srcPosition);
                return (limbLength * scaleFactor) - midDiff;
            }

            return limbLength * scaleFactor;
        }


        public Vector3 GetHintDefaultPosition()
        {
            return MiddleIKBone.srcPosition + MiddleIKBone.srcRotation * StartIKBone.GetCurrentOrientationNormal();
            //return StartIKBone.srcPosition + GetOrientationDirection(IKTargetPosition, StartIKBone.GetCurrentOrientationNormal());
        }

        /// <summary> Returning >= 1f when max range for IK point is reached </summary>
        public float GetStretchValue(Vector3 targetPos)
        {
            return GetStretchValue((StartIKBone.srcPosition - targetPos).magnitude);
        }

        /// <summary> Returning >= 1f when max range for IK point is reached </summary>
        public float GetStretchValue(float distance)
        {
            return distance / GetLimbLength();

            //float fullLength = Mathf.Epsilon;
            //fullLength += (StartBone.transform.position - MiddleIKBone.transform.position).magnitude;
            //fullLength += (MiddleIKBone.transform.position - EndBone.transform.position).magnitude;

            //return distance / fullLength;
        }


        private Vector3 GetOrientationNormal()
        {
            if (ManualHintPositionWeight > 0f)
            {
                if (ManualHintPositionWeight >= 1f)
                    return CalculateElbowNormalToPosition(IKManualHintPosition);
                else
                    return Vector3.LerpUnclamped(GetAutomaticElbowNormal().normalized, CalculateElbowNormalToPosition(IKManualHintPosition), ManualHintPositionWeight);
            }
            else
                return GetAutomaticElbowNormal();
        }


        public Vector3 CalculateElbowNormalToPosition(Vector3 targetElbowPos)
        {
            return Vector3.Cross(targetElbowPos - StartBone.transform.position, EndBone.transform.position - StartBone.transform.position);
        }


        public void RefreshOrientationNormal()
        {
            Vector3 normal = Vector3.Cross(MiddleIKBone.transform.position - StartBone.transform.position, EndBone.transform.position - MiddleIKBone.transform.position);
            if (normal != Vector3.zero) targetElbowNormal = normal;
        }


        private Vector3 GetOrientationDirection(Vector3 ikPosition, Vector3 orientationNormal)
        {
            Vector3 direction = ikPosition - StartBone.transform.position; // From start bone to target ik position
            if (direction == Vector3.zero) return Vector3.zero;

            float distSqrStartToGoal = direction.sqrMagnitude; // Computing length for bones
            float distStartToGoal = Mathf.Sqrt(distSqrStartToGoal);

            float forwardLen = (distSqrStartToGoal + StartBone.sqrMagn - MiddleIKBone.sqrMagn) / 2f / distStartToGoal;
            float upLen = Mathf.Sqrt(Mathf.Clamp(StartBone.sqrMagn - forwardLen * forwardLen, 0, Mathf.Infinity));

            Vector3 perpendicularUp = Vector3.Cross(direction / distStartToGoal, orientationNormal);
            return Quaternion.LookRotation(direction, perpendicularUp) * new Vector3(0f, upLen, forwardLen);
        }


        private Vector3 GetAutomaticElbowNormal()
        {
            Vector3 bendNormal = StartIKBone.GetCurrentOrientationNormal();

            switch (AutoHintMode)
            {
                case FIK_HintMode.MiddleForward: return Vector3.LerpUnclamped(bendNormal.normalized, MiddleIKBone.srcRotation * MiddleIKBone.right, 0.5f);
                case FIK_HintMode.MiddleBack: return MiddleIKBone.srcRotation * -MiddleIKBone.right;

                case FIK_HintMode.EndForward:

                    Vector3 hintPos = MiddleIKBone.srcPosition + EndIKBone.srcRotation * EndIKBone.forward;
                    Vector3 normal = Vector3.Cross(hintPos - StartIKBone.srcPosition, IKTargetPosition - StartIKBone.srcPosition);
                    if (normal == Vector3.zero) return bendNormal;

                    return normal;

                case FIK_HintMode.OnGoal: return lastEndBoneRotation * EndIKBone.right;
                //case FIK_HintMode.OnGoal: return Vector3.LerpUnclamped(bendNormal.normalized, lastEndBoneRotation * EndIKBone.right, 1f);

                case FIK_HintMode.Cross:
                    return Vector3.Cross(MiddleIKBone.srcPosition - StartIKBone.srcPosition, EndIKBone.srcPosition - MiddleIKBone.srcPosition);

            }

            return bendNormal;
        }

        #endregion


        public void OnDrawGizmos()
        {
            if (!Initialized) return;
        }


        [System.Serializable]
        public class IKBone : FIK_IKBoneBase
        {
            [SerializeField] private Quaternion targetToLocalSpace;
            [SerializeField] private Vector3 defaultLocalPoleNormal;

            public Vector3 right;
            public Vector3 up;
            public Vector3 forward;

            public Vector3 srcPosition;
            public Quaternion srcRotation;

            bool ensured = false;
            Quaternion pre = Quaternion.identity;

            public IKBone(Transform t) : base(t) { }

            public void Init(Transform root, Vector3 childPosition, Vector3 orientationNormal, bool ensured)
            {
                RefreshOrientations(childPosition, orientationNormal);

                sqrMagn = (childPosition - transform.position).sqrMagnitude;
                LastKeyLocalRotation = transform.localRotation;

                right = transform.InverseTransformDirection(root.right);
                up = transform.InverseTransformDirection(root.up);
                forward = transform.InverseTransformDirection(root.forward);

                CaptureSourceAnimation();
            }

            public void RefreshOrientations(Vector3 childPosition, Vector3 orientationNormal)
            {
                Quaternion defaultTargetRotation = Quaternion.LookRotation(childPosition - transform.position, orientationNormal);

                if (ensured)
                {
                    defaultTargetRotation = FIMSpace.AnimationTools.AnimationGenerateUtils.EnsureQuaternionContinuity(pre, defaultTargetRotation);
                    pre = defaultTargetRotation;
                }

                targetToLocalSpace = RotationToLocal(transform.rotation, defaultTargetRotation);
                defaultLocalPoleNormal = Quaternion.Inverse(transform.rotation) * (orientationNormal);
            }

            public void CaptureSourceAnimation()
            {
                srcPosition = transform.position;
                srcRotation = transform.rotation;
            }

            public static Quaternion RotationToLocal(Quaternion parent, Quaternion rotation)
            { return Quaternion.Inverse(Quaternion.Inverse(parent) * rotation); }

            public Quaternion GetRotation(Vector3 direction, Vector3 orientationNormal)
            { return Quaternion.LookRotation(direction, orientationNormal) * targetToLocalSpace; }

            public Vector3 GetCurrentOrientationNormal()
            {
                //Debug.DrawRay(transform.position, transform.rotation * defaultLocalPoleNormal, Color.yellow);
                return transform.rotation * (defaultLocalPoleNormal);
            }


        }

    }

}
