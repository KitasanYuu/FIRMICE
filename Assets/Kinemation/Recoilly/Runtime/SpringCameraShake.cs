using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Kinemation.Recoilly.Runtime
{
    [Serializable]
    public struct SpringShakeProfile
    {
        [SerializeField] public VectorSpringData springData;
        [SerializeField] public float dampSpeed;
        [SerializeField] public Vector2 pitch;
        [SerializeField] public Vector2 yaw;
        [SerializeField] public Vector2 roll;

        public Vector3 GetRandomTarget()
        {
            return new Vector3(Random.Range(pitch.x, pitch.y), Random.Range(yaw.x, yaw.y),
                Random.Range(roll.x, roll.y));
        }
    }
    
    public class SpringCameraShake : MonoBehaviour
    {
        [SerializeField] private SpringShakeProfile shakeProfile;
        private Vector3 _dampedTarget;
        private Vector3 _target;

        // Should be applied after camera stabilization logic
        private void LateUpdate()
        {
            // Interpolate
            _target = AnimToolkitLib.SpringInterp(_target, Vector3.zero, ref shakeProfile.springData);
            _dampedTarget = AnimToolkitLib.Glerp(_dampedTarget, _target, shakeProfile.dampSpeed);
            
            transform.localRotation *= Quaternion.Euler(_dampedTarget);
        }

        public void PlayCameraShake()
        {
            _target = shakeProfile.GetRandomTarget();
        }
    }
}
