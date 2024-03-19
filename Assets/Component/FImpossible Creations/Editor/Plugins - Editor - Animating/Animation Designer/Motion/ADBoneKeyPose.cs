using UnityEngine;

namespace FIMSpace.AnimationTools
{
    [System.Serializable]
    public class ADBoneKeyPose
    {
        public int ID = -1;
        public Transform TempTransform;
        public string BoneName;
        public int BoneDepth;
        public Vector3 LocalPosition;
        public Quaternion LocalRotation;
        public Vector3 LocalScale;

        public ADBoneKeyPose(ADBoneReference b):this(b.TempTransform, b.ID) { }

        public ADBoneKeyPose(Transform copyFrom, int id = -1)
        {
            BoneName = copyFrom.name;
            TempTransform = copyFrom;
            ID = id;
            CopyCoords(copyFrom);
        }

        public void DefineDepth(Transform root)
        {
            if (root == null) return;
            if (TempTransform == null) return;
            BoneDepth = ADBoneReference.GetDepth(TempTransform, root);
        }

        public void CopyCoords(Transform copyFrom)
        {
            if (copyFrom == null) return;

            LocalPosition = copyFrom.localPosition;
            LocalRotation = copyFrom.localRotation;
            LocalScale = copyFrom.localScale;
        }

        public void ApplyCoords(Transform applyTo = null)
        {
            if (applyTo == null) applyTo = TempTransform;
            if (applyTo == null) return;

            applyTo.localPosition = LocalPosition;
            applyTo.localRotation = LocalRotation;
            applyTo.localScale = LocalScale;
        }

    }
}