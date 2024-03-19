using System;
using UnityEngine;

namespace FIMSpace.FTools
{
   
    public partial class FimpIK_Arm 
    {
        private Vector3 lastIKBasePosition;
        private Quaternion lastIKBaseRotation;

        public Vector3 IKTargetPosition;
        public Quaternion IKTargetRotation;

        /// <summary> Length of whole bones chain (squared and not scalled) </summary>
        public float FullLength { get; protected set; }

        public bool Initialized { get; set; }

        public void PreCalibrate(float blend = 1f)
        {
            IKBone child = IKBones[0];

            if (blend >= 1f)
            {
                while (child != null)
                {
                    child.transform.localRotation = child.InitialLocalRotation;
                    child = child.Child;
                }
            }
            else
            {
                while (child != null)
                {
                    child.transform.localRotation = Quaternion.LerpUnclamped(child.transform.localRotation, child.InitialLocalRotation, blend);
                    child = child.Child;
                }
            }

            RefreshScaleReference();
        }

        private float sd_ikBlend = 0f;
        public void User_SmoothIKBlend(float target, float duration, float delta, float maxSpeed = 1000f)
        {
            IKWeight = Mathf.SmoothDamp(IKWeight, target, ref sd_ikBlend, duration, maxSpeed, delta);
        }


        private Vector3 sd_ikTargetPosition = Vector3.zero;
        public void User_SmoothPositionTowards(Vector3 newIKPos, float duration, float delta, float maxSpeed = 1000f)
        {
            IKTargetPosition = Vector3.SmoothDamp(IKTargetPosition, newIKPos, ref sd_ikTargetPosition, duration, maxSpeed, delta);
        }

        public Vector3 GetHintDefaultPosition()
        {
            return ForeArmIKBone.transform.position + ForeArmIKBone.transform.rotation * UpperArmIKBone.GetDefaultPoleNormal();
        }
    }
}
