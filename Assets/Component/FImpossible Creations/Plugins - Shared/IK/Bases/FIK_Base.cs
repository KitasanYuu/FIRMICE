using System;
using UnityEngine;

namespace FIMSpace.FTools
{
    /// <summary>
    /// FC: Base class for processing IK logics
    /// </summary>
    [System.Serializable]
    public abstract class FIK_ProcessorBase
    {
        [Range(0f, 1f)] public float IKWeight = 1f;
        public Vector3 IKTargetPosition;
        public Quaternion IKTargetRotation;
        public Vector3 LastLocalDirection;
        public Vector3 LocalDirection;

        /// <summary> Length of whole bones chain (squared) </summary>
        public float fullLength { get; protected set; }

        public bool Initialized { get; protected set; }

        public FIK_IKBoneBase[] Bones { get; protected set; }
        public FIK_IKBoneBase StartBone { get { return Bones[0]; } }
        public FIK_IKBoneBase EndBone { get { return Bones[Bones.Length - 1]; } }
        public Quaternion StartBoneRotationOffset { get; set; } = Quaternion.identity;


        public virtual void Init(Transform root) 
        {
            StartBoneRotationOffset = Quaternion.identity;
        }

        [NonSerialized] public bool CallPreCalibrate = true;
        public virtual void PreCalibrate()
        {
            if (!CallPreCalibrate) return;

            FIK_IKBoneBase child = Bones[0];
            while (child != null)
            {
                child.transform.localRotation = child.InitialLocalRotation;
                child = child.Child;
            }
        }

        public virtual void Update()
        {
        }

        public static float EaseInOutQuint(float start, float end, float value)
        {
            value /= .5f; end -= start;
            if (value < 1) return end * 0.5f * value * value * value * value * value + start; value -= 2;
            return end * 0.5f * (value * value * value * value * value + 2) + start;
        }
    }


    /// <summary>
    /// FC: Base class for IK bones computations
    /// </summary>
    [System.Serializable]
    public abstract class FIK_IKBoneBase
    {
        public FIK_IKBoneBase Child { get; private set; }

        public Transform transform { get; protected set; }
        public float sqrMagn = 0.1f;
        public float BoneLength = 0.1f;
        public float MotionWeight = 1f;

        public Vector3 InitialLocalPosition;
        public Quaternion InitialLocalRotation;
        public Quaternion LastKeyLocalRotation;

        public FIK_IKBoneBase(Transform t)
        {
            transform = t;

            if (transform)
            {
                InitialLocalPosition = transform.localPosition;
                InitialLocalRotation = transform.localRotation;
                LastKeyLocalRotation = t.localRotation;
            }
        }

        public virtual void SetChild(FIK_IKBoneBase child)
        {
            if (child == null) return;
            Child = child;
            sqrMagn = (child.transform.position - transform.position).sqrMagnitude;
            BoneLength = (child.transform.position - transform.position).sqrMagnitude;
        }

    }

}
