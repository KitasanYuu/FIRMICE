using UnityEngine;

namespace FIMSpace.FTools
{
    /// <summary>
    /// FC: Class for processing IK logics for 3-bones or multiple bones inverse kinematics
    /// </summary>
    [System.Serializable]
    public partial class FTools_IKProcessorek
    {
        public Vector3 IKTargetPosition;
        public Quaternion IKTargetRotation;
        public Vector3 IKElbowTargetPosition = Vector3.zero;

        public FTools_IKProcessorBone[] IKBones;// { get; private set; }

        public bool Initialized = false;
        public bool CCDIK { get; private set; }


        #region Limb / CCD IK Variables

        // Global
        [Range(0f, 1f)] public float IKWeight = 1f;

        // Limb
        public FTools_IKProcessorBone StartBone { get { return IKBones[0]; } }
        public FTools_IKProcessorBone ElbowBone { get { return IKBones[1]; } }
        public FTools_IKProcessorBone EndBone { get { if (!CCDIK) return IKBones[2]; else return IKBones[IKBones.Length - 1]; } }

        public Vector3 targetElbowNormal = Vector3.right;

        public bool LHand = false;
        public FIK_ElbowMode ElbowMode = FIK_ElbowMode.Target;
        public enum FIK_ElbowMode { None, Animation, Target, Parent, /*Arm,*/ }
        public Quaternion frameEndBoneRotation;

        /// <summary> Length of whole bones chain (squared) </summary>
        private float fullLength;

        // CCD
        [Range(1, 12)]
        public int CCD_ReactionQuality = 4;
        [Range(0f, 1f)]
        public float CCD_Smoothing = 0f;
        [Range(0f, 181f)]
        public float CCD_LimitAngle = 60f;
        public bool AutoWeight = true;

        public Vector3 LastLocalDirection;
        public Vector3 LocalDirection;

        #endregion


        #region Initiation methods

        /// <summary> Assigning bones for IK processor with limb IK logics (3-bones) </summary>
        public void SetLimb(Transform startBone, Transform elbowBone, Transform endBone)
        {
            CCDIK = false;

            IKBones = new FTools_IKProcessorBone[3];
            IKBones[0] = new FTools_IKProcessorBone() { transform = startBone };
            IKBones[1] = new FTools_IKProcessorBone() { transform = elbowBone };
            IKBones[2] = new FTools_IKProcessorBone() { transform = endBone };

            IKTargetPosition = endBone.position; IKTargetRotation = endBone.rotation;
        }


        /// <summary> Assigning bones for IK processor with CCD IK logics (unlimited bone count) </summary>
        public void SetCCD(Transform[] bonesChain)
        {
            CCDIK = true;

            IKBones = new FTools_IKProcessorBone[bonesChain.Length];
            for (int i = 0; i < bonesChain.Length; i++)
                IKBones[i] = new FTools_IKProcessorBone() { transform = bonesChain[i] };

            IKTargetPosition = EndBone.transform.position; IKTargetRotation = EndBone.transform.rotation;
        }

        #endregion

        Quaternion initWorldRootRotation;
        public void Initialize(Transform root)
        {
            if (Initialized) return;

            initWorldRootRotation = root.rotation;
            Vector3 normal = Vector3.Cross(ElbowBone.transform.position - StartBone.transform.position, EndBone.transform.position - ElbowBone.transform.position);
            if (normal != Vector3.zero) targetElbowNormal = normal;

            if (StartBone.transform.parent != null) startParentWorldRotation = Quaternion.Inverse(initWorldRootRotation) * StartBone.transform.parent.rotation;

            fullLength = 0f;

            if (!CCDIK) // Initializing Limb IK Bones
            {
                StartBone.Init(ElbowBone.transform.position, targetElbowNormal);
                ElbowBone.Init(EndBone.transform.position, targetElbowNormal);
                EndBone.Init(EndBone.transform.position + (EndBone.transform.position - ElbowBone.transform.position), targetElbowNormal);

                fullLength = IKBones[0].BoneLength + IKBones[1].BoneLength;

                RefreshOrientationNormal(); // ?
            }
            else // Initializing CCD IK Bones
            {
                float step = 1f / (float)(IKBones.Length * 1.3f);

                for (int i = 0; i < IKBones.Length; i++)
                {
                    FTools_IKProcessorBone b = IKBones[i];

                    if (i < IKBones.Length - 1)
                    {
                        b.Init(IKBones[i + 1].transform.position, targetElbowNormal);
                        fullLength += b.BoneLength;
                        b.Axis = Quaternion.Inverse(b.transform.rotation) * (IKBones[i + 1].transform.position - b.transform.position);
                    }
                    else
                        b.Axis = Quaternion.Inverse(b.transform.rotation) * (IKBones[IKBones.Length - 1].transform.position - IKBones[0].transform.position);

                    if (AutoWeight) b.MotionWeight = 1f - step * i;
                }
            }

            if (CCD_LimitAngle < 180)
                for (int i = 0; i < IKBones.Length; i++)
                {
                    IKBones[i].angleLimit = CCD_LimitAngle;
                    IKBones[i].twistAngleLimit = Mathf.Min(80f, CCD_LimitAngle);
                }

            Initialized = true;
        }


        /// <summary> Updates Limb or CCD IK depends which setup is initialized </summary>
        public void Update()
        {
            //for (int i = 0; i < IKBones.Length; i++)
            //    IKBones[i].transform.localRotation = IKBones[i].initLocalRotation;

            if (CCDIK) UpdateCCDIK(); else UpdateLimbIK();
        }


        #region Limb IK Methods

        /// <summary> Updating processor with 3-bones oriented inverse kinematics </summary>
        public void UpdateLimbIK()
        {
            if (!Initialized) return;

            frameEndBoneRotation = EndBone.transform.rotation;

            StartBone.BoneLength = (ElbowBone.transform.position - StartBone.transform.position).sqrMagnitude;
            ElbowBone.BoneLength = (EndBone.transform.position - ElbowBone.transform.position).sqrMagnitude;

            targetElbowNormal = GetOrientationNormal();

            Vector3 orientationDirection = GetOrientationDirection(IKTargetPosition, targetElbowNormal);
            if (orientationDirection == Vector3.zero) orientationDirection = ElbowBone.transform.position - StartBone.transform.position;

            StartBone.transform.rotation = StartBone.GetRotation(orientationDirection, targetElbowNormal);
            ElbowBone.transform.rotation = ElbowBone.GetRotation(IKTargetPosition - ElbowBone.transform.position, ElbowBone.GetCurrentOrientationNormal());
        }


        /// <summary> Returning >= 1f when max range for IK point is reached </summary>
        public float GetStretchValue(Vector3 targetPos)
        {
            if (!CCDIK)
            {
                float fullLength = Mathf.Epsilon;
                fullLength += (StartBone.transform.position - ElbowBone.transform.position).magnitude;
                fullLength += (ElbowBone.transform.position - EndBone.transform.position).magnitude;

                float toGoal = (StartBone.transform.position - targetPos).magnitude;

                return toGoal / fullLength;
            }
            else
            {
                float fullLength = Mathf.Epsilon;

                for (int i = 0; i < IKBones.Length - 1; i++)
                    fullLength += (IKBones[i].transform.position - IKBones[i + 1].transform.position).magnitude;

                float toGoal = (StartBone.transform.position - targetPos).magnitude;

                return toGoal / fullLength;
            }
        }

        private Vector3 GetOrientationNormal()
        {
            if (IKElbowTargetPosition.sqrMagnitude != 0)
                return CalculateElbowNormalToPosition(IKElbowTargetPosition);
            else
                return GetAutomaticElbowNormal();

            // weight
        }

        public Vector3 CalculateElbowNormalToPosition(Vector3 targetElbowPos)
        {
            return Vector3.Cross(targetElbowPos - StartBone.transform.position, EndBone.transform.position - StartBone.transform.position);
        }

        public void RefreshOrientationNormal()
        {
            Vector3 normal = Vector3.Cross(ElbowBone.transform.position - StartBone.transform.position, EndBone.transform.position - ElbowBone.transform.position);
            if (normal != Vector3.zero) targetElbowNormal = normal;
        }

        private Vector3 GetOrientationDirection(Vector3 ikPosition, Vector3 orientationNormal)
        {
            Vector3 direction = ikPosition - StartBone.transform.position;
            if (direction == Vector3.zero) return Vector3.zero;

            float directionLength = direction.sqrMagnitude;
            float forward = (directionLength + StartBone.BoneLength - ElbowBone.BoneLength) / 2f / Mathf.Sqrt(directionLength);
            float up = Mathf.Sqrt(StartBone.BoneLength - forward * forward);
            if (float.IsNaN(up)) up = 0f;

            Vector3 perpendicularUp = Vector3.Cross(direction, orientationNormal);

            return Quaternion.LookRotation(direction, perpendicularUp) * new Vector3(0f, up, forward);
        }

        bool maintained = false;
        [Range(0f, 1f)]
        public float weight = 1f;
        Quaternion startParentWorldRotation;
        private Vector3 GetAutomaticElbowNormal()
        {
            Vector3 bendNormal = StartBone.GetCurrentOrientationNormal();

            switch (ElbowMode)
            {
                case FIK_ElbowMode.Animation:
                    if (!maintained) targetElbowNormal = StartBone.GetCurrentOrientationNormal(); maintained = false;
                    return Vector3.Lerp(bendNormal, targetElbowNormal, weight);

                case FIK_ElbowMode.Parent:
                    Quaternion parentRotation = StartBone.transform.parent.rotation * Quaternion.Inverse(startParentWorldRotation);
                    return Quaternion.Slerp(Quaternion.identity, parentRotation * Quaternion.Inverse(initWorldRootRotation), weight) * bendNormal;

                case FIK_ElbowMode.Target:
                    Quaternion targetRotation = IKTargetRotation * Quaternion.Inverse(EndBone.initLocalRotation);
                    return Quaternion.Slerp(Quaternion.identity, targetRotation, weight) * bendNormal;

                    {
                        //case FIK_ElbowMode.Arm:

                        //    if (StartBone.transform.parent == null) return bendNormal;
                        //    Vector3 direction = (IKTargetPosition - StartBone.transform.position).normalized;
                        //    direction = Quaternion.Inverse(StartBone.transform.parent.rotation * Quaternion.Inverse(startParentWorldRotation)) * direction;
                        //    if (LHand) direction.x = -direction.x;

                        //    for (int i = 1; i < axisDirections.Length; i++)
                        //    {
                        //        axisDirections[i].dot = Mathf.Clamp(Vector3.Dot(axisDirections[i].direction, direction), 0f, 1f);
                        //        axisDirections[i].dot = EaseInOutQuint(0f, 1f, axisDirections[i].dot);
                        //    }

                        //    Vector3 sum = axisDirections[0].axis;
                        //    for (int i = 1; i < axisDirections.Length; i++) sum = Vector3.Slerp(sum, axisDirections[i].axis, axisDirections[i].dot);
                        //    if (LHand) { sum.x = -sum.x; sum = -sum; }

                        //    Vector3 armBendNormal = StartBone.transform.parent.rotation * Quaternion.Inverse(startParentWorldRotation) * sum;
                        //    if (weight >= 1) return armBendNormal;
                        //    return Vector3.Lerp(bendNormal, armBendNormal, weight);
                    }

            }

            return bendNormal;
        }

        //float EaseInOutQuint(float start, float end, float value)
        //{
        //    value /= .5f; end -= start;
        //    if (value < 1) return end * 0.5f * value * value * value * value * value + start; value -= 2;
        //    return end * 0.5f * (value * value * value * value * value + 2) + start;
        //}

        #endregion


        #region CCD IK Methods

        /// <summary> Updating processor with n-bones oriented inverse kinematics </summary>
        public void UpdateCCDIK()
        {
            if (!Initialized) return;

            if (CCD_ReactionQuality < 0) CCD_ReactionQuality = 1;
            Vector3 goalPivotOffset = Vector3.zero;
            if (CCD_ReactionQuality > 1) goalPivotOffset = GetGoalPivotOffset();

            for (int itr = 0; itr < CCD_ReactionQuality; itr++)
            {
                // Restrictions for multiple interations
                if (itr >= 1)
                    if (goalPivotOffset.sqrMagnitude == 0)
                        if (CCD_Smoothing > 0)
                            if (GetVelocityDifference() < CCD_Smoothing * CCD_Smoothing) break;

                LastLocalDirection = RefreshLocalDirection();

                Vector3 ikGoal = IKTargetPosition + goalPivotOffset;

                // Solving CCD IK
                for (int b = IKBones.Length - 2; b > -1; b--)
                {
                    float weight = IKBones[b].MotionWeight * IKWeight;

                    if (weight > 0f)
                    {
                        Vector3 toEnd = IKBones[IKBones.Length - 1].transform.position - IKBones[b].transform.position;
                        Vector3 toTarget = ikGoal - IKBones[b].transform.position;

                        Quaternion targetRotation = Quaternion.FromToRotation(toEnd, toTarget) * IKBones[b].transform.rotation;

                        if (weight < 1) IKBones[b].transform.rotation = Quaternion.Lerp(IKBones[b].transform.rotation, targetRotation, weight);
                        else IKBones[b].transform.rotation = targetRotation;
                    }

                    IKBones[b].AngleLimiting();
                }
            }

            LastLocalDirection = RefreshLocalDirection();
        }


        protected Vector3 GetGoalPivotOffset()
        {
            if (!GoalPivotOffsetDetected()) return Vector3.zero;

            Vector3 IKDirection = (IKTargetPosition - IKBones[0].transform.position).normalized;
            Vector3 secondaryDirection = new Vector3(IKDirection.y, IKDirection.z, IKDirection.x);

            if (CCD_LimitAngle > 0f)
                if (IKBones[IKBones.Length - 2].angleLimit < 180 || IKBones[IKBones.Length - 2].twistAngleLimit < 180)
                    secondaryDirection = IKBones[IKBones.Length - 2].transform.rotation * IKBones[IKBones.Length - 2].Axis;

            return Vector3.Cross(IKDirection, secondaryDirection) * IKBones[IKBones.Length - 2].BoneLength * 0.5f;
        }

        private bool GoalPivotOffsetDetected()
        {
            if (!Initialized) return false;

            Vector3 toLastDirection = IKBones[IKBones.Length - 1].transform.position - IKBones[0].transform.position;
            Vector3 toGoalDirection = IKTargetPosition - IKBones[0].transform.position;

            float toLastMagn = toLastDirection.magnitude;
            float toGoalMagn = toGoalDirection.magnitude;

            if (toGoalMagn == 0) return false;
            if (toLastMagn == 0) return false;
            if (toLastMagn < toGoalMagn) return false;
            if (toLastMagn < fullLength - (IKBones[IKBones.Length - 2].BoneLength * 0.1f)) return false;
            if (toGoalMagn > toLastMagn) return false;

            float dot = Vector3.Dot(toLastDirection / toLastMagn, toGoalDirection / toGoalMagn);
            if (dot < 0.999f) return false;

            return true;
        }

        Vector3 RefreshLocalDirection()
        {
            LocalDirection = IKBones[0].transform.InverseTransformDirection(IKBones[IKBones.Length - 1].transform.position - IKBones[0].transform.position);
            return LocalDirection;
        }

        float GetVelocityDifference()
        { return Vector3.SqrMagnitude(LocalDirection - LastLocalDirection); }

        #endregion


        [System.Serializable]
        public class FTools_IKProcessorBone
        {

            public Transform transform;
            public float BoneLength;
            public Vector3 Axis;
            public float MotionWeight = 1f;

            [SerializeField] private Quaternion targetToLocalSpace;
            [SerializeField] private Vector3 defaultLocalPoleNormal;
            public Quaternion initWorldRotation;

            #region CCD IK Variables

            [Range(0f, 180f)] public float angleLimit = 45;
            [Range(0f, 180f)] public float twistAngleLimit = 180;
            public Vector2 hingeLimits = Vector2.zero;

            public Quaternion initLocalRotation;
            public Quaternion previousHingeRotation;
            public float previousHingeAngle;

            #endregion

            public void Init(Vector3 childPosition, Vector3 orientationNormal)
            {
                Quaternion defaultTargetRotation = Quaternion.LookRotation(childPosition - transform.position, orientationNormal);

                targetToLocalSpace = RotationToLocal(transform.rotation, defaultTargetRotation);
                defaultLocalPoleNormal = Quaternion.Inverse(transform.rotation) * orientationNormal;
                BoneLength = (childPosition - transform.position).sqrMagnitude;
                initLocalRotation = transform.localRotation;
                initWorldRotation = transform.rotation;
            }

            #region Limb IK methods

            public static Quaternion RotationToLocal(Quaternion parent, Quaternion rotation)
            { return Quaternion.Inverse(Quaternion.Inverse(parent) * rotation); }

            public Quaternion GetRotation(Vector3 direction, Vector3 orientationNormal)
            { return Quaternion.LookRotation(direction, orientationNormal) * targetToLocalSpace; }

            public Vector3 GetCurrentOrientationNormal()
            { return transform.rotation * (defaultLocalPoleNormal); }

            #endregion


            #region CCD IK Methods

            public void AngleLimiting()
            {
                Quaternion localRotation = Quaternion.Inverse(initLocalRotation) * transform.localRotation;
                Quaternion limitedRotation = localRotation;

                if (hingeLimits.sqrMagnitude == 0)
                {
                    if (angleLimit < 180) limitedRotation = LimitPY(limitedRotation);
                    if (twistAngleLimit < 180) limitedRotation = LimitRoll(limitedRotation);
                }
                else limitedRotation = LimitHinge(limitedRotation);

                if (Equals(limitedRotation, localRotation)) return;

                transform.localRotation = initLocalRotation * limitedRotation;
            }

            private Quaternion LimitPY(Quaternion rotation)
            {
                if (Equals(rotation, Quaternion.identity)) return rotation;

                Vector3 pyAxis = rotation * Axis;
                Quaternion angleRotation = Quaternion.FromToRotation(Axis, pyAxis);
                Quaternion limitAngle = Quaternion.RotateTowards(Quaternion.identity, angleRotation, angleLimit);
                Quaternion limit = Quaternion.FromToRotation(pyAxis, limitAngle * Axis);

                return limit * rotation;
            }

            private Quaternion LimitRoll(Quaternion currentRotation)
            {
                Vector3 orthogonalAxis = new Vector3(Axis.y, Axis.z, Axis.x);
                Vector3 normal = currentRotation * Axis;
                Vector3 tangent = orthogonalAxis;
                Vector3.OrthoNormalize(ref normal, ref tangent);

                Vector3 tangentRotation = currentRotation * orthogonalAxis;
                Vector3.OrthoNormalize(ref normal, ref tangentRotation);

                Quaternion limitRot = Quaternion.FromToRotation(tangentRotation, tangent) * currentRotation;
                if (twistAngleLimit <= 0) return limitRot;

                return Quaternion.RotateTowards(limitRot, currentRotation, twistAngleLimit);
            }

            private Quaternion LimitHinge(Quaternion rotation)
            {
                Quaternion freeDegree = Quaternion.FromToRotation(rotation * Axis, Axis) * rotation;
                Quaternion addRotation = freeDegree * Quaternion.Inverse(previousHingeRotation);

                float addAngle = Quaternion.Angle(Quaternion.identity, addRotation);

                Vector3 orthogonalAxis = new Vector3(Axis.z, Axis.x, Axis.y);
                Vector3 cross = Vector3.Cross(orthogonalAxis, Axis);
                if (Vector3.Dot(addRotation * orthogonalAxis, cross) > 0f) addAngle = -addAngle;

                previousHingeAngle = Mathf.Clamp(previousHingeAngle + addAngle, hingeLimits.x, hingeLimits.y);

                previousHingeRotation = Quaternion.AngleAxis(previousHingeAngle, Axis);
                return previousHingeRotation;
            }

            #endregion

        }

    }
}
