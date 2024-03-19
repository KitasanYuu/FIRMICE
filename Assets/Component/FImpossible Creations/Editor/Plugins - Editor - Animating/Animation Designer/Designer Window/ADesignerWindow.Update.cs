using UnityEditor;
using UnityEngine;

namespace FIMSpace.AnimationTools
{
    public partial class AnimationDesignerWindow : EditorWindow
    {
        bool playPreview = false;
        float playbackSpeed = 1f;

        float animationElapsed = 0f;
        float animationProgress = 0f;
        public float LastAnimationProgress { get { return animationProgress; } }

        float animationProgressForEval = 0f;
        float timeElapsed = 0f;
        float timeSin = 0f;
        float timeSin01 = 0f;
        float timePerlin = 0f;

        GameObject _latest_SceneObj = null;
        int _latest_SceneObjRepaintFor = -1;

        private float _play_sett_startTime = 0f;
        private float _play_sett_stopTime = 0f;

        /// <summary> Is it a dream? Or Unity seriously made it so unnceccesary overcomplicated???? </summary>
        private float _play_humanoidPreviewCorrection = 0f;
        private float _play_humanoidPreviewCorrectionMul = 1f;

        private float _play_mod_clipOrigLen = 1f;
        private float _play_mod_clipLenMul = 1f;
        private float _play_mod_clipStartTrim = 0f;
        private float _play_mod_trimmedStart = 0f;
        //private float _play_mod_trimmedEnd = 0f;
        private float _play_mod_clipEndTrim = 0f;
        private float _play_mod_dtMod = 1f;

        /// <summary> Clip length in seconds after export modifications (with trimming) </summary>
        private float _play_mod_Length = 1f;
        /// <summary> Clip length in seconds trimmed, without duration multiplier </summary>
        private float _play_mod_TrimmedLength = 1f;
        /// <summary> Clip length in seconds after export modifications </summary>
        private float _play_mod_Length_PlusJustMul = 1f;
        public float GetClipLengthModified()
        {
            if (!TargetClip) return 1f;

            _play_mod_clipLenMul = GetClipDurationMul();
            _play_mod_clipStartTrim = GetClipLeftTrim();
            _play_mod_clipEndTrim = GetClipRightTrim(false);

            _play_mod_TrimmedLength = _play_mod_clipOrigLen - (_play_mod_clipOrigLen * (_play_mod_clipStartTrim + _play_mod_clipEndTrim));

            return _play_mod_TrimmedLength * _play_mod_clipLenMul;
        }

        private void Update()
        {
            if (!wasSceneRepaint) Repaint();
            wasSceneRepaint = false;

            if (_switchingReferences) return;
            if (_serializationChanges) return;


            #region Forcing window repaint when switching scene selections

            if (_latest_SceneObj != Selection.activeGameObject)
            {
                _latest_SceneObj = Selection.activeGameObject;
                _latest_SceneObjRepaintFor = 10;
            }

            if (_latest_SceneObjRepaintFor > 0)
            {
                _latest_SceneObjRepaintFor -= 1;
                if (!_serializationChanges) Repaint();
            }

            #endregion


            #region Return Conditions + TPoseRestore

            if (!S) return;
            if (!IsReady) return;
            if (!S.SkelRootBone) return;
            if (!S.ReferencePelvis) return;
            if (!TargetClip)
            {
                if (!restoredTPose)
                {
                    S.RestoreTPose();
                    restoredTPose = true;
                }

                return;
            }

            if (isBaking) return;

            restoredTPose = false;

            #endregion


            _dtForcingUpdate = true;


            UpdateEditorDeltaTime();


            UtilsUpdate();


            timeElapsed += dt;
            timeSin = Mathf.Sin(timeElapsed * 4f);
            timeSin01 = (timeSin + 1f) * 0.5f;
            timePerlin = Mathf.PerlinNoise(timeElapsed * 5f, 1000f + timeElapsed * 6f);


            if (debugTabFoldout) return;

            RefreshClipLengthModValues();

            if (TargetClip)
            {
                if (animationElapsed > _play_mod_Length * 2f)
                {
                    animationElapsed = 0f;
                    triggerLoopRestoreForNonLooped = true;
                }
            }

            if (playPreview)
            {
                animationElapsed += dt * playbackSpeed;

                if (animationElapsed >= _play_mod_Length)
                {
                    animationElapsed -= _play_mod_Length;
                    triggerLoopRestoreForNonLooped = true;
                }
            }

            CheckComponentsInitialization(false);
            SampleCurrentAnimation();

            if (updateDesigner)
            {
                UpdateSimulationAfterAnimators(null);
                LateUpdateSimulation();
            }

            SectionsUpdateLoop();

        }


        void SectionsUpdateLoop()
        {
            switch (Category)
            {
                case ECategory.Setup: _Update_SetupCategory(); break;
                case ECategory.IK: _Update_IKCategory(); break;
                case ECategory.Modifiers: _Update_ModsCategory(); break;
                case ECategory.Elasticity: _Update_ElasticnessCategory(); break;
                case ECategory.Morphing: _Gizmos_MorphingCategory(); break;
                case ECategory.Custom: _Update_ModulesCategory(); break;
            }
        }



        //public float SetTimeByProgress(float animProgress)
        //{

        //}

        //public float SetAnimationTimeBySeconds(float animSeconds)
        //{

        //}





        #region Clip export mods

        public float GetCurrentAnimationProgress()
        {
            return 0f;
        }


        public float GetClipDurationMul()
        {
            if (S == null) return 1f;
            if (TargetClip == null) return 1f;
            if (_anim_MainSet == null) { _anim_MainSet = S.GetSetupForClip(S.MainSetupsForClips, TargetClip, _toSet_SetSwitchToHash); return 1f; }
            if (_anim_MainSet.ClipDurationMultiplier < 0.1f) return 1f;
            if (_anim_MainSet.ClipDurationMultiplier > 100f) return 1f;
            return _anim_MainSet.ClipDurationMultiplier;
        }

        public float GetClipLeftTrim()
        {
            if (S == null) return 0f;
            if (TargetClip == null) return 0f;
            if (_anim_MainSet == null) { _anim_MainSet = S.GetSetupForClip(S.MainSetupsForClips, TargetClip, _toSet_SetSwitchToHash); return 0f; }
            if (_anim_MainSet.ClipTrimFirstFrames < 0f) return 0f;
            if (_anim_MainSet.ClipTrimFirstFrames > 1f) return 0f;
            return _anim_MainSet.ClipTrimFirstFrames;
        }

        public float GetClipRightTrim(bool oneMinus)
        {
            float defa = oneMinus ? 1f : 0f;
            if (S == null) return defa;
            if (TargetClip == null) return defa;
            if (_anim_MainSet == null) { _anim_MainSet = S.GetSetupForClip(S.MainSetupsForClips, TargetClip, _toSet_SetSwitchToHash); return defa; }
            if (_anim_MainSet.ClipTrimLastFrames < 0f) return defa;
            if (_anim_MainSet.ClipTrimLastFrames > 1f) return defa;
            if (oneMinus) return 1f - _anim_MainSet.ClipTrimLastFrames; else return _anim_MainSet.ClipTrimLastFrames;
        }

        public void RefreshClipLengthModValues()
        {
            if (TargetClip) _play_mod_clipOrigLen = TargetClip.length;

            _play_sett_startTime = 0f;
            _play_sett_stopTime = 0f;
            _play_humanoidPreviewCorrection = 0f;
            _play_humanoidPreviewCorrectionMul = 1f;

            bool skipAnimRange = currentClipSettings == null;
            bool isHumanoidIKSetup = false;
            if (currentMecanim != null) if (currentMecanim.isHuman)
                    if (Anim_MainSet.Additional_UseHumanoidMecanimIK)
                    {
                        skipAnimRange = true;
                        isHumanoidIKSetup = true;
                    }

            if (skipAnimRange == false)
            {
                bool change = false;
                if (currentClipSettings.startTime != 0f)
                {
                    change = true;
                    _play_sett_startTime = currentClipSettings.startTime;
                }

                if (currentClipSettings.stopTime != TargetClip.length)
                {
                    change = true;
                    _play_sett_stopTime = currentClipSettings.stopTime;
                }

                if (change)
                {
                    _play_mod_clipOrigLen = _play_sett_stopTime - _play_sett_startTime;
                }
            }

            if (isHumanoidIKSetup)
            {
                _play_mod_clipOrigLen = currentClipSettings.stopTime - currentClipSettings.startTime;
                _play_humanoidPreviewCorrection = currentClipSettings.startTime;
                _play_humanoidPreviewCorrectionMul = TargetClip.length / _play_mod_clipOrigLen;
            }

            _play_mod_clipLenMul = GetClipDurationMul();
            _play_mod_clipStartTrim = GetClipLeftTrim();
            _play_mod_clipEndTrim = GetClipRightTrim(false);
            _play_mod_Length = GetClipLengthModified();
            _play_mod_Length_PlusJustMul = _play_mod_clipOrigLen * _play_mod_clipLenMul;
            _play_mod_trimmedStart = _play_mod_clipOrigLen * _play_mod_clipStartTrim;
            _play_mod_dtMod = 1f / _play_mod_clipLenMul;
        }

        #endregion


        /// <summary> Checking if humanoid clip on humanoid rig etc. </summary>
        bool ClipIsValid
        {
            get
            {
                if (latestAnimator.IsHuman())
                {
                    if (TargetClip.isHumanMotion == false) return false;
                }
                else
                {
                    if (TargetClip.isHumanMotion) return false;
                }

                return true;
            }
        }

        public static bool latestAutoElapse = false;
        private bool triggerLoopRestoreForNonLooped = false;
        public void SampleCurrentAnimation(bool autoElapse = true)
        {
            // Not sampling if humanoid playing generic animation and vice versa
            if (ClipIsValid == false) return;
            latestAutoElapse = autoElapse;

            PreCalibrateLimbs();

            float sampleTime = GetMainClipAnimationSampleTime(autoElapse);

            #region Reset on no-looped clip

            if (triggerLoopRestoreForNonLooped)
            {
                triggerLoopRestoreForNonLooped = false;

                if (!isBaking)
                {
                    if (_anim_MainSet != null)
                        if (_anim_MainSet.settingsForClip != null)
                            if (_anim_MainSet.Export_LoopClip != ADClipSettings_Main.ELoopClipDetection.ForceLoop)
                                if (_anim_MainSet.Export_LoopClip == ADClipSettings_Main.ELoopClipDetection.NoLoop || _anim_MainSet.settingsForClip.isLooping == false)
                                {
                                    animationElapsed = 0f;
                                    animationProgressForEval = 0f;
                                    animationProgress = 0f;
                                    sampleTime = 0f;
                                    dt = 0.25f; deltaTime = dt;
                                    for (int i = 0; i < 32; i++) // CalmModel down model by simulating 0 frame for few seconds in the single tick
                                    {
                                        UpdateSimulationAfterAnimators(null);
                                        LateUpdateSimulation();
                                    }
                                }
                }
            }

            #endregion

            float clipTime = sampleTime;
            float animationProgressClipTime = animationElapsed;
            if (updateDesigner)
                if (Ar != null) UpdateSimulationOnSampling(Ar.rootBake, ref clipTime, ref animationProgressClipTime);

            float evlaluatedTime = clipTime;

            if (_anim_MainSet.ClipEvaluateTimeCurve != null)
                if (_anim_MainSet.ClipEvaluateTimeCurve.keys.Length > 1)
                {
                    bool isDefault = _anim_MainSet.IsUsingDefaultTimeEvaluation();

                    if (!isDefault)
                    {
                        if (_play_mod_Length != 0f)
                            evlaluatedTime = _anim_MainSet.ClipEvaluateTimeCurve.Evaluate
                                (clipTime / _play_mod_clipOrigLen) * _play_mod_clipOrigLen;
                    }
                }

            TargetClip.SampleAnimation(latestAnimator.gameObject, evlaluatedTime + _play_humanoidPreviewCorrection);
            UpdateHumanoidIKPreview(TargetClip, evlaluatedTime * _play_humanoidPreviewCorrectionMul);

            #region Secondary Animator Play

            //if (isBaking == false)
            {
                if (latestSecondaryAnimator != null)
                    if (latestSecondaryAnimatorClip != null)
                    {
                        if (latestSecondaryAnimatorClip.length != 0f && TargetClip.length != 0f)
                        {
                            float secondaryAnimTime = animationProgress * latestSecondaryAnimatorClip.length;
                            latestSecondaryAnimatorClip.SampleAnimation(latestSecondaryAnimator.gameObject, secondaryAnimTime);
                        }
                    }
            }

            #endregion


            if (_anim_MainSet != null)
            {
                if (_anim_MainSet.ResetRootPosition)
                {
                    Ar.RootBoneReference.TempTransform.localPosition = Vector3.zero;
                    Vector3 hipsLocal = latestAnimator.InverseTransformPoint(Ar.PelvisBoneReference.TempTransform.position);
                    hipsLocal.z = 0f;
                    Ar.PelvisBoneReference.TempTransform.position = latestAnimator.TransformPoint(hipsLocal);
                }
            }

            animationProgress = GetAnimationProgressFromSampleTime(autoElapse, null);
            animationProgressForEval = GetAnimationProgressFromSampleTime(false, animationProgressClipTime);
            //animationProgressForEval = animationProgress;
        }


        internal float EnsureMorphClipTime(float morphClipTime, AnimationClip clip, ADClipSettings_Morphing.MorphingSet morphSet)
        {
            if (updateDesigner)
                if (Ar != null) UpdateSimulationOnSamplingMorph(Ar.rootBake, clip, morphSet, ref morphClipTime);
            return morphClipTime;
        }

        public float GetMainClipAnimationSampleTime(bool? autoElapse)
        {
            if (autoElapse == null) autoElapse = latestAutoElapse;

            float cyclesMul = 1f;
            if (_anim_MainSet.AdditionalAnimationCycles > 0) cyclesMul = 1f + (float)_anim_MainSet.AdditionalAnimationCycles;

            float elapsed = animationElapsed;

            if (_anim_MainSet != null)
            {
                if (TargetClip.length > 0f)
                    if (_anim_MainSet.Export_ClipTimeOffset > 0.001f)
                        elapsed += _anim_MainSet.Export_ClipTimeOffset * _play_mod_Length;
            }

            float sampleTime = elapsed / _play_mod_clipLenMul;

            if (autoElapse.Value == true) sampleTime = (_play_mod_trimmedStart * _play_mod_clipLenMul + elapsed) / _play_mod_clipLenMul;

            if (cyclesMul > 1f)
            {
                sampleTime *= cyclesMul;
                sampleTime %= _play_mod_clipOrigLen;
            }

            if (_anim_MainSet != null)
            {
                if (_anim_MainSet.Export_ClipTimeOffset > 0.001f)
                {
                    sampleTime %= _play_mod_clipOrigLen;
                }

                float sampleTimeNorm = sampleTime / _play_mod_clipOrigLen;

                if (_anim_MainSet.ClipTimeReverse)
                {
                    sampleTime = (1f - sampleTimeNorm) * _play_mod_clipOrigLen;
                }

                if (_anim_MainSet.ClipSampleTimeCurve != null)
                    if (_anim_MainSet.ClipSampleTimeCurve.keys.Length > 0)
                    {
                        sampleTime = Mathf.LerpUnclamped(0f, sampleTime, _anim_MainSet.ClipSampleTimeCurve.Evaluate(sampleTimeNorm));
                    }
            }

            sampleTime += _play_sett_startTime;
            return sampleTime;
        }

        public float GetAnimationProgressFromSampleTime(bool? autoElapse, float? sampleTime = null)
        {
            if (autoElapse == null) autoElapse = latestAutoElapse;

            if (sampleTime == null) sampleTime = animationElapsed;

            float animationProgress = this.animationProgress;

            if (_play_mod_Length > 0f)
            {

                if (autoElapse == true) animationProgress = sampleTime.Value / _play_mod_Length;
                else
                {
                    if (isBaking == false)
                        animationProgress = FLogicMethods.InverseLerpUnclamped(0f, _play_mod_Length_PlusJustMul - (_play_mod_clipEndTrim * _play_mod_Length_PlusJustMul + (_play_mod_clipStartTrim * _play_mod_Length_PlusJustMul)), sampleTime.Value);
                    else
                        animationProgress = FLogicMethods.InverseLerpUnclamped(_play_mod_clipStartTrim * _play_mod_Length_PlusJustMul, _play_mod_Length_PlusJustMul - (_play_mod_clipEndTrim * _play_mod_Length_PlusJustMul), sampleTime.Value);
                    //UnityEngine.Debug.Log("progr = " + sampleTime.Value + "  clipStartTrim = " + _play_mod_clipStartTrim + "|" + (_play_mod_clipStartTrim * _play_mod_Length_PlusJustMul) + "   endTrim =  " + _play_mod_clipEndTrim + "|" + (_play_mod_Length_PlusJustMul - (_play_mod_clipEndTrim * _play_mod_Length_PlusJustMul)) + "   lengJustMul: " + _play_mod_Length_PlusJustMul + "  =>  " + animationProgress);
                }
                //if (!autoElapse)
                //{
                //    UnityEngine.Debug.Log("elapsed  = " + sampleTime + "   " + animationElapsed);
                //    UnityEngine.Debug.Log("trim < " + (_play_mod_clipStartTrim * _play_mod_Length_PlusJustMul) + "  " + (_play_mod_Length_PlusJustMul - (_play_mod_clipEndTrim * _play_mod_Length_PlusJustMul)) + " > = " + (animationElapsed));
                //}
            }

            if (System.Single.IsNaN(animationProgress)) animationProgress = 0f;

            return animationProgress;
        }

        void PreCalibrateLimbs()
        {
            for (int i = 0; i < Limbs.Count; i++) Limbs[i].ComponentBlendingPreLateUpdateCalibrate(_anim_morphSet);
        }

        void StartBake(AnimationClip originalClip)
        {
            Ar.Bake_Prepare(latestAnimator, originalClip, S, _anim_MainSet);
        }


        void BakingLoop(ref AnimationClip clip, int i, int keys, int startKey, int endKey)
        {
            if (bakeFramerateRatio <= 1.05f || i == startKey || i == endKey) // No need for oversimulating
            {
                animationElapsed = (((float)(i) / (float)(keys)) * _play_mod_Length_PlusJustMul);
                if (System.Single.IsNaN(animationElapsed)) animationElapsed = 0f;
                deltaTime = dt;

                SampleCurrentAnimation(false);
                //if (AnimationDesignerWindow.isBaking) if (animationProgress > 0.7f) UnityEngine.Debug.Log("pr = " + animationProgress);
                UpdateSimulationAfterAnimators(Ar.rootBake);

                LateUpdateSimulation();

                Ar.Bake_CaptureFramePose(animationElapsed);
            }
            else // Oversimulating for 60fps density
            {
                float overSimulates = Mathf.Ceil(bakeFramerateRatio); // Density of additional frames if required

                for (int o = 0; o < overSimulates; o++)
                {
                    deltaTime = dt / overSimulates;
                    float overSimProgr = (float)o / overSimulates;

                    animationElapsed = (((float)((float)(i) + overSimProgr) / (float)(keys)) * _play_mod_Length_PlusJustMul);

                    SampleCurrentAnimation(false);
                    UpdateSimulationAfterAnimators(Ar.rootBake);
                    LateUpdateSimulation();
                }

                Ar.Bake_CaptureFramePose(animationElapsed);
            }
        }


        void FinishBake(ref AnimationClip clip, AnimationClip originalClip, ADClipSettings_Main main)
        {
            Ar.Bake_Complete(ref clip, S, originalClip, main);
        }

        /// <summary> Simulate to calm down model after export </summary>
        void CalmModel()
        {
            try
            {
                dt = 0.2f;
                animationElapsed = 0f;
                deltaTime = dt;

                for (int i = 0; i < 54; i++)
                {
                    BakingPreSimulation();
                }

            }
            catch (System.Exception exc)
            {
                UnityEngine.Debug.Log("[Animation Designer] Something wrong happened during post-simulating (not important stage of baking)");
                UnityEngine.Debug.LogException(exc);
            }
        }


        #region Editor Window Delta Time

        // Delta time for window in editor

        /// <summary> Static delta time computed on clip length and framerate </summary>
        public float dt { get; private set; } = 0.1f;
        /// <summary> Static delta time computed on clip length and framerate but adapted to 60fps </summary>
        protected float adaptDt = 0.1f;

        /// <summary> Delta time which should be used in simulation methods - it's active and changing when oversimulating desnity frames for 60fps </summary>
        protected float deltaTime = 0.1f;

        /// <summary> When clip is 30fps, we simulate bake in 60fps so we need [60f / framerate] (2f) times more simulation steps</summary>
        protected float bakeFramerateRatio = 1f;

        double lastUpdateTime = 0f;
        protected bool _dtWasUpdating = false;
        protected bool _dtForcingUpdate = false;

        protected virtual void UpdateEditorDeltaTime()
        {
            if (_dtForcingUpdate)
            {
                if (!_dtWasUpdating)
                {
                    lastUpdateTime = EditorApplication.timeSinceStartup;
                    _dtWasUpdating = true;
                }
            }
            else
            {
                _dtWasUpdating = false;
            }

            if (_dtWasUpdating)
            {
                if (isBaking == false)
                {
                    dt = (float)(EditorApplication.timeSinceStartup - lastUpdateTime);
                    adaptDt = dt;
                    deltaTime = dt;
                    bakeFramerateRatio = 1f;
                }

                lastUpdateTime = EditorApplication.timeSinceStartup;
            }


            _dtForcingUpdate = false;
        }


        #endregion

    }
}