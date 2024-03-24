using System;
using UnityEngine;

namespace Kinemation.Recoilly.Runtime
{
    [Serializable]
    public struct LocRot
    {
        public Vector3 position;
        public Quaternion rotation;
    }
    
    [Serializable]
    public struct SpringData
    {
        public float stiffness;
        public float criticalDamping;
        public float speed;
        public float mass;
        public float maxValue;
        [NonSerialized] public float error;
        [NonSerialized] public float velocity;

        public SpringData(float stiffness, float damping, float speed, float mass)
        {
            this.stiffness = stiffness;
            criticalDamping = damping;
            this.speed = speed;
            this.mass = mass;

            error = 0f;
            velocity = 0f;
            maxValue = 0f;
        }
        
        public SpringData(float stiffness, float damping, float speed)
        {
            this.stiffness = stiffness;
            criticalDamping = damping;
            this.speed = speed;
            mass = 1f;

            error = 0f;
            velocity = 0f;
            maxValue = 0f;
        }
    }
    
    [Serializable]
    public struct VectorSpringData
    {
        public SpringData x;
        public SpringData y;
        public SpringData z;
        public Vector3 scale;
    }

    public static class AnimToolkitLib
    {
        private const float FloatMin = 1e-10f;
        private const float SqrEpsilon = 1e-8f;
        
        public static float SpringInterp(float current, float target, ref SpringData springData)
        {
            float interpSpeed = Mathf.Min(Time.deltaTime * springData.speed, 1f);
            target = Mathf.Clamp(target, -springData.maxValue, springData.maxValue);
            
            if (!Mathf.Approximately(interpSpeed, 0f))
            {
                if (!Mathf.Approximately(springData.mass, 0f))
                {
                    float damping = 2 * Mathf.Sqrt(springData.mass * springData.stiffness) * springData.criticalDamping;
                    float error = target - current;
                    float errorDeriv = (error - springData.error);
                    springData.velocity +=
                        (error * springData.stiffness * interpSpeed + errorDeriv * damping) /
                        springData.mass;
                    springData.error = error;

                    float value = current + springData.velocity * interpSpeed;
                    return value;
                }
            
                return target;
            }

            return current;
        }
        
        public static Vector3 SpringInterp(Vector3 current, Vector3 target, ref VectorSpringData springData)
        {
            Vector3 final = Vector3.zero;

            final.x = SpringInterp(current.x, target.x * springData.scale.x, ref springData.x);
            final.y = SpringInterp(current.y, target.y * springData.scale.y, ref springData.y);
            final.z = SpringInterp(current.z, target.z * springData.scale.z, ref springData.z);

            return final;
        }

        // Frame-rate independent interpolation
        public static float Glerp(float a, float b, float speed)
        {
            return Mathf.Lerp(a, b, 1 - Mathf.Exp(-speed * Time.deltaTime));
        }
        
        public static float GlerpLayer(float a, float b, float speed)
        {
            return Mathf.Approximately(speed, 0f)
                ? b
                : Mathf.Lerp(a, b, 1 - Mathf.Exp(-speed * Time.deltaTime));
        }

        public static Vector3 Glerp(Vector3 a, Vector3 b, float speed)
        {
            return Vector3.Lerp(a, b, 1 - Mathf.Exp(-speed * Time.deltaTime));
        }

        public static Quaternion Glerp(Quaternion a, Quaternion b, float speed)
        {
            return Quaternion.Slerp(a, b, 1 - Mathf.Exp(-speed * Time.deltaTime));
        }

        public static void RotateInBoneSpace(Quaternion target, Transform boneToRotate, Vector3 rotationAmount)
        {
            var headRot = boneToRotate.rotation;
            var headToMesh = Quaternion.Inverse(target) * headRot;
            var headOffsetRot = target * Quaternion.Euler(rotationAmount);

            var finalRot = headOffsetRot * headToMesh;

            boneToRotate.rotation = finalRot;
        }

        public static void RotateInBoneSpace(Quaternion target, Transform boneToRotate, Quaternion rotationAmount)
        {
            var headRot = boneToRotate.rotation;
            var headToMesh = Quaternion.Inverse(target) * headRot;
            var headOffsetRot = target * rotationAmount;

            var finalRot = headOffsetRot * headToMesh;

            boneToRotate.rotation = finalRot;
        }

        public static void MoveInBoneSpace(Transform target, Transform boneToMove, Vector3 offsetMeshSpace)
        {
            var root = target.transform;
            Vector3 offset = root.TransformPoint(offsetMeshSpace);
            offset -= root.position;

            boneToMove.position += offset;
        }
        
        // Adapted from Two Bone IK constraint, Unity Animation Rigging package
        public static void SolveTwoBoneIK(
            Transform root,
            Transform mid,
            Transform tip,
            Transform target,
            Transform hint,
            float posWeight,
            float rotWeight,
            float hintWeight
        )
        {
            Vector3 aPosition = root.position;
            Vector3 bPosition = mid.position;
            Vector3 cPosition = tip.position;
            Vector3 tPosition = Vector3.Lerp(cPosition, target.position, posWeight);
            Quaternion tRotation = Quaternion.Lerp(tip.rotation, target.rotation, rotWeight);
            bool hasHint = hint != null && hintWeight > 0f;

            Vector3 ab = bPosition - aPosition;
            Vector3 bc = cPosition - bPosition;
            Vector3 ac = cPosition - aPosition;
            Vector3 at = tPosition - aPosition;

            float abLen = ab.magnitude;
            float bcLen = bc.magnitude;
            float acLen = ac.magnitude;
            float atLen = at.magnitude;

            float oldAbcAngle = TriangleAngle(acLen, abLen, bcLen);
            float newAbcAngle = TriangleAngle(atLen, abLen, bcLen);

            // Bend normal strategy is to take whatever has been provided in the animation
            // stream to minimize configuration changes, however if this is collinear
            // try computing a bend normal given the desired target position.
            // If this also fails, try resolving axis using hint if provided.
            Vector3 axis = Vector3.Cross(ab, bc);
            if (axis.sqrMagnitude < SqrEpsilon)
            {
                axis = hasHint ? Vector3.Cross(hint.position - aPosition, bc) : Vector3.zero;

                if (axis.sqrMagnitude < SqrEpsilon)
                    axis = Vector3.Cross(at, bc);

                if (axis.sqrMagnitude < SqrEpsilon)
                    axis = Vector3.up;
            }

            axis = Vector3.Normalize(axis);

            float a = 0.5f * (oldAbcAngle - newAbcAngle);
            float sin = Mathf.Sin(a);
            float cos = Mathf.Cos(a);
            Quaternion deltaR = new Quaternion(axis.x * sin, axis.y * sin, axis.z * sin, cos);
            mid.rotation = deltaR * mid.rotation;
            
            cPosition = tip.position;
            ac = cPosition - aPosition;
            root.rotation = FromToRotation(ac, at) * root.rotation;

            if (hasHint)
            {
                float acSqrMag = ac.sqrMagnitude;
                if (acSqrMag > 0f)
                {
                    bPosition = mid.position;
                    cPosition = tip.position;
                    ab = bPosition - aPosition;
                    ac = cPosition - aPosition;

                    Vector3 acNorm = ac / Mathf.Sqrt(acSqrMag);
                    Vector3 ah = hint.position - aPosition;
                    Vector3 abProj = ab - acNorm * Vector3.Dot(ab, acNorm);
                    Vector3 ahProj = ah - acNorm * Vector3.Dot(ah, acNorm);

                    float maxReach = abLen + bcLen;
                    if (abProj.sqrMagnitude > (maxReach * maxReach * 0.001f) && ahProj.sqrMagnitude > 0f)
                    {
                        Quaternion hintR = FromToRotation(abProj, ahProj);
                        hintR.x *= hintWeight;
                        hintR.y *= hintWeight;
                        hintR.z *= hintWeight;
                        hintR = NormalizeSafe(hintR);
                        root.rotation = hintR * root.rotation;
                    }
                }
            }

            tip.rotation = tRotation;
        }
        
        private static float TriangleAngle(float aLen, float aLen1, float aLen2)
        {
            float c = Mathf.Clamp((aLen1 * aLen1 + aLen2 * aLen2 - aLen * aLen) / (aLen1 * aLen2) / 2.0f, -1.0f, 1.0f);
            return Mathf.Acos(c);
        }

        private static Quaternion FromToRotation(Vector3 from, Vector3 to)
        {
            float theta = Vector3.Dot(from.normalized, to.normalized);
            if (theta >= 1f)
                return Quaternion.identity;

            if (theta <= -1f)
            {
                Vector3 axis = Vector3.Cross(from, Vector3.right);
                if (axis.sqrMagnitude == 0f)
                    axis = Vector3.Cross(from, Vector3.up);

                return Quaternion.AngleAxis(180f, axis);
            }

            return Quaternion.AngleAxis(Mathf.Acos(theta) * Mathf.Rad2Deg, Vector3.Cross(from, to).normalized);
        }

        private static Quaternion NormalizeSafe(Quaternion q)
        {
            float dot = Quaternion.Dot(q, q);
            if (dot > FloatMin)
            {
                float rsqrt = 1.0f / Mathf.Sqrt(dot);
                return new Quaternion(q.x * rsqrt, q.y * rsqrt, q.z * rsqrt, q.w * rsqrt);
            }

            return Quaternion.identity;
        }
    }
}