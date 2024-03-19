using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// Editor courutine usings
//using Unity.EditorCoroutines.Editor;
//using System.Collections;

namespace FIMSpace.AnimationTools
{
    public partial class AnimationDesignerWindow : EditorWindow
    {
        //bool includeHeadKeyframes = false;
        //bool includeFingersKeyframes = false;
        bool additionalBakeSettingsFoldout = false;
        //float optimizeCurves = 0.01f;
        public static bool isBaking = false;
        public static int CurrentBakeFrame = 0;
        public static bool DebugSurgic = false;

        public void ExportToFile(string path, bool rememberSavePath = false)
        {
            isBaking = true;
            RefreshClipLengthModValues();

            //EditorCoroutineUtility.StartCoroutine(IEExportToFile(path), latestAnimator);
            //return;

            AnimationClip originalClip = Get.TargetClip;
            AnimationClipSettings originalSettings = AnimationUtility.GetAnimationClipSettings(originalClip);
            AnimationClip newGeneratedClip = AssetDatabase.LoadAssetAtPath(path, typeof(AnimationClip)) as AnimationClip;
            ADBoneReference.LoopBakedPose = originalClip.isLooping;

            bool exists = false;
            if (newGeneratedClip == null) newGeneratedClip = new AnimationClip();
            else exists = true;

            newGeneratedClip.frameRate = TargetClip.frameRate;
            if (newGeneratedClip.frameRate <= 0f) newGeneratedClip.frameRate = 30;
            playPreview = false;


            Vector3 preAnimPos = latestAnimator.transform.position;
            Quaternion preAnimRot = latestAnimator.transform.rotation;
            Vector3 preAnimLocalScale = latestAnimator.transform.localScale;

            #region Parent Save

            Transform preParent = latestAnimator.transform.parent;

            Vector3 preParentPos = Vector3.zero;
            Quaternion preParentRot = Quaternion.identity;
            Vector3 preParentScale = Vector3.one;

            if (preParent)
            {
                preParentPos = preParent.position;
                preParentRot = preParent.rotation;
                preParentScale = preParent.localScale;

                preParent.transform.position = Vector3.zero;
                preParent.transform.rotation = Quaternion.identity;
                preParent.transform.localScale = Vector3.one;
            }

            #endregion

            latestAnimator.transform.position = Vector3.zero;
            latestAnimator.transform.rotation = Quaternion.identity;
            latestAnimator.transform.localScale = Vector3.one;

            bool clipIsLooping = false;

            try
            {
                _anim_MainSet = S.GetSetupForClip(S.MainSetupsForClips, TargetClip, _toSet_SetSwitchToHash);

                animationElapsed = 0f;
                animationProgress = 0f;

                ForceZeroFramePose();
                CalmModel();

                StartBake(originalClip);

                int keys = Mathf.CeilToInt(TargetClip.length * _play_mod_clipLenMul * TargetClip.frameRate);
                //int trimmedKeysCount = Mathf.CeilToInt(_play_mod_Length * TargetClip.frameRate);

                int keysStartOffset = 0;
                int keysEndOffset = 0;
                if (_play_mod_clipStartTrim <= 0.001f) keysStartOffset = 0; else keysStartOffset = Mathf.RoundToInt(_play_mod_clipStartTrim * (TargetClip.length * TargetClip.frameRate) * _play_mod_clipLenMul);
                if (_play_mod_clipEndTrim <= 0.001f) keysEndOffset = 0; else keysEndOffset = Mathf.RoundToInt(_play_mod_clipEndTrim * (TargetClip.length * TargetClip.frameRate) * _play_mod_clipLenMul);

                int startKey = keysStartOffset;
                int endKey = keys - keysEndOffset;

                int targetKeysCountAfterTrim = endKey - startKey;
                //UnityEngine.Debug.Log("startkey = " + startKey + " end = " + endKey + " trimCNT = " + targetKeysCountAfterTrim);

                ComputeBakeStepDelta((float)targetKeysCountAfterTrim); // Prepare constant delta time for simulation
                ResetComponentsStates(true);

                float overSimulates = Mathf.Ceil(bakeFramerateRatio); // Density of additional frames if required

                //UnityEngine.Debug.Log("Framerate ratio: " + bakeFramerateRatio + " framerate " + TargetClip.frameRate + " oversimulates = " + overSimulates);

                #region Pre-simulating cycle for better modificators loop posing

                bool preSimulate = false;


                if (_anim_MainSet.Export_LoopClip == ADClipSettings_Main.ELoopClipDetection.AutoDetect)
                {
                    if (originalClip.legacy)
                    {
                        if (originalClip.wrapMode == WrapMode.Loop) clipIsLooping = true;
                    }
                    else
                    {
                        if (originalClip.isLooping || originalSettings.loopBlend || originalSettings.loopTime)
                        {

                            clipIsLooping = true;
                        }
                    }
                }
                else
                {
                    clipIsLooping = _anim_MainSet.Export_LoopClip == ADClipSettings_Main.ELoopClipDetection.ForceLoop;
                }


                if (clipIsLooping) preSimulate = true;

                if (preSimulate)
                {
                    int preLoops = 8;
                    if (keys > 100) preLoops = 6;
                    if (keys > 200) preLoops = 4;
                    if (keys > 400) preLoops = 1;

                    for (int l = 0; l < preLoops; l++)
                    {
                        for (int i = startKey; i <= endKey; i++)
                        {
                            EditorUtility.DisplayProgressBar("Pre-Simulating Animation Clip for better Loop Match...", (i + l * keys) + " / " + (keys * preLoops), (float)(i + l * keys) / (float)(keys * preLoops));

                            if (bakeFramerateRatio <= 1.05f)
                            {
                                animationElapsed = (((float)(i) / (float)(keys - 1)) * _play_mod_Length_PlusJustMul);
                                //animationElapsed = ((float)i / (float)(keys - 1)) * _play_mod_Length;
                                deltaTime = dt;
                                BakingPreSimulation();
                            }
                            else
                            {
                                //UnityEngine.Debug.Log("oversimulates = " + overSimulates + " ratio = " +bakeFramerateRatio);

                                for (int o = 0; o < overSimulates; o++)
                                {
                                    deltaTime = dt / overSimulates;
                                    float overSimProgr = (float)o / overSimulates;

                                    animationElapsed = (((float)((float)(i) + overSimProgr) / (float)(keys - 1)) * _play_mod_Length_PlusJustMul);
                                    //animationElapsed = ((float)((float)i + overSimProgr) / (float)(keys - 1)) * _play_mod_Length;
                                    BakingPreSimulation();
                                }
                            }
                        }
                    }
                }
                else // Presimulating but just on simple zero frame
                {
                    for (int i = startKey; i <= endKey; i++)
                    {
                        EditorUtility.DisplayProgressBar("Pre-Simulating Animation Clip for better Loop Match...", (i) + " / " + (keys), (float)(i) / (float)(keys));

                        if (bakeFramerateRatio <= 1.05f)
                        {
                            animationElapsed = 0f;
                            deltaTime = dt;
                            BakingPreSimulation();
                        }
                        else
                        {
                            for (int o = 0; o < overSimulates; o++)
                            {
                                deltaTime = dt / overSimulates;
                                float overSimProgr = (float)o / overSimulates;

                                animationElapsed = 0f;
                                BakingPreSimulation();
                            }
                        }
                    }
                }

                #endregion

                animationElapsed = 0f;
                animationProgress = 0f;
                BakingPreSimulation();

                for (int i = startKey; i <= endKey; i++)
                {
                    CurrentBakeFrame = i;
                    bool breakBake = EditorUtility.DisplayCancelableProgressBar("Baking Animation Clip...", i + " / " + keys, (float)i / (float)keys);

                    BakingLoop(ref newGeneratedClip, i, keys, startKey, endKey);

                    if (breakBake)
                    {
                        EditorUtility.ClearProgressBar();
                        return;
                    }
                }

                if (_anim_MainSet == null) { UnityEngine.Debug.Log("[Animation Designer Error] There is no 'main settings' reference!"); }
                if (_anim_MainSet != null) if (clipIsLooping) ADBoneReference.LoopWrapAdditionalFrames = _anim_MainSet.Export_LoopAdditionalKeys;
                if (exists) newGeneratedClip.ClearCurves();
                FinishBake(ref newGeneratedClip, originalClip, _anim_MainSet);

                #region Additional processes like copying events

                List<AnimationEvent> aEvents = new List<AnimationEvent>();

                if (S.Export_CopyEvents)
                    for (int e = 0; e < originalClip.events.Length; e++)
                    {
                        aEvents.Add(originalClip.events[e]);
                    }

                if (S.Export_CopyCurves)
                {
                    var origCurves = AnimationUtility.GetCurveBindings(originalClip);
                    var newCurves = AnimationUtility.GetCurveBindings(newGeneratedClip);

                    for (int o = 0; o < origCurves.Length; o++)
                    {
                        EditorCurveBinding origB = origCurves[o];

                        bool contains = false;
                        for (int n = 0; n < newCurves.Length; n++)
                        {
                            if (newCurves[n].propertyName == origB.propertyName)
                            {
                                contains = true;
                                break;
                            }
                        }

                        if (!contains)
                        {
                            AnimationCurve curve = AnimationUtility.GetEditorCurve(originalClip, origB);
                            AnimationGenerateUtils.DistrubuteCurveOnTime(ref curve, 0f, newGeneratedClip.length);
                            newGeneratedClip.SetCurve(origB.path, origB.type, origB.propertyName, curve);
                        }

                    }
                }


                if (_anim_cModuleSet != null) _anim_cModuleSet.OnExportFinalizeModules(originalClip, newGeneratedClip, S, _anim_MainSet, aEvents);


                if (_anim_ikSet != null)
                    if (_anim_ikSet.LimbIKSetups != null)
                        for (int i = 0; i < _anim_ikSet.LimbIKSetups.Count; i++)
                        {
                            _anim_ikSet.LimbIKSetups[i].GenerateGroundingEventsFor(newGeneratedClip, aEvents);
                            //_anim_ikSet.LimbIKSetups[i].ExportGroundingCurveFor(ref newGeneratedClip);
                        }

                AnimationUtility.SetAnimationEvents(newGeneratedClip, aEvents.ToArray());

                #endregion


            }
            catch (Exception exc)
            {
                EditorUtility.ClearProgressBar();
                UnityEngine.Debug.Log("[Animation Designer] Error occured during baking the animation clip!");
                UnityEngine.Debug.LogException(exc);
                EditorUtility.DisplayDialog("Error Occured!", "Some Error Occured during animation baking.\nPlease check console logs.\n(Also take look on your limbs setup if you're not lacking some of the bone transforms!)", "Ok");
            }

            isBaking = false;
            EditorUtility.ClearProgressBar();

            //if (exists) { var toDestr = AssetDatabase.LoadAssetAtPath(path, typeof(AnimationClip)); if (toDestr) DestroyImmediate(toDestr, true); }
            if (!exists) AssetDatabase.CreateAsset(newGeneratedClip, path);
            else
            {
                //UnityEngine.Debug.Log("[Animation Designer] Overwriting Animation Clip File - If you changed clip duration try saving animation clip file in new file. Unity seems to handle animation clip overwriting glitchy.");
            }

            newGeneratedClip.frameRate = TargetClip.frameRate;
            newGeneratedClip.legacy = originalClip.legacy;
            if (_exportLegacy) newGeneratedClip.legacy = _exportLegacy;
            newGeneratedClip.wrapMode = originalClip.wrapMode;

            if (newGeneratedClip.wrapMode == WrapMode.Loop && !clipIsLooping)
            {
                newGeneratedClip.wrapMode = WrapMode.Default;
            }

            if (latestAnimator.IsHuman() && _forceExportGeneric == false)
            {
                if (S.Export_SetAllOriginalBake)
                {
                    originalSettings.keepOriginalPositionXZ = true;
                    originalSettings.keepOriginalOrientation = true;
                    originalSettings.keepOriginalPositionY = true;
                    originalSettings.loopBlendPositionXZ = true;
                    originalSettings.loopBlendOrientation = true;
                    originalSettings.loopBlendPositionY = true;

                    //if (newGeneratedClip.hasRootCurves || ADRootMotionBakeHelper.ClipContainsRootPositionCurves(newGeneratedClip) )
                    //{
                    //    originalSettings.keepOriginalPositionXZ = false;
                    //    originalSettings.loopBlendPositionXZ = false;
                    //}

                    //if (originalClip.isLooping || originalClip.isLooping || originalSettings.loopBlend || originalSettings.loopTime)
                    if (clipIsLooping)
                    {
                        bool loopBl = true;
                        if (_anim_MainSet != null)
                            if (_anim_MainSet.Export_LoopAdditionalKeys > 0) loopBl = false;

                        originalSettings.loopBlend = loopBl;
                        originalSettings.loopTime = true;
                    }
                    else
                    {
                        originalSettings.loopBlend = false;
                        originalSettings.loopTime = false;
                    }
                }
                else
                {
                    if (!clipIsLooping)
                    {
                        originalSettings.loopBlend = false;
                        originalSettings.loopTime = false;
                    }
                }

                newGeneratedClip.EnsureQuaternionContinuity();
            }
            else
            {
                if (clipIsLooping)
                {
                    if (currentLegacy) newGeneratedClip.wrapMode = WrapMode.Loop;
                    else
                    {
                        bool loopBl = true;
                        if (_anim_MainSet != null)
                            if (_anim_MainSet.Export_LoopAdditionalKeys > 0) loopBl = false;

                        originalSettings.loopBlend = loopBl;
                        originalSettings.loopTime = true;
                    }
                }
                else
                {
                    newGeneratedClip.wrapMode = WrapMode.Default;
                    originalSettings.loopTime = false;
                }
            }




            if (_anim_MainSet.Export_ClipTimeOffset > 0.001f) originalSettings.cycleOffset = 0f;

            originalSettings.startTime = _play_mod_Length_PlusJustMul * _play_mod_clipStartTrim;
            originalSettings.stopTime = originalSettings.startTime + _play_mod_Length;
            AnimationUtility.SetAnimationClipSettings(newGeneratedClip, originalSettings);

            EditorUtility.SetDirty(newGeneratedClip);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Reimport for refresh
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            // Commend below are left for debugging if needed!
            if (debugLogs)
            {
                UnityEngine.Debug.Log("--- Animation Designer Debug Log Below ---\n(multiple logs because too long logs are not handles by unity)");
                string debug = "ORIGINAL CLIP INFO:\n";
                debug += GetClipReportLog(TargetClip);
                UnityEngine.Debug.Log(debug);

                debug = "EXPORTED CLIP INFO:\n";
                debug += GetClipReportLog(newGeneratedClip);
                UnityEngine.Debug.Log(debug);

                debug = "ADDITIONAL INFO:\n";
                debug += "Oversimulates: " + Mathf.Ceil(bakeFramerateRatio);
                debug += "\nBake frame ratio: " + bakeFramerateRatio;
                debug += "\nClip Length Multiplier: " + _anim_MainSet.ClipDurationMultiplier;
                debug += "\nClip Trims: " + _anim_MainSet.ClipTrimFirstFrames + "   <->   " + (1f - _anim_MainSet.ClipTrimLastFrames);
                UnityEngine.Debug.Log(debug);

            }

            if (rememberSavePath)
            {
                staticExportDirectory = path;
                staticExportDirectory = System.IO.Path.GetDirectoryName( staticExportDirectory.Replace(Application.dataPath, ""));
                if (staticExportDirectory == "Assets") staticExportDirectory = "";
            }

            newGeneratedClip = AssetDatabase.LoadAssetAtPath(path, typeof(AnimationClip)) as AnimationClip;

            Selection.activeObject = null; // Deselecting since I didn't find way to refresh inspector window of selected animation clip (old animation clip was displayed)

            if (!newGeneratedClip) UnityEngine.Debug.Log("[Animation Designer] Something went wrong when creating animation clip file! No file created!");
            else EditorGUIUtility.PingObject(newGeneratedClip);

            LatestSaved = newGeneratedClip;


            #region Parent restore

            if (preParent)
            {
                preParent.transform.position = preParentPos;
                preParent.transform.rotation = preParentRot;
                preParent.transform.localScale = preParentScale;
            }

            #endregion


            latestAnimator.transform.position = preAnimPos;
            latestAnimator.transform.rotation = preAnimRot;
            latestAnimator.transform.localScale = preAnimLocalScale;

            CalmModel();
        }



        #region Editor Coroutine Baking Debugging




        //public IEnumerator IEExportToFile(string path)
        //{
        //    float coroMul = 0.175f;

        //    UnityEngine.Debug.Log("START");
        //    yield return new WaitForSecondsRealtime(1f * coroMul);

        //    AnimationClip originalClip = Get.TargetClip;
        //    AnimationClipSettings originalSettings = AnimationUtility.GetAnimationClipSettings(originalClip);
        //    AnimationClip newGeneratedClip = AssetDatabase.LoadAssetAtPath(path, typeof(AnimationClip)) as AnimationClip;

        //    ADBoneReference.LoopBakedPose = originalClip.isLooping;

        //    bool exists = false;
        //    if (newGeneratedClip == null) newGeneratedClip = new AnimationClip(); else exists = true;

        //    newGeneratedClip.frameRate = TargetClip.frameRate;
        //    if (newGeneratedClip.frameRate <= 0f) newGeneratedClip.frameRate = 30;
        //    playPreview = false;


        //    UnityEngine.Debug.Log("ZERO");
        //    yield return new WaitForSecondsRealtime(1f * coroMul);


        //    Vector3 preAnimPos = latestAnimator.transform.position;
        //    Quaternion preAnimRot = latestAnimator.transform.rotation;
        //    Vector3 preAnimLocalScale = latestAnimator.transform.localScale;

        //    #region Parent Save

        //    Transform preParent = latestAnimator.transform.parent;

        //    Vector3 preParentPos = Vector3.zero;
        //    Quaternion preParentRot = Quaternion.identity;
        //    Vector3 preParentScale = Vector3.one;

        //    if (preParent)
        //    {
        //        preParentPos = preParent.position;
        //        preParentRot = preParent.rotation;
        //        preParentScale = preParent.localScale;

        //        preParent.transform.position = Vector3.zero;
        //        preParent.transform.rotation = Quaternion.identity;
        //        preParent.transform.localScale = Vector3.one;
        //    }

        //    #endregion

        //    latestAnimator.transform.position = Vector3.zero;
        //    latestAnimator.transform.rotation = Quaternion.identity;
        //    latestAnimator.transform.localScale = Vector3.one;

        //    UnityEngine.Debug.Log("RESET");
        //    yield return new WaitForSecondsRealtime(1f * coroMul);

        //    //ForceTPose();
        //    ForceZeroFramePose();
        //    CalmModel();
        //    StartBake(originalClip);

        //    int keys = Mathf.CeilToInt(TargetClip.length * _play_mod_clipLenMul * TargetClip.frameRate);
        //    //int trimmedKeysCount = Mathf.CeilToInt(_play_mod_Length * TargetClip.frameRate);

        //    int keysStartOffset = 0;
        //    int keysEndOffset = 0;
        //    if (_play_mod_clipStartTrim <= 0.001f) keysStartOffset = 0; else keysStartOffset = Mathf.RoundToInt(_play_mod_clipStartTrim * (TargetClip.length * TargetClip.frameRate) * _play_mod_clipLenMul);
        //    if (_play_mod_clipEndTrim <= 0.001f) keysEndOffset = 0; else keysEndOffset = Mathf.RoundToInt(_play_mod_clipEndTrim * (TargetClip.length * TargetClip.frameRate) * _play_mod_clipLenMul);

        //    int startKey = keysStartOffset;
        //    int endKey = keys - keysEndOffset;
        //    int targetKeysCountAfterTrim = endKey - startKey;

        //    ComputeBakeStepDelta(targetKeysCountAfterTrim); // Prepare constant delta time for simulation
        //    ResetComponentsStates(true);

        //    float overSimulates = Mathf.Ceil(bakeFramerateRatio); // Density of additional frames if required


        //    UnityEngine.Debug.Log("COMPONENTS RESETTED");
        //    yield return new WaitForSecondsRealtime(1f * coroMul);
        //    UnityEngine.Debug.Log("PRESIMULATING (oversimulates = " + overSimulates + " - ClipFramerate: " + TargetClip.frameRate + ")");


        //    #region Pre-simulating cycle for better modificators loop posing

        //    bool preSimulate = false;

        //    if (originalClip.legacy)
        //    {
        //        if (originalClip.wrapMode == WrapMode.Loop) preSimulate = true;
        //    }
        //    else
        //    {
        //        if (originalClip.isLooping || originalSettings.loopBlend || originalSettings.loopTime)
        //        {

        //            preSimulate = true;
        //        }
        //    }

        //    if (preSimulate)
        //    {
        //        int preLoops = 8;
        //        if (keys > 100) preLoops = 6;
        //        if (keys > 200) preLoops = 4;
        //        if (keys > 400) preLoops = 1;

        //        for (int l = 0; l < preLoops; l++)
        //        {
        //            for (int i = startKey; i <= endKey; i++)
        //            {
        //                //EditorUtility.DisplayProgressBar("Pre-Simulating Animation Clip for better Loop Match...", (i + l * keys) + " / " + (keys * preLoops), (float)(i + l * keys) / (float)(keys * preLoops));

        //                if (bakeFramerateRatio <= 1.05f)
        //                {
        //                    animationElapsed = (((float)(i) / (float)(keys - 1)) * _play_mod_Length_PlusJustMul);
        //                    //animationElapsed = ((float)i / (float)(keys - 1)) * _play_mod_Length;
        //                    deltaTime = dt;
        //                    BakingPreSimulation();
        //                    yield return new WaitForSecondsRealtime((0.001f / (4f + l * 2f)) * coroMul);
        //                }
        //                else
        //                {
        //                    //UnityEngine.Debug.Log("oversimulates = " + overSimulates + " ratio = " +bakeFramerateRatio);

        //                    for (int o = 0; o < overSimulates; o++)
        //                    {
        //                        deltaTime = dt / overSimulates;
        //                        float overSimProgr = (float)o / overSimulates;

        //                        animationElapsed = (((float)((float)(i) + overSimProgr) / (float)(keys - 1)) * _play_mod_Length_PlusJustMul);
        //                        //animationElapsed = ((float)((float)i + overSimProgr) / (float)(keys - 1)) * _play_mod_Length;
        //                        BakingPreSimulation();
        //                        yield return new WaitForSecondsRealtime((0.001f / (4f + l * 2f)) * coroMul);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    else // Presimulating but just on simple zero frame
        //    {
        //        UnityEngine.Debug.Log("PRESIMULATING ON ZERO FRAME - NOT LOOPED CLIP");

        //        for (int i = startKey; i <= endKey; i++)
        //        {
        //            //EditorUtility.DisplayProgressBar("Pre-Simulating Animation Clip for better Loop Match...", (i) + " / " + (keys), (float)(i) / (float)(keys));

        //            if (bakeFramerateRatio <= 1.05f)
        //            {
        //                animationElapsed = 0f;
        //                deltaTime = dt;
        //                BakingPreSimulation();
        //                yield return new WaitForSecondsRealtime((0.001f / (4f)) * coroMul);
        //            }
        //            else
        //            {
        //                for (int o = 0; o < overSimulates; o++)
        //                {
        //                    deltaTime = dt / overSimulates;
        //                    animationElapsed = 0f;
        //                    BakingPreSimulation();
        //                    yield return new WaitForSecondsRealtime((0.001f / (4f)) * coroMul);
        //                }
        //            }
        //        }
        //    }

        //    #endregion

        //    UnityEngine.Debug.Log("PRESIMULATING DONE");
        //    yield return new WaitForSecondsRealtime(1f * coroMul + 1f);
        //    UnityEngine.Debug.Log("BAKING");

        //    yield return new WaitForSecondsRealtime(0.3f);
        //    UnityEngine.Debug.Log("PRE CHECK");
        //    DebugSurgic = true;

        //    animationElapsed = 0f;
        //    animationProgress = 0f;
        //    BakingPreSimulation();
        //    yield return new WaitForSecondsRealtime(0.3f);
        //    UnityEngine.Debug.Log("PRE CHECK DONE");

        //    DebugSurgic = false;

        //    yield return new WaitForSecondsRealtime(1f * coroMul);
        //    if (bakeFramerateRatio <= 1.05f)
        //        UnityEngine.Debug.Log("NO OVERSIMULATING");
        //    else
        //        UnityEngine.Debug.Log("OVERSIMULATING WILL BE APPLIED");

        //    UnityEngine.Debug.Log("BAKING WILL START NOW");
        //    yield return new WaitForSecondsRealtime(1f * coroMul + 1f);

        //    for (float i = startKey; i <= endKey; i++)
        //    {

        //        if (bakeFramerateRatio <= 1.05f || i == startKey || i == endKey) // No need for oversimulating
        //        {
        //            animationElapsed = (((float)(i) / (float)(keys - 1)) * _play_mod_Length_PlusJustMul);
        //            deltaTime = dt;

        //            SampleCurrentAnimation(false);
        //            //UnityEngine.Debug.Log("LBake for " + animationElapsed + " = progress: " + animationProgress + " (" + i + "/" + keys + ")");
        //            UpdateSimulationAfterAnimators(null);
        //            LateUpdateSimulation();

        //            Ar.Bake_CaptureFramePose(animationElapsed);
        //            yield return new WaitForSecondsRealtime(Mathf.Max(0.125f, 0.4f - (float)i * 0.05f) * coroMul);
        //        }
        //        else // Oversimulating for 60fps density
        //        {
        //            overSimulates = Mathf.Ceil(bakeFramerateRatio); // Density of additional frames if required

        //            for (float o = 0; o < overSimulates; o++)
        //            {
        //                deltaTime = dt / overSimulates;
        //                float overSimProgr = o / overSimulates;

        //                animationElapsed = (((float)((float)(i) + overSimProgr) / (float)(keys - 1)) * _play_mod_Length_PlusJustMul);
        //                SampleCurrentAnimation(false);

        //                //UnityEngine.Debug.Log("Bake for " + animationElapsed + " = progress: " + animationProgress + " (" + i + "/" + keys + ")");
        //                UpdateSimulationAfterAnimators(null);
        //                LateUpdateSimulation();
        //                yield return new WaitForSecondsRealtime(Mathf.Max(0.125f, 0.4f - (float)i * 0.05f) * coroMul);
        //            }

        //            Ar.Bake_CaptureFramePose(animationElapsed);
        //        }

        //    }

        //    if (_anim_MainSet != null) ADBoneReference.LoopWrapAdditionalFrames = _anim_MainSet.Export_LoopAdditionalKeys;
        //    if (exists) newGeneratedClip.ClearCurves();
        //    FinishBake(ref newGeneratedClip, originalClip, _anim_MainSet);

        //    UnityEngine.Debug.Log("BAKING DONE");
        //    yield return new WaitForSecondsRealtime(1f * coroMul);

        //    EditorUtility.ClearProgressBar();

        //    if (!exists) AssetDatabase.CreateAsset(newGeneratedClip, path);


        //    newGeneratedClip.frameRate = TargetClip.frameRate;
        //    newGeneratedClip.EnsureQuaternionContinuity();
        //    newGeneratedClip.legacy = newGeneratedClip.legacy;
        //    newGeneratedClip.wrapMode = newGeneratedClip.wrapMode;


        //    if (latestAnimator.IsHuman())
        //    {
        //        if (S.Export_SetAllOriginalBake)
        //        {
        //            originalSettings.keepOriginalPositionXZ = true;
        //            originalSettings.keepOriginalOrientation = true;
        //            originalSettings.keepOriginalPositionY = true;
        //            originalSettings.loopBlendPositionXZ = true;
        //            originalSettings.loopBlendOrientation = true;
        //            originalSettings.loopBlendPositionY = true;

        //            if (TargetClip.isLooping)
        //            {
        //                originalSettings.loopBlend = true;
        //                originalSettings.loopTime = true;
        //            }
        //        }
        //    }

        //    UnityEngine.Debug.Log("SAVE");
        //    yield return new WaitForSecondsRealtime(.1f * coroMul);

        //    originalSettings.startTime = _play_mod_Length_PlusJustMul * _play_mod_clipStartTrim;
        //    //originalSettings.stopTime = newGeneratedClip.length;
        //    originalSettings.stopTime = originalSettings.startTime + _play_mod_Length;

        //    AnimationUtility.SetAnimationClipSettings(newGeneratedClip, originalSettings);
        //    EditorUtility.SetDirty(newGeneratedClip);

        //    AssetDatabase.SaveAssets();
        //    AssetDatabase.Refresh();

        //    // Reimport for refresh
        //    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);



        //    // Commend below are left for debugging if needed!
        //    if (debugLogs)
        //    {
        //        UnityEngine.Debug.Log("--- Animation Designer Debug Log Below ---\n(multiple logs because too long logs are not handles by unity)");
        //        string debug = "ORIGINAL CLIP INFO:\n";
        //        debug += GetClipReportLog(TargetClip);
        //        UnityEngine.Debug.Log(debug);

        //        debug = "EXPORTED CLIP INFO:\n";
        //        debug += GetClipReportLog(newGeneratedClip);
        //        UnityEngine.Debug.Log(debug);

        //        debug = "ADDITIONAL INFO:\n";
        //        debug += "Oversimulates: " + Mathf.Ceil(bakeFramerateRatio);
        //        debug += "\nBake frame ratio: " + bakeFramerateRatio;
        //        debug += "\nClip Length Multiplier: " + _anim_MainSet.ClipDurationMultiplier;
        //        debug += "\nClip Trims: " + _anim_MainSet.ClipTrimFirstFrames + "   <->   " + (1f - _anim_MainSet.ClipTrimLastFrames);
        //        UnityEngine.Debug.Log(debug);

        //    }


        //    newGeneratedClip = AssetDatabase.LoadAssetAtPath(path, typeof(AnimationClip)) as AnimationClip;

        //    Selection.activeObject = null; // Deselecting since I didn't find way to refresh inspector window of selected animation clip (old animation clip was displayed)

        //    if (newGeneratedClip) EditorGUIUtility.PingObject(newGeneratedClip);

        //    LatestSaved = newGeneratedClip;

        //    UnityEngine.Debug.Log("RESET");
        //    yield return new WaitForSecondsRealtime(1.2f * coroMul);

        //    #region Parent restore

        //    if (preParent)
        //    {
        //        preParent.transform.position = preParentPos;
        //        preParent.transform.rotation = preParentRot;
        //        preParent.transform.localScale = preParentScale;
        //    }

        //    #endregion


        //    latestAnimator.transform.position = preAnimPos;
        //    latestAnimator.transform.rotation = preAnimRot;
        //    latestAnimator.transform.localScale = preAnimLocalScale;

        //    UnityEngine.Debug.Log("DONE");
        //    isBaking = false;
        //    CalmModel();
        //}





        #endregion


        void BakingPreSimulation()
        {
            SampleCurrentAnimation(!isBaking);
            UpdateSimulationAfterAnimators(null);
            LateUpdateSimulation();
        }

        /// <summary>
        /// Average delta time for every simulation step on target clip duration in seconds
        /// </summary>
        private void ComputeBakeStepDelta(float keys)
        {
            if (keys > 2)
            {
                dt = _play_mod_Length / ((float)keys - 1);
            }
            else
            {
                if (TargetClip.frameRate <= 0f) dt = 0.1f;
                else dt = 1f / (float)TargetClip.frameRate;
            }


            if (TargetClip.frameRate > 5f && TargetClip.frameRate < 50f)
            {
                bakeFramerateRatio = 60f / TargetClip.frameRate;
                adaptDt = dt / bakeFramerateRatio;
            }

            //elasticDt = dt;

            //if (S.Export_AdaptBakeFramerate > 0f) // Converting delta to be 60fps accurate for elasticness
            //{
            //    float frameR = TargetClip.frameRate; // 30?
            //    if (frameR < 10f) frameR = 10f; // clamp to 10
            //    frameR = frameR / 60f;
            //    //frameR = 1 / frameR; // 
            //    frameR = dt * frameR;
            //    if (frameR < 0.0001f) frameR = 0.0001f; else if (frameR > 0.05f) frameR = 0.05f;

            //    elasticDt = Mathf.LerpUnclamped(dt, frameR, S.Export_AdaptBakeFramerate);
            //}

        }

        string GetClipReportLog(AnimationClip clip)
        {
            EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(clip);
            string log = clip.name + "-Report ("+ bindings.Length+"):\n\n";
            log += "frameRate: " + clip.frameRate + "\n";
            log += "apparentSpeed: " + clip.apparentSpeed + "\n";
            log += "averageAngularSpeed: " + clip.averageAngularSpeed + "\n";
            log += "averageDuration: " + clip.averageDuration + "\n";
            log += "humanMotion: " + clip.humanMotion + "\n";
            log += "isHumanMotion: " + clip.isHumanMotion + "\n";
            log += "isLooping: " + clip.isLooping + "\n";
            log += "legacy: " + clip.legacy + "\n";
            log += "length: " + clip.length + "\n";
            log += "wrapMode: " + clip.wrapMode + "\n";
            log += "genericRoot: " + clip.hasGenericRootTransform + "\n";
            log += "rootCurves: " + clip.hasRootCurves + "\n\n";

            foreach (var binding in bindings)
            {
                AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, binding);
                log += (binding.path + "/" + binding.propertyName + ", Keys: " + curve.keys.Length + " \n");
            }

            return log;
        }

    }
}