using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FIMSpace.AnimationTools
{

    #region Leg Animation Clip Motion Analysis Data Class


    [System.Serializable]
    public class LegAnimationClipMotion
    {
        public bool analyzed = false;
        public AnimationClip targetClip;

        public LegAnimationClipMotion(AnimationClip clip)
        {
            analyzed = false;
            targetClip = clip;
        }


        public List<MotionSample> sampledData;

        /// <summary> Lowest local position values for each axis in this Animation Clip </summary>
        public Vector3 LowestFootCoords;
        /// <summary> Highest local position values for each axis in this Animation Clip </summary>
        public Vector3 HighestFootCoords;
        /// <summary> Difference values between highest and lowest local position in this Animation Clip </summary>
        public Vector3 FootCoordsDiffs;

        /// <summary> Position in which foot starts touching ground in this animation clip </summary>
        public Vector3 startStepFootLocal;
        public int startStepFootLocalIndex;
        public float startStepFootProgress;
        /// <summary> Position in which foot ends touching ground in this animation clip </summary>
        public Vector3 endStepFootLocal;
        public int endStepFootLocalIndex;
        public float endStepFootProgress;

        /// <summary> Approximate velocity of animation basing on foot movement when foot is near to the ground </summary>
        public Vector3 approximateFootPushDirection;
        /// <summary> Just 0,0,-1 or 1,0,0 normalized vector value </summary>
        public Vector3 approximateFootPushDirectionDominant;
        /// <summary> Approximate foot left-right position in animator local space during touching ground </summary>
        public float approximateFootLocalXPosDuringPush;
        public float approximateFootLocalZPosDuringPush;

        /// <summary> Coordinates calculated from T-Pose </summary>
        public Vector3 _footForward = Vector3.forward;
        public Vector3 _footToToes = Vector3.forward;
        public Vector3 _footToToesForw = Vector3.forward;
        public Vector3 _footLocalToGround = Vector3.zero;

        public float _latestToesTesh = 1f;
        public float _latestHeelTesh = 1f;
        public float _latestHoldOnGround = 0f;
        public float _latestFloorOffset = 0f;
        public EAnalyzeMode _latestAnalyzeMode = EAnalyzeMode.HeelFeetTight;

        public float _groundToFootHeight = 0f;

        public Quaternion _initFootRot;
        public Quaternion _initFootLocRot;

        public Quaternion _footRotMapping;

        public AnimationCurve GroundingCurve;

        public enum EAnalyzeMode
        {
            HeelFeetTight,
            HeelFeetWide
        }

        /// <summary> When calling analyze character must be set in T-Pose! </summary>
        public List<MotionSample> AnalyzeClip(Transform rootTr, Transform hips, Transform upperLeg, Transform knee, Transform foot, Transform optionalToes, Transform animator, int samples = 20, float heelTreshold = 1f, float toesTreshold = 1f, float holdOnGround = 0f, bool displayEditorProgressBar = false, bool removeRootMotion = false, float floorOffset = 0f, EAnalyzeMode mode = EAnalyzeMode.HeelFeetTight, int cutFirstFrames = 0, int cutLastFrames = 0)
        {
            analyzed = false;

            Matrix4x4 rootMx = Matrix4x4.TRS(animator.position, animator.rotation, animator.lossyScale);
            Matrix4x4 rootMxRev = rootMx.inverse;

            #region Analyze Helper Definitions

            float toesGroundedTresh = toesTreshold;
            float heelGroundedTresh = heelTreshold;

            _latestToesTesh = toesTreshold;
            _latestHeelTesh = heelTreshold;
            _latestHoldOnGround = holdOnGround;
            _latestAnalyzeMode = mode;
            _latestFloorOffset = floorOffset;

            if (cutFirstFrames < 0) cutFirstFrames = 0;
            if (cutLastFrames < 0) cutLastFrames = 0;

            #endregion

            sampledData = new List<MotionSample>();

            StorePoseBackup(rootTr);

            _initFootRot = foot.rotation;
            _initFootLocRot = foot.localRotation;
            _groundToFootHeight = rootTr.InverseTransformPoint(foot.position).y;

            // Referencing from initial character pose
            //// Referencing from initial animation clip frame
            //targetClip.SampleAnimation(animator.gameObject, 0f);
            float stepOffset = (float)cutFirstFrames / samples;
            samples -= (cutFirstFrames + cutLastFrames);
            float step = 1f / (float)samples;

            if (displayEditorProgressBar) ProgressBar("Preparing foot analysis...", 0f);

            #region Getting Root Motion Bone

            Transform rootMotionBone = null;
            SkinnedMeshRenderer[] skin = animator.GetComponentsInChildren<SkinnedMeshRenderer>();
            if (skin != null)
                if (skin.Length > 0)
                {
                    SkinnedMeshRenderer mostBones = skin.OrderBy(s => s.bones.Length).First();
                    if (mostBones != null) rootMotionBone = mostBones.rootBone;
                }

            if (rootMotionBone == null) rootMotionBone = hips;

            #endregion


            #region Calculating helper directions

            Vector3 localFootToGround, ankleForward, ankleToToes, ankleToToesForward, ankleToToesNorm;

            localFootToGround = new Vector3(foot.position.x, rootTr.position.y, foot.position.z);
            localFootToGround = foot.InverseTransformPoint(localFootToGround);

            ankleForward = foot.InverseTransformPoint(foot.position + rootTr.forward).normalized;

            if (optionalToes)
                ankleToToes = foot.InverseTransformPoint(optionalToes.position);
            else
            {
                ankleToToes = ankleForward * (Vector3.Distance(foot.position, knee.position) + Vector3.Distance(upperLeg.position, knee.position)) * 0.325f;
            }

            if (optionalToes)
            {
                Vector3 mapped = optionalToes.position;
                mapped.y = foot.position.y;
                ankleToToesForward = foot.InverseTransformPoint(mapped);
            }
            else
            {
                ankleToToesForward = ankleToToes;
            }

            ankleToToesNorm = ankleToToes.normalized;

            _footForward = ankleForward;
            _footToToes = ankleToToes;
            _footToToesForw = ankleToToesForward;
            _footLocalToGround = localFootToGround;

            _footRotMapping = Quaternion.FromToRotation(foot.InverseTransformDirection(rootTr.right), Vector3.right);
            _footRotMapping *= Quaternion.FromToRotation(foot.InverseTransformDirection(rootTr.up), Vector3.up);

            #endregion


            #region Sampling Positions


            Vector3 startRootPosition = rootTr.InverseTransformPoint(rootMotionBone.position);
            GroundingCurve = new AnimationCurve();

            for (int i = 0; i < samples; i++)
            {
                if (displayEditorProgressBar) ProgressBar("Sampling leg animation data " + i + " / " + samples, (float)i / (float)samples);

                MotionSample s = new MotionSample();

                // rootMotionBone Root Motion Cancelling -----------------------------------
                targetClip.SampleAnimation(animator.gameObject, stepOffset + step * i * targetClip.length);
                
                if (removeRootMotion)
                {
                    rootMotionBone.localPosition = Vector3.zero;
                    Vector3 hipsLocal = animator.InverseTransformPoint(hips.position);
                    hipsLocal.z = 0f;
                    hips.position = animator.TransformPoint(hipsLocal);
                }

                s.sampledAnkleRoot = rootMxRev.MultiplyPoint(foot.position);

                Vector3 preR = rootMotionBone.position;

                Vector3 refPosition = rootTr.InverseTransformPoint(rootMotionBone.position);
                rootMotionBone.position = rootTr.TransformPoint(refPosition.x, refPosition.y, startRootPosition.z);
                s.sampledRootLocal = rootTr.InverseTransformPoint(rootMotionBone.position);
                s.sampledFootInRMLocal = rootMotionBone.InverseTransformPoint(rootMotionBone.position);
                s.sampledFootInAnimLocal = animator.InverseTransformPoint(rootMotionBone.position);

                // Foot Center grounding position -----------------------------------
                Vector3 sampledFoot = foot.position;
                s.sampledAnkleLocal = rootTr.InverseTransformPoint(sampledFoot); // Foot ankle

                sampledFoot += foot.TransformDirection(localFootToGround);
                s.sampledFootLocal = rootTr.InverseTransformPoint(sampledFoot); // Foot on ground


                // Toes -----------------------------------
                Vector3 sampledToes = GetSamplingToesPoint(foot, toesTreshold);
                // Heel -----------------------------------
                Vector3 sampledHeel = GetSamplingHeelPoint(foot, heelTreshold);

                if (mode == EAnalyzeMode.HeelFeetTight)
                {
                    s.sampledToesLocal = rootTr.InverseTransformPoint(sampledToes);
                    s.sampledHeelLocal = rootTr.InverseTransformPoint(sampledHeel);
                }
                else
                {
                    sampledToes = foot.position;
                    sampledToes += foot.TransformDirection(ankleToToes); // Ankle position offsetted to ground position
                    sampledToes += foot.TransformDirection(ankleToToesForward) * localFootToGround.magnitude * 4f; // Ankle position slightly forwarded
                    s.sampledToesLocal = rootTr.InverseTransformPoint(sampledToes);
                    sampledHeel = foot.position;
                    sampledHeel += foot.TransformDirection(localFootToGround); // Ankle position offsetted to ground position
                    sampledHeel -= foot.TransformDirection(ankleForward) * localFootToGround.magnitude * .8f; // Ankle position slightly forwarded
                    s.sampledHeelLocal = rootTr.InverseTransformPoint(sampledHeel);
                }

                // Knee and upper leg ---------
                s.sampledKneeLocal = rootTr.InverseTransformPoint(knee.position);
                s.sampledUpperLegLocal = rootTr.InverseTransformPoint(upperLeg.position);

                sampledData.Add(s);
            }

            if (displayEditorProgressBar) ProgressBar("Restoring character pose", 1f);

            #region Restoring object pose --------------------------------------------

            AnimationClip getDefaultClip = targetClip;
            string[] defaultKeywords = new string[] { "idle", "stop", "none" };

            Animator anim = animator.GetComponent<Animator>();

            if (anim)
            {
                if (anim.runtimeAnimatorController)
                {
                    for (int i = 0; i < anim.runtimeAnimatorController.animationClips.Length; i++)
                        for (int k = 0; k < defaultKeywords.Length; k++)
                            if (anim.runtimeAnimatorController.animationClips[i].name.ToLower().Contains(defaultKeywords[k]))
                            {
                                getDefaultClip = anim.runtimeAnimatorController.animationClips[i];
                                break;
                            }
                }
                else
                {
                    UnityEngine.Debug.Log("[Error] No Animator Controller in " + anim.name);
                }
            }

            getDefaultClip.SampleAnimation(animator.gameObject, 0f);

            #endregion

            #endregion

            if (displayEditorProgressBar) ProgressBar("Checking collected data", 0f);

            // Lowest / Highest coords ---------------------------------------
            LowestFootCoords = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            HighestFootCoords = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            for (int i = 0; i < samples; ++i)
            {
                if (sampledData[i].sampledFootLocal.x < LowestFootCoords.x) LowestFootCoords.x = sampledData[i].sampledFootLocal.x;
                if (sampledData[i].sampledFootLocal.y < LowestFootCoords.y) LowestFootCoords.y = sampledData[i].sampledFootLocal.y;
                if (sampledData[i].sampledFootLocal.z < LowestFootCoords.z) LowestFootCoords.z = sampledData[i].sampledFootLocal.z;

                if (sampledData[i].sampledFootLocal.x > HighestFootCoords.x) HighestFootCoords.x = sampledData[i].sampledFootLocal.x;
                if (sampledData[i].sampledFootLocal.y > HighestFootCoords.y) HighestFootCoords.y = sampledData[i].sampledFootLocal.y;
                if (sampledData[i].sampledFootLocal.z > HighestFootCoords.z) HighestFootCoords.z = sampledData[i].sampledFootLocal.z;
            }

            FootCoordsDiffs = new Vector3(LowestFootCoords.x - HighestFootCoords.x, LowestFootCoords.y - HighestFootCoords.y, LowestFootCoords.z - HighestFootCoords.z);

            // Analyzing motion with sampled data
            float refScale = GetRefScale(knee, foot);
            float heightTreshold = GetHeightTresholdScale(knee, foot, refScale);

            bool? lastGrounded = null;
            float holding = 0f;
            float ungroundFor = 0f;
            float stepLen = (float)1 / (float)samples;

            approximateFootLocalXPosDuringPush = sampledData[0].sampledFootLocal.x;
            approximateFootLocalZPosDuringPush = sampledData[0].sampledFootLocal.z;
            bool wasAppFootXPushGrnd = false;
            bool wasAppFootZPushGrnd = false;

            // Grounded set to true when foot-toes bottom or heel on ground (with 100% toes grounding was too long)
            // To false when toes and heel over ground position
            for (int i = 0; i < samples; ++i)
            {
                if (displayEditorProgressBar) ProgressBar("Sampling collected data " + i + " / " + samples, (float)i / (float)samples);
                float cycle = (float)i / (float)samples;

                MotionSample s = sampledData[i];

                s.grounded = false;

                if (mode == EAnalyzeMode.HeelFeetTight)
                {
                    // Heel foot position y check
                    if (s.sampledHeelLocal.y < heightTreshold * heelGroundedTresh + floorOffset) s.grounded = true;

                    // Middle foot position y check
                    //float footMidToes = Mathf.LerpUnclamped(s.sampledFootLocal.y, s.sampledToesLocal.y, 0.6f);
                    if (s.sampledToesLocal.y < heightTreshold * toesGroundedTresh + floorOffset) s.grounded = true;
                }
                else
                {
                    if (s.sampledHeelLocal.y < heightTreshold * heelGroundedTresh + floorOffset
                        || s.sampledFootLocal.y < heightTreshold * toesGroundedTresh + floorOffset) s.grounded = true;

                }

                // Hold feature
                if (s.grounded && lastGrounded == true)
                {
                    holding += stepLen;
                    ungroundFor = 0f;
                }
                else if (holding > 0f)
                {
                    if (holdOnGround > 0f)
                        if (holding < holdOnGround)
                        {
                            holding += stepLen;
                            s.grounded = true;
                        }
                }
                else
                {
                    ungroundFor += stepLen;
                    if (ungroundFor > stepLen * 3)
                        if (holding > holdOnGround) holding = 0f;
                }


                // Detected grounding
                if (lastGrounded != true && s.grounded == true)
                {
                    startStepFootLocal = s.sampledFootLocal;
                    startStepFootLocalIndex = i;
                    startStepFootProgress = (float)i / (float)(samples);
                }
                else
                    // Detected unground
                    if (lastGrounded == true && s.grounded == false)
                {
                    endStepFootLocal = s.sampledFootLocal;
                    endStepFootLocalIndex = i;
                    endStepFootProgress = (float)i / (float)(samples);
                }

                lastGrounded = s.grounded;

                if (s.grounded)
                {
                    float preCycle = cycle - 2f / (float)samples;
                    if (preCycle < 0f) preCycle += 1f;
                    Vector3 diff = GetFootLocalPosition(cycle) - GetFootLocalPosition(preCycle);
                    diff.y = 0f;
                    approximateFootPushDirection += diff;

                    #region Approximate Local Foot X when grounded

                    if (wasAppFootXPushGrnd == false)
                    {
                        wasAppFootXPushGrnd = true;
                        approximateFootLocalXPosDuringPush = s.sampledFootLocal.x;
                    }
                    else
                    {
                        approximateFootLocalXPosDuringPush = Mathf.Lerp(approximateFootLocalXPosDuringPush, s.sampledFootLocal.x, 0.5f);
                    }

                    #endregion

                    #region Approximate Local Foot Z when grounded

                    if (wasAppFootZPushGrnd == false)
                    {
                        wasAppFootZPushGrnd = true;
                        approximateFootLocalZPosDuringPush = s.sampledFootLocal.z;
                    }
                    else
                    {
                        approximateFootLocalZPosDuringPush = Mathf.Lerp(approximateFootLocalZPosDuringPush, s.sampledFootLocal.z, 0.5f);
                    }

                    #endregion


                }

                approximateFootPushDirection.Normalize();
                approximateFootPushDirectionDominant = FIMSpace.FVectorMethods.ChooseDominantAxis(approximateFootPushDirection);

                // Checking for leg swinging
                Vector3 pre = GetFootLocalPosition(cycle - (step));
                Vector3 curr = GetFootLocalPosition(cycle);

                if (curr.z > pre.z) s.swingForwards = true;


                float refSc = refScale;
                if (refSc < 0.4f) refSc = 0.4f;
                float val = Mathf.Clamp(s.sampledFootLocal.y / refSc, 0.2f, 1f);
                if (s.grounded) val = 0f;
                GroundingCurve.AddKey(cycle, val);

            }



            List<Keyframe> optimizedCurve = new List<Keyframe>();

            optimizedCurve.Add(GroundingCurve.keys[0]);
            for (int i = 1; i < GroundingCurve.keys.Length - 1; i++)
            {
                if (GroundingCurve.keys[i].value == 0f && GroundingCurve.keys[i + 1].value > 0f)
                {
                    optimizedCurve.Add(GroundingCurve.keys[i]);
                    continue;
                }

                if (GroundingCurve.keys[i - 1].value > 0f && GroundingCurve.keys[i].value == 0f)
                {
                    optimizedCurve.Add(GroundingCurve.keys[i]);
                    continue;
                }

                if (GroundingCurve.keys[i - 1].value != GroundingCurve.keys[i].value)
                    optimizedCurve.Add(GroundingCurve.keys[i]);
            }

            optimizedCurve.Add(GroundingCurve.keys[GroundingCurve.keys.Length - 1]);
            GroundingCurve = new AnimationCurve(optimizedCurve.ToArray());

            for (int i = 0; i < GroundingCurve.keys.Length; i++)
            {
                GroundingCurve.SmoothTangents(i, 0.2f);
            }

            GroundingCurve = AnimationGenerateUtils.ReduceKeyframes(GroundingCurve, 0.022f);

            // Prediction when leg is going to step down ---------------------------------------
            for (int i = 0; i < samples; ++i)
            {
                if (displayEditorProgressBar) ProgressBar("Defining Additional Data " + i + " / " + samples, (float)i / (float)samples);

                MotionSample s = sampledData[i];
                if (s.grounded) continue;

                if (s.sampledFootLocal.z < 0) // if foot .z is behind origin then we set prediction state only when foot swings forward
                {
                    if (s.swingForwards)
                    {
                        if (s.sampledFootLocal.z > LowestFootCoords.z / 2f) s.predictState = true;
                    }
                }
                else // When foot .z is in front of origin
                    s.predictState = true;
            }

            if (displayEditorProgressBar) ProgressBar("Finalizing", 1f);

            RestorePoseBackup();

            if (displayEditorProgressBar) ProgressBar("", 1.1f);

            analyzed = true;

            return sampledData;
        }

        public float GetRefScale(Transform knee, Transform foot)
        {
            return Vector3.Distance(foot.position, knee.position) * 0.1f;
        }

        public float GetTresholdLength(Transform knee, Transform foot, float treshold, float amplification = 1f)
        {
            float refSc = GetHeightTresholdScale(knee, foot) + LowestFootCoords.y;
            return GetAmplified02Range(1f - treshold, amplification) * refSc;
        }

        public float GetHeightTresholdScale(Transform knee, Transform foot, float? refScale = null)
        {
            float refs;
            if (refScale == null) refs = GetRefScale(knee, foot); else refs = refScale.Value;
            return refs + LowestFootCoords.y;
        }


        #region Requesting positions

        public Vector3 GetToesFootLocalPosition(float progress, float toesToFoot = 0.5f)
        {
            return Vector3.LerpUnclamped(GetToesLocalPosition(progress), GetFootLocalPosition(progress), toesToFoot);
        }

        public Vector3 GetToesLocalPosition(float progress)
        {
            RefreshInterpolationIndexes(progress);
            return Vector3.LerpUnclamped(sampledData[i_lowerIndex].sampledToesLocal, sampledData[i_higherIndex].sampledToesLocal, i_progress);
        }

        public Vector3 GetAnkleLocalPosition(float progress)
        {
            RefreshInterpolationIndexes(progress);
            return Vector3.LerpUnclamped(sampledData[i_lowerIndex].sampledAnkleLocal, sampledData[i_higherIndex].sampledAnkleLocal, i_progress);
        }

        public MotionSample GetSampleInProgress(float progress, bool higher = true)
        {
            RefreshInterpolationIndexes(progress);

            if (higher)
                return sampledData[i_higherIndex];
            else
                return sampledData[i_lowerIndex];
        }

        public Vector3 GetFootLocalPositionInAnimator(float progress)
        {
            progress = RoundCycle(progress);
            RefreshInterpolationIndexes(progress);
            return Vector3.LerpUnclamped(sampledData[i_lowerIndex].sampledFootInAnimLocal, sampledData[i_higherIndex].sampledFootInAnimLocal, i_progress);
        }

        public Vector3 GetFootLocalPositionInRootMotion(float progress)
        {
            progress = RoundCycle(progress);
            RefreshInterpolationIndexes(progress);
            return Vector3.LerpUnclamped(sampledData[i_lowerIndex].sampledFootInRMLocal, sampledData[i_higherIndex].sampledFootInRMLocal, i_progress);
        }

        public Vector3 GetFootLocalPosition(float progress)
        {
            progress = RoundCycle(progress);
            RefreshInterpolationIndexes(progress);
            return Vector3.LerpUnclamped(sampledData[i_lowerIndex].sampledFootLocal, sampledData[i_higherIndex].sampledFootLocal, i_progress);
        }

        public Vector3 GetHeelLocalPosition(float progress)
        {
            RefreshInterpolationIndexes(progress);
            return Vector3.LerpUnclamped(sampledData[i_lowerIndex].sampledHeelLocal, sampledData[i_higherIndex].sampledHeelLocal, i_progress);
        }

        public Vector3 GetUpperLegLocalPosition(float progress)
        {
            RefreshInterpolationIndexes(progress);
            return Vector3.LerpUnclamped(sampledData[i_lowerIndex].sampledUpperLegLocal, sampledData[i_higherIndex].sampledUpperLegLocal, i_progress);
        }

        public Vector3 GetRootLocalPosition(float progress)
        {
            RefreshInterpolationIndexes(progress);
            return Vector3.LerpUnclamped(sampledData[i_lowerIndex].sampledRootLocal, sampledData[i_higherIndex].sampledRootLocal, i_progress);
        }

        public Vector3 GetKneeLocalPosition(float progress)
        {
            RefreshInterpolationIndexes(progress);
            return Vector3.LerpUnclamped(sampledData[i_lowerIndex].sampledKneeLocal, sampledData[i_higherIndex].sampledKneeLocal, i_progress);
        }

        #endregion


        #region Requesting info

        public bool GroundedIn(float progress)
        {
            //RefreshInterpolationIndexes(progress);

            if (sampledData == null) return false;
            if (sampledData.Count == 0) return false;
            //if (progress > 1f) progress -= 1f;
            //if (progress < 0f) progress += 1f;

            return GroundingCurve.Evaluate(progress) < 0.05f;

            //if (i_progress > 0.5f)
            //    return sampledData[i_higherIndex].grounded;
            //else
            //    return sampledData[i_lowerIndex].grounded;
        }

        public bool PredictIn(float progress)
        {
            RefreshInterpolationIndexes(progress);

            if (i_progress > 0.5f)
                return sampledData[i_higherIndex].predictState;
            else
                return sampledData[i_lowerIndex].predictState;
        }

        public bool GetSwingingForward(float progress)
        {
            RefreshInterpolationIndexes(progress);

            if (i_progress > 0.5f)
                return sampledData[i_higherIndex].swingForwards;
            else
                return sampledData[i_lowerIndex].swingForwards;
        }


        public float GetAmplified02Range(float enterValue, float amplifyAfter1UpTo2 = 5f)
        {
            if (enterValue <= 1f) return enterValue;
            float a = -(1f - enterValue);
            return enterValue + a * amplifyAfter1UpTo2;
        }




        #region Interpolation Preparations

        private int i_gameFrame = -1;
        public int i_lowerIndex;
        public int i_higherIndex;
        public float i_progress;
        public float i_forProgress;
        /// <summary>
        /// After refreshing use i_lowerIndex and i_higherIndex for interpolation samples
        /// </summary>
        public void RefreshInterpolationIndexes(float progress)
        {
            if (sampledData == null) return;
            if (sampledData.Count == 0) return;

            if (progress == i_forProgress)
                if (i_gameFrame == Time.frameCount) return; // Limiting calculations if not needed to recompute in game frame

            i_gameFrame = Time.frameCount;
            i_forProgress = progress;

            i_progress = RoundCycle(i_forProgress);
            i_higherIndex = Mathf.CeilToInt(i_progress * sampledData.Count);

            if (i_higherIndex > sampledData.Count - 1)
            {
                i_lowerIndex = sampledData.Count - 1; i_higherIndex = 0;
                i_progress = Mathf.InverseLerp(i_lowerIndex, sampledData.Count, i_progress * sampledData.Count);
            }
            else
            {
                i_lowerIndex = Mathf.FloorToInt(i_progress * sampledData.Count);
                i_progress = Mathf.InverseLerp(i_lowerIndex, i_higherIndex, i_progress * sampledData.Count);
            }
        }

        #endregion


        #endregion


        void ProgressBar(string text, float prog)
        {
#if UNITY_EDITOR

            if (string.IsNullOrEmpty(text) || prog > 1f)
            {
                UnityEditor.EditorUtility.ClearProgressBar();
                return;
            }

            UnityEditor.EditorUtility.DisplayProgressBar("Leg Motion Analysis...", text, prog);
#endif
        }

        /// <summary>
        /// Rounding cycle to value 0 to 1
        /// When progress is  1.5 then returns 0.5 ... When progress is -0.2 then returns 0.8
        /// </summary>
        public static float RoundCycle(float cycleProgress)
        {
            return cycleProgress - Mathf.Floor(cycleProgress);
        }


        public LegAnimationClipMotion GetCopy()
        {
            return base.MemberwiseClone() as LegAnimationClipMotion;
        }


        [System.Serializable]
        public class MotionSample
        {

            #region Sampling positions

            public Vector3 sampledToesLocal;
            public Vector3 sampledAnkleRoot;
            public Vector3 sampledFootLocal;
            public Vector3 sampledAnkleLocal;
            public Vector3 sampledHeelLocal;
            public Vector3 sampledKneeLocal;
            public Vector3 sampledUpperLegLocal;
            public Vector3 sampledRootLocal;
            public Vector3 sampledFootInRMLocal;
            public Vector3 sampledFootInAnimLocal;

            #endregion

            /// <summary> Foot touches ground level </summary>
            public bool grounded = false;
            /// <summary> Foot swings forwards but not touching ground </summary>
            public bool predictState = false;
            /// <summary> Swinging forwards instant after pushing back from ground</summary>
            public bool swingForwards = false;
            /// <summary> Swinging forwards with some delay after pushing back from ground</summary>
            //public bool swingForwardsToStep = false;

            public MotionSample GetCopy()
            {
                return base.MemberwiseClone() as MotionSample;
            }
        }


        #region Transforms Pose Restore

        static void StorePoseBackup(Transform rootTransform)
        {
            poseBackup.Clear();

            foreach (Transform t in rootTransform.GetComponentsInChildren<Transform>())
            {
                TransformsBackup b = new TransformsBackup();
                b.t = t;
                b.localPos = t.localPosition;
                b.localRot = t.localRotation;
                b.localScale = t.localScale;
                poseBackup.Add(b);
            }
        }

        static void RestorePoseBackup()
        {
            for (int i = poseBackup.Count - 1; i >= 0; i--) poseBackup[i].Restore();

            poseBackup.Clear();
        }

        public static List<TransformsBackup> poseBackup = new List<TransformsBackup>();
        public struct TransformsBackup
        {
            public Transform t;
            public Vector3 localPos;
            public Quaternion localRot;
            public Vector3 localScale;

            public void Restore()
            {
                if (t == null) return;
                t.localPosition = localPos;
                t.localRotation = localRot;
                t.localScale = localScale;
            }
        }


        public Vector3 GetSamplingToesPoint(Transform foot, float tresh)
        {
            Vector3 sampledToes = foot.position;
            sampledToes += foot.TransformDirection(_footLocalToGround); // Ankle position offsetted to ground position
            sampledToes += foot.TransformDirection(_footToToesForw) * _footLocalToGround.magnitude * (4f - Mathf.LerpUnclamped(0f, 0.1f, tresh)); // Ankle position slightly forwarded
            return sampledToes;
        }

        public Vector3 GetSamplingHeelPoint(Transform foot, float tresh)
        {
            Vector3 sampledHeel = foot.position;
            sampledHeel += foot.TransformDirection(_footLocalToGround); // Ankle position offsetted to ground position
            sampledHeel -= foot.TransformDirection(_footForward) * _footLocalToGround.magnitude * (.6f - Mathf.Lerp(0f, 2f, tresh)); // Ankle position slightly forwarded
            return sampledHeel;
        }


        #endregion

    }


    #endregion


}
