using UnityEngine;

namespace FIMSpace.FTools
{
    public partial class FimpIK_Limb : FIK_ProcessorBase
    {
        public Transform Root { get; protected set; }
        public IKBone StartIKBone { get { return IKBones[0]; } }
        public IKBone MiddleIKBone { get { return IKBones[1]; } }
        public IKBone EndIKBone { get { return IKBones[2]; } }

        /// <summary> If there is bone between end and middle bone, it's initial info is stored there, otherwise it's simply MiddleIKBone</summary>
        public IKBone EndParentIKBone { get; private set; }

        public IKBone GetBone(int index) { return IKBones[index]; }
        public int BonesCount { get { return IKBones.Length; } }

        public enum FIK_HintMode { Default, MiddleForward, MiddleBack, OnGoal, EndForward, Leg }
        private bool everyIsChild = true;
        private bool hasFeet = false;
        private bool hasRoot = false;

        public override void Init(Transform root)
        {
            if (Initialized) return;

            Vector3 preNormal = Vector3.Cross(MiddleIKBone.transform.position - StartIKBone.transform.position, EndIKBone.transform.position - MiddleIKBone.transform.position);
            if (preNormal != Vector3.zero) targetElbowNormal = preNormal;

            fullLength = 0f;

            StartIKBone.Init(root, MiddleIKBone.transform.position, targetElbowNormal);
            MiddleIKBone.Init(root, EndIKBone.transform.position, targetElbowNormal);
            EndIKBone.Init(root, EndIKBone.transform.position + (EndIKBone.transform.position - MiddleIKBone.transform.position), targetElbowNormal);

            fullLength = Bones[0].BoneLength + Bones[1].BoneLength;
            RefreshDefaultFlexNormal();

            // Checking if bones hierarchy is fully connected and straight forward direct
            if (EndIKBone.transform.parent != MiddleIKBone.transform) everyIsChild = false;
            else
            if (MiddleIKBone.transform.parent != StartIKBone.transform) everyIsChild = false;
            else everyIsChild = true;

            SetRootReference(root);

            if (Application.isPlaying) Initialized = true;

            if ( hasFeet) PrepareFeet();

            if (everyIsChild) EndParentIKBone = MiddleIKBone;
            else EndParentIKBone = new IKBone(EndIKBone.transform.parent);
        }

        public void SetBones(Transform startBone, Transform midBone, Transform endBone)
        {
            IKBones = new IKBone[3];
            IKBones[0] = new IKBone(startBone);
            IKBones[1] = new IKBone(midBone);
            IKBones[2] = new IKBone(endBone);

            Bones = new FIK_IKBoneBase[3] { IKBones[0], IKBones[1], IKBones[2] };

            IKBones[0].SetChild(IKBones[1]);
            IKBones[1].SetChild(IKBones[2]);

            IKTargetPosition = endBone.position; IKTargetRotation = endBone.rotation;
        }


        public void SetLegWithFeet(Transform startBone, Transform midBone, Transform endBone, Transform feet)
        {
            IKBones = new IKBone[4];
            IKBones[0] = new IKBone(startBone);
            IKBones[1] = new IKBone(midBone);
            IKBones[2] = new IKBone(endBone);
            IKBones[3] = new IKBone(feet);

            Bones = new FIK_IKBoneBase[4] { IKBones[0], IKBones[1], IKBones[2], IKBones[3] };

            IKBones[0].SetChild(IKBones[1]);
            IKBones[1].SetChild(IKBones[2]);
            IKBones[2].SetChild(IKBones[3]);

            IKTargetPosition = endBone.position; IKTargetRotation = endBone.rotation;
            
            hasFeet = true;
        }

        public void SetBones(Transform startBone, Transform endBone)
        {
            SetBones(startBone, endBone.parent, endBone);
        }

    }
}
