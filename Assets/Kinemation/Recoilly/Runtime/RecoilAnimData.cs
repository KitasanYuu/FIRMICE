using UnityEngine;

namespace Kinemation.Recoilly
{
    [CreateAssetMenu(fileName = "NewRecoilAnimData", menuName = "RecoilAnimData")]
    public class RecoilAnimData : ScriptableObject
    {
        [Header("Rotation Targets")]
        public Vector2 pitch;
        public Vector4 roll;
        public Vector4 yaw;

        [Header("Translation Targets")] 
        public Vector2 kickback;
        public Vector2 kickUp;
        public Vector2 kickRight;
    
        [Header("Aiming Multipliers")]
        public Vector3 aimRot;
        public Vector3 aimLoc;
    
        [Header("Auto/Burst Settings")]
        public Vector3 smoothRot;
        public Vector3 smoothLoc;

        public Vector3 extraRot;
        public Vector3 extraLoc;
    
        [Header("Noise Layer")]
        public Vector2 noiseX;
        public Vector2 noiseY;

        public Vector2 noiseAccel;
        public Vector2 noiseDamp;
    
        public float noiseScalar = 1f;
    
        [Header("Pushback Layer")]
        public float pushAmount = 0f;
        public float pushAccel;
        public float pushDamp;

        [Header("Misc")]
        public bool smoothRoll;
        public float playRate;
    
        [Header("Recoil Curves")]
        public RecoilCurves recoilCurves;
    }
}
