using System;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.AnimationTools
{
    public static class AnimationGenerateUtils
    {

        #region Curve Utilties


        public static AnimationCurve ReduceKeyframes(AnimationCurve curve, float maxError)
        {
            Keyframe[] keys = curve.keys;
            int i = 1;

            while (keys.Length > 2 && i < keys.Length - 1)
            {
                Keyframe[] tempK = new Keyframe[keys.Length - 1];

                int t = 0;

                for (int k = 0; k < keys.Length; k++)
                    if (i != k) { tempK[t] = new Keyframe(keys[k].time, keys[k].value, keys[k].inTangent, keys[k].outTangent); t++; }

                AnimationCurve tempCurve = new AnimationCurve();
                tempCurve.keys = tempK;

                float eval = Mathf.Abs(tempCurve.Evaluate(keys[i].time) - keys[i].value);
                float checkBack = keys[i].time + (keys[i - 1].time - keys[i].time) * 0.5f;
                float checkForward = keys[i].time + (keys[i + 1].time - keys[i].time) * 0.5f;

                float previous = Mathf.Abs(tempCurve.Evaluate(checkBack) - curve.Evaluate(checkBack));
                float next = Mathf.Abs(tempCurve.Evaluate(checkForward) - curve.Evaluate(checkForward));

                if (eval < maxError && previous < maxError && next < maxError) keys = tempK; else i++;
            }

            return new AnimationCurve(keys);
        }



        public static void LoopCurve(ref AnimationCurve curve, bool averageBoth = false, float? endTime = null)
        {
            float key0Val = 0f;
            if (curve.keys.Length > 0) key0Val = curve.keys[0].value;

            if (endTime == null) // Loop start and last keyframe
            {
                if (curve.keys.Length == 0)
                {
                    curve.AddKey(new Keyframe(0f, key0Val));
                    curve.AddKey(new Keyframe(1f, key0Val));
                    return;
                }
                else if (curve.keys.Length == 1)
                {
                    curve.AddKey(new Keyframe(Mathf.Max(1f, curve.keys[0].time + 0.5f), key0Val));
                    return;
                }


                float targetVal = key0Val;
                if (averageBoth) targetVal = Mathf.Lerp(key0Val, curve.keys[curve.keys.Length - 1].value, 0.5f);

                curve.MoveKey(0, new Keyframe(curve.keys[0].time, targetVal));
                curve.MoveKey(curve.keys.Length - 1, new Keyframe(curve.keys[curve.keys.Length - 1].time, targetVal));
            }
            else // Shifting last key near to end or adding new key
            {
                float maxTime = endTime.Value;

                if (curve.keys.Length == 0)
                {
                    curve.AddKey(new Keyframe(0f, key0Val));
                    curve.AddKey(new Keyframe(maxTime, key0Val));
                    return;
                }
                else if (curve.keys.Length == 1)
                {
                    curve.AddKey(new Keyframe(maxTime, key0Val));
                    return;
                }

                float targetVal = key0Val;
                if (averageBoth) targetVal = Mathf.Lerp(key0Val, curve.keys[curve.keys.Length - 1].value, 0.5f);

                var key = curve.keys[curve.keys.Length - 1];
                float lastKeyTime = key.time;

                if (lastKeyTime != maxTime)
                {
                    if (lastKeyTime < maxTime)
                    {
                        if (maxTime - lastKeyTime < maxTime * 0.1f)
                        {
                            lastKeyTime = maxTime;
                        }
                    }
                }

                curve.MoveKey(0, new Keyframe(curve.keys[0].time, targetVal));
                curve.MoveKey(curve.keys.Length - 1, new Keyframe(lastKeyTime, targetVal));
            }

        }


        public static void DistrubuteCurveOnTime(ref AnimationCurve curve, float startTime, float endTime)
        {
            float curveStart = curve.keys[0].time;
            float curveEnd = curve.keys[curve.keys.Length - 1].time;

            Keyframe[] evalKeys = new Keyframe[curve.keys.Length];
            curve.keys.CopyTo(evalKeys, 0);
            AnimationCurve refCurve = new AnimationCurve(evalKeys);

            while (curve.keys.Length > 0)
            {
                curve.RemoveKey(curve.keys.Length - 1);
            }
            
            for (int k = 0; k < refCurve.keys.Length; k++)
            //for (int k = refCurve.keys.Length - 1; k >= 0; k--)
            //for (int k = refCurve.keys.Length - 1; k >= 0; k--)
            {
                Keyframe src = refCurve.keys[k];
                Keyframe newK = src;

                newK.time = Mathf.Lerp(startTime, endTime, Mathf.InverseLerp(curveStart, curveEnd, src.time));
                curve.AddKey(newK);
            }

            //for (int k = 0; k < curve.keys.Length; k++)
            //{
            //    Keyframe src = curve.keys[k];
            //    Keyframe newK = src;
            //    newK.time = Mathf.Lerp(startTime, endTime, Mathf.InverseLerp(curve.keys[0].time, curve.keys[curve.keys.Length - 1].time, src.time));

            //    curve.MoveKey(k, newK);
            //}

        }


        #endregion


        #region Coords Utilities


        public static Quaternion EnsureQuaternionContinuity(Quaternion latestRot, Quaternion targetRot, bool normalize = false)
        {
            Quaternion flipped = new Quaternion(-targetRot.x, -targetRot.y, -targetRot.z, -targetRot.w);

            Quaternion midQ = new Quaternion
                (
                Mathf.LerpUnclamped(latestRot.x, targetRot.x, 0.5f),
                Mathf.LerpUnclamped(latestRot.y, targetRot.y, 0.5f),
                Mathf.LerpUnclamped(latestRot.z, targetRot.z, 0.5f),
                Mathf.LerpUnclamped(latestRot.w, targetRot.w, 0.5f)
                );

            Quaternion midQFlipped = new Quaternion
                (
                Mathf.LerpUnclamped(latestRot.x, flipped.x, 0.5f),
                Mathf.LerpUnclamped(latestRot.y, flipped.y, 0.5f),
                Mathf.LerpUnclamped(latestRot.z, flipped.z, 0.5f),
                Mathf.LerpUnclamped(latestRot.w, flipped.w, 0.5f)
                );

            float angle = Quaternion.Angle(latestRot, midQ);
            float angleTreshold = Quaternion.Angle(latestRot, midQFlipped);

            if (normalize)
                return angleTreshold < angle ? flipped.normalized : targetRot.normalized;
            else
                return angleTreshold < angle ? flipped : targetRot;
        }




        #endregion


        #region Animator Extra Utilities

#if UNITY_EDITOR
        static UnityEditor.Animations.AnimatorController _ikHelperAnimController = null;
        public static UnityEditor.Animations.AnimatorController GetStoredHumanoidIKPreviousController { get { return _ikHelperAnimController; } }
        //static RuntimeAnimatorController _ikHelperAnimRController = null;
#endif

        /// <summary>
        /// Applying unity humanoid IK on the scene preview after sampling animation
        /// for better precision for animations editing
        /// </summary>
        public static void UpdateHumanoidIKPreview(Animator mecanim, AnimationClip clip, float time, bool restoreAnimator = true)
        {
#if UNITY_EDITOR

            if (clip != null)
            {
                #region Initialize temporary animator controller

                if (_ikHelperAnimController == null)
                {
                    _ikHelperAnimController = new UnityEditor.Animations.AnimatorController();
                    _ikHelperAnimController.name = "ADesigner-Helper-Controller";
                }

                if (_ikHelperAnimController.layers.Length == 0)
                {
                    var state = new UnityEditor.Animations.AnimatorState();
                    state.motion = null;
                    state.iKOnFeet = true;
                    state.name = "0";

                    UnityEditor.Animations.AnimatorControllerLayer layer = new UnityEditor.Animations.AnimatorControllerLayer();
                    layer.name = "0";
                    layer.iKPass = true;
                    layer.stateMachine = new UnityEditor.Animations.AnimatorStateMachine();
                    layer.stateMachine.AddState(state, Vector3.zero);
                    layer.stateMachine.defaultState = state;

                    _ikHelperAnimController.AddLayer(layer);
                }

                #endregion

                _ikHelperAnimController.layers[0].stateMachine.defaultState.motion = clip;

                RuntimeAnimatorController preController = mecanim.runtimeAnimatorController;

                var preUpd = mecanim.updateMode;

                mecanim.updateMode = AnimatorUpdateMode.UnscaledTime;
                mecanim.Rebind();
                mecanim.runtimeAnimatorController = (RuntimeAnimatorController)_ikHelperAnimController;
                mecanim.Play("0", 0, time / clip.length);
                mecanim.Update(0f);

                mecanim.updateMode = preUpd;

                if (restoreAnimator) mecanim.runtimeAnimatorController = preController;
            }

#endif
        }


        #endregion

    }
}
