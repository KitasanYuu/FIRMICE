using System;
using System.Linq;
using UnityEngine;

namespace FIMSpace.FTools
{
    public partial class FimpIK_Arm 
    {
        public Transform Root { get; protected set; }

        public IKBone ChestIKBone;
        public IKBone ShoulderIKBone { get { return IKBones[0]; } }
        public IKBone UpperArmIKBone { get { return IKBones[1]; } }
        public IKBone ForeArmIKBone { get { return IKBones[2]; } }
        public IKBone HandIKBone { get { return IKBones[3]; } }
        public IKBone GetBone(int index) { return IKBones[index]; }
        public int BonesCount { get { return IKBones.Length; } }

        public enum FIK_HintMode { Default, MiddleForward, MiddleBack, OnGoal, EndForward }
        private bool everyIsChild;

        public void Init(Transform root)
        {
            if (Initialized) return;
            if (IKBones == null) return;

            if (IKBones.Length == 0)
            {
                SetBones(ShoulderTransform, UpperarmTransform, LowerarmTransform, HandTransform);
            }

            UpperarmRotationOffset = Quaternion.identity;
            TargetElbowNormal = Vector3.right;

            Vector3 preNormal = Vector3.Cross(ForeArmIKBone.transform.position - UpperArmIKBone.transform.position, HandIKBone.transform.position - ForeArmIKBone.transform.position);
            if (preNormal != Vector3.zero) TargetElbowNormal = preNormal;

            FullLength = 0f;

            ShoulderIKBone.Init(root, UpperArmIKBone.transform.position, TargetElbowNormal);
            UpperArmIKBone.Init(root, ForeArmIKBone.transform.position, TargetElbowNormal);
            ForeArmIKBone.Init(root, HandIKBone.transform.position, TargetElbowNormal);
            HandIKBone.Init(root, HandIKBone.transform.position + (HandIKBone.transform.position - ForeArmIKBone.transform.position), TargetElbowNormal);

            FullLength = IKBones[1].BoneLength + IKBones[2].BoneLength;
            RefreshDefaultFlexNormal();

            // Checking if bones hierarchy is fully connected and straight forward direct
            if (HandIKBone.transform.parent != ForeArmIKBone.transform) everyIsChild = false;
            else
            if (ForeArmIKBone.transform.parent != UpperArmIKBone.transform) everyIsChild = false;
            else everyIsChild = true;

            ChestIKBone = new IKBone(ShoulderIKBone.transform.parent);
            ChestIKBone.Init(root, ShoulderIKBone.transform.position, TargetElbowNormal);

            SetRootReference(root);

            // Calculating Hand middle
            HandMiddleOffset = Vector3.zero;

            if (HandIKBone.transform.childCount > 0)
            {
                HandMiddleOffset = HandIKBone.transform.GetChild(0).position;
                for (int i = 1; i < HandIKBone.transform.childCount; i++)
                {
                    HandMiddleOffset = Vector3.Lerp(HandMiddleOffset, HandIKBone.transform.GetChild(i).position, 0.5f);
                }

                HandMiddleOffset = Vector3.Lerp(HandMiddleOffset, HandIKBone.transform.position, 0.4f);
                HandMiddleOffset = HandIKBone.transform.InverseTransformPoint(HandMiddleOffset);
            }

            Initialized = true;
        }


        public void SetBones(Transform shoulder, Transform upperArm, Transform forearm, Transform hand)
        {
            if (upperArm == null || forearm == null || hand == null) return;

            ShoulderTransform = shoulder;
            UpperarmTransform = upperArm;
            LowerarmTransform = forearm;
            HandTransform = hand;

            int i = 0;
            if ( shoulder == null)
            {
                IKBones = new IKBone[3];
            }
            else
            {
                IKBones = new IKBone[4];
                IKBones[0] = new IKBone(shoulder);
                i = 1;
            }

            IKBones[i] = new IKBone(upperArm);
            IKBones[i+1] = new IKBone(forearm);
            IKBones[i+2] = new IKBone(hand);

            IKBones[0].SetChild(IKBones[1]);
            IKBones[1].SetChild(IKBones[2]);

            if ( shoulder != null) IKBones[2].SetChild(IKBones[3]);

            IKTargetPosition = hand.position; IKTargetRotation = hand.rotation;
        }


        public void SetBones()
        {
            SetBones(ShoulderTransform, UpperarmTransform, LowerarmTransform, HandTransform);
        }
    }
}
