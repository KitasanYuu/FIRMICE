using System;
using UnityEngine;

namespace FIMSpace.FTools
{
    public partial class FimpIK_Arm
    {
        [System.Serializable]
        public class IKBone
        {
            // Base variables -----------------------
            public IKBone Child { get; private set; }
            public Transform transform { get; protected set; }
            public float sqrMagn = 0.1f;
            public float BoneLength = 0.1f;
            public float MotionWeight = 1f;

            public Vector3 InitialLocalPosition;
            public Quaternion InitialLocalRotation;
            public Quaternion LastKeyLocalRotation;


            // Arm ik specific -----------------------
            [SerializeField] private Quaternion targetToLocalSpace;
            [SerializeField] private Vector3 defaultLocalPoleNormal;
            public Vector3 GetDefaultPoleNormal() { return defaultLocalPoleNormal; }

            public Vector3 right { get; private set; }
            public Vector3 up { get; private set; }
            public Vector3 forward { get; private set; }

            public Vector3 srcPosition { get; private set; }
            public Quaternion srcRotation { get; private set; }


            public IKBone(Transform t)
            {
                if (t == null) return;
                transform = t;
                InitialLocalPosition = transform.localPosition;
                InitialLocalRotation = transform.localRotation;
                LastKeyLocalRotation = t.localRotation;
            }


            public virtual void SetChild(IKBone child)
            {
                if (child.transform == null) return;

                Child = child;
                sqrMagn = (child.transform.position - transform.position).sqrMagnitude;
                BoneLength = (child.transform.position - transform.position).magnitude;
            }


            public Vector3 Dir(Vector3 local)
            {
                return transform.TransformDirection(local);
            }


            public void Init(Transform root, Vector3 childPosition, Vector3 orientationNormal)
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
                if (transform == null) return;
                Vector3 dir = childPosition - transform.position;

                Quaternion defaultTargetRotation;
                if (dir == Vector3.zero) defaultTargetRotation = Quaternion.identity;
                else
                    defaultTargetRotation = Quaternion.LookRotation(dir, orientationNormal);

                targetToLocalSpace = RotationToLocal(transform.rotation, defaultTargetRotation);
                defaultLocalPoleNormal = Quaternion.Inverse(transform.rotation) * orientationNormal;
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
            { return transform.rotation * (defaultLocalPoleNormal); }

        }

    }

}