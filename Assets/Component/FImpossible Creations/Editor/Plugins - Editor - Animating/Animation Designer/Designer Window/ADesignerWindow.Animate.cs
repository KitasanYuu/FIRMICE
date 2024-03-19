using System;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.AnimationTools
{
    public partial class AnimationDesignerWindow : EditorWindow
    {

        ADClipSettings_Main _anim_MainSet;
        public ADClipSettings_Main Anim_MainSet { get { return _anim_MainSet; } }
        ADClipSettings_Elasticness _anim_elSet;
        ADClipSettings_Modificators _anim_modSet;
        ADClipSettings_IK _anim_ikSet;
        ADClipSettings_CustomModules _anim_cModuleSet;
        ADClipSettings_Morphing _anim_morphSet;


        public void ResetComponentsStates(bool reInitialize)
        {
            CheckComponentsInitialization(reInitialize);

            for (int i = 0; i < Limbs.Count; i++)
                Limbs[i].ResetLimbComponentsState();

            _anim_MainSet.ResetState(S);
            _anim_modSet.ResetStates();
            _anim_ikSet.ResetState();
            _anim_cModuleSet.ResetState();
            _anim_morphSet.RefreshWithSetup(_anim_MainSet);
        }

        bool _latestWasMorphing = false;
        void UpdateSimulationOnSampling(ADRootMotionBakeHelper rootBaker, ref float clipTime, ref float animationProgressClipTime)
        {
            if (_anim_MainSet.TurnOnModules)
                if (_anim_cModuleSet != null)
                    if (_anim_cModuleSet.CustomModules != null)
                        for (int i = 0; i < _anim_cModuleSet.CustomModules.Count; i++)
                        {
                            var cModl = _anim_cModuleSet.CustomModules[i];
                            if (cModl.Enabled == false) continue;
                            cModl.OnPreUpdateSampling(S, _anim_MainSet, ref clipTime, ref animationProgressClipTime);
                        }
        }

        void UpdateSimulationOnSamplingMorph(ADRootMotionBakeHelper rootBaker, AnimationClip clip, ADClipSettings_Morphing.MorphingSet morphSet, ref float morphClipTime)
        {
            if (_anim_MainSet.TurnOnModules)
                if (_anim_cModuleSet != null)
                    if (_anim_cModuleSet.CustomModules != null)
                        for (int i = 0; i < _anim_cModuleSet.CustomModules.Count; i++)
                        {
                            var cModl = _anim_cModuleSet.CustomModules[i];
                            if (cModl.Enabled == false) continue;
                            cModl.OnPreUpdateSamplingMorph(S, clip, morphSet, ref morphClipTime);
                        }
        }

        void UpdateSimulationAfterAnimators(ADRootMotionBakeHelper rootBaker)
        {

            #region Morphing bake and restore

            _latestWasMorphing = false;

            if (_anim_MainSet.TurnOnMorphs)
                for (int i = 0; i < _anim_morphSet.Morphs.Count; i++)
                {
                    var morph = _anim_morphSet.Morphs[i];
                    if (morph.Enabled == false) continue;
                    _latestWasMorphing = true;
                    morph.CaptureMorph(S, _anim_MainSet);
                }

            if (_latestWasMorphing)
            {
                SampleCurrentAnimation(latestAutoElapse);
            }

            #endregion


            if (rootBaker != null)
            {
                rootBaker.PostAnimator();
            }


            #region Morphs Apply

            if (_latestWasMorphing)
            {
                for (int i = 0; i < _anim_morphSet.Morphs.Count; i++)
                {
                    var morph = _anim_morphSet.Morphs[i];
                    if (morph.Enabled == false) continue;
                    if (morph.UpdateOrder != ADClipSettings_Morphing.MorphingSet.EOrder.InheritElasticity) continue;
                    morph.ApplyMorph(animationProgressForEval, S);
                }
            }

            #endregion


            _anim_modSet.PreLateUpdateModificators(deltaTime);
            _anim_modSet.BeforeLateUpdateModificators(deltaTime, animationProgressForEval, S, _anim_MainSet);

            if (_anim_MainSet.TurnOnModules)
                _anim_cModuleSet.BeforeIKUpdateModules(deltaTime, animationProgressForEval, S, _anim_MainSet);

            if (rootBaker != null)
            {
                rootBaker.PostRootMotion();
                _anim_MainSet.LatestInternalRootMotionOffset = rootBaker.latestRootMotionPos;
            }
            else
            {
                _anim_MainSet.LatestInternalRootMotionOffset = ADRootMotionBakeHelper.RootModsOffsetAccumulation;
            }

            if (_anim_MainSet.TurnOnIK)
            {
                var limbsExecutionListIK = S.GetLimbsExecutionList(_anim_ikSet.LimbIKSetups);
                for (int i = 0; i < limbsExecutionListIK.Count; i++)
                    limbsExecutionListIK[i].IKCapture(_anim_ikSet.GetIKSettingsForLimb(limbsExecutionListIK[i], S));
            }

            _anim_MainSet.PreUpdateSimulation(S);

            _anim_modSet.PreElasticnessLateUpdateModificators(deltaTime, animationProgressForEval, S, _anim_MainSet);

            if (_anim_MainSet.TurnOnModules)
                _anim_cModuleSet.BeforeElasticnessLateUpdateModules(deltaTime, animationProgressForEval, S, _anim_MainSet);

            if (_anim_MainSet.TurnOnElasticness)
                for (int i = 0; i < Limbs.Count; i++)
                    Limbs[i].ElasticnessPreLateUpdate(_anim_elSet);


            if (_anim_MainSet.TurnOnIK)
            {
                var limbsExecutionListIK = S.GetLimbsExecutionList(_anim_ikSet.LimbIKSetups);
                for (int i = 0; i < limbsExecutionListIK.Count; i++)
                    limbsExecutionListIK[i].IKUpdateSimulation(_anim_ikSet.GetIKSettingsForLimb(limbsExecutionListIK[i], S), deltaTime, animationProgressForEval, 1f);
            }
        }


        void LateUpdateSimulation()
        {
            for (int i = 0; i < Limbs.Count; i++)
                Limbs[i].ComponentsBlendingLateUpdate(_anim_morphSet, deltaTime, animationProgressForEval);


            if (_anim_MainSet.TurnOnElasticness)
            {
                for (int i = 0; i < Limbs.Count; i++)
                {
                    if (Limbs[i].ExecuteFirst == false) continue;
                    Limbs[i].ElasticnessComponentsLateUpdate(_anim_elSet, _anim_MainSet, deltaTime, animationProgressForEval);
                }

                for (int i = 0; i < Limbs.Count; i++)
                {
                    if (Limbs[i].ExecuteFirst == true) continue;
                    Limbs[i].ElasticnessComponentsLateUpdate(_anim_elSet, _anim_MainSet, deltaTime, animationProgressForEval);
                }
            }


            _anim_modSet.LateUpdateModificators(deltaTime, animationProgressForEval, S, _anim_MainSet);

            if (_anim_MainSet.TurnOnModules)
                _anim_cModuleSet.LateUpdateModules(deltaTime, animationProgressForEval, S, _anim_MainSet);


            _anim_MainSet.LateUpdateSimulation(deltaTime, deltaTime, animationProgressForEval, S);

            if (_latestWasMorphing)
            {
                for (int i = 0; i < _anim_morphSet.Morphs.Count; i++)
                {
                    var morph = _anim_morphSet.Morphs[i];
                    if (morph.Enabled == false) continue;
                    if (morph.UpdateOrder != ADClipSettings_Morphing.MorphingSet.EOrder.BeforeIK) continue;
                    morph.ApplyMorph(animationProgressForEval, S);
                }
            }

            if (_anim_MainSet.TurnOnIK)
            {
                if (_anim_MainSet.TurnOnModules)
                    _anim_cModuleSet.OnInfluenceIKUpdateModules(deltaTime, animationProgressForEval, S, _anim_MainSet);

                var limbsExecutionListIK = S.GetLimbsExecutionList(_anim_ikSet.LimbIKSetups);
                for (int i = 0; i < limbsExecutionListIK.Count; i++)
                    limbsExecutionListIK[i].IKLateUpdateSimulation(_anim_ikSet.GetIKSettingsForLimb(limbsExecutionListIK[i], S), dt, animationProgressForEval, 1f, _anim_MainSet);
            }

            _anim_modSet.LastLateUpdateModificators(deltaTime, animationProgressForEval, S, _anim_MainSet);


            #region Morphs Apply

            if (_latestWasMorphing)
            {
                for (int i = 0; i < _anim_morphSet.Morphs.Count; i++)
                {
                    var morph = _anim_morphSet.Morphs[i];
                    if (morph.Enabled == false) continue;
                    if (morph.UpdateOrder != ADClipSettings_Morphing.MorphingSet.EOrder.OverrideModsAndIK) continue;
                    morph.ApplyMorph(animationProgressForEval, S);
                }
            }

            #endregion


            if (_anim_MainSet.TurnOnModules)
                _anim_cModuleSet.AfterLateUpdateModules(deltaTime, animationProgressForEval, S, _anim_MainSet);
            _anim_MainSet.LateUpdateAfterAllSimulation();

        }


        internal void CheckComponentsInitialization(bool reInitialize)
        {
            bool hChanged = false;
            for (int i = 0; i < Limbs.Count; i++)
                if (Limbs[i].CheckIfHierarchyChanged()) hChanged = true;

            if (S) if (S.TargetAvatar == null)
                    if (S.LatestAnimator)
                    {
                        Animator anim = S.LatestAnimator.GetAnimator();
                        if (anim) { S.TargetAvatar = anim.avatar; S._SetDirty(); }
                    }

            if (hChanged)
            {
                reInitialize = true;
                if (S) S._SetDirty();
            }

            _anim_MainSet = S.GetSetupForClip(S.MainSetupsForClips, TargetClip, _toSet_SetSwitchToHash);
            //_anim_MainSet = S.GetMainSetupForClip(TargetClip);
            _anim_MainSet.CheckForInitialization(S, reInitialize);

            for (int i = 0; i < Limbs.Count; i++) Limbs[i].RefreshLimb(S);

            _anim_elSet = S.GetSetupForClip(S.ElasticnessSetupsForClips, TargetClip, _toSet_SetSwitchToHash); //S.GetElasticnessSetupForClip(TargetClip);
            for (int i = 0; i < Limbs.Count; i++) Limbs[i].CheckLimbElasticnessComponentsInitialization(S, reInitialize);

            _anim_modSet = S.GetSetupForClip(S.ModificatorsSetupsForClips, TargetClip, _toSet_SetSwitchToHash); //_anim_modSet = S.GetModificatorsSetupForClip(TargetClip);
            _anim_modSet.CheckInitialization(S, reInitialize, _anim_MainSet);

            _anim_ikSet = S.GetSetupForClip(S.IKSetupsForClips, TargetClip, _toSet_SetSwitchToHash); //_anim_ikSet = S.GetIKSetupForClip(TargetClip);
            var limbsExecutionListIK = S.GetLimbsExecutionList(_anim_ikSet.LimbIKSetups);
            for (int i = 0; i < limbsExecutionListIK.Count; i++) limbsExecutionListIK[i].CheckForIKInitialization(S, _anim_ikSet.GetIKSettingsForLimb(limbsExecutionListIK[i], S), _anim_MainSet, animationProgressForEval, dt, 1f, reInitialize);

            _anim_cModuleSet = S.GetSetupForClip(S.CustomModuleSetupsForClips, TargetClip, _toSet_SetSwitchToHash); //_anim_springsSet = S.GetSpringSetupForClip(TargetClip);
            _anim_cModuleSet.CheckInitialization(S);
            //_anim_springsSet = S.GetSetupForClip(S.SpringSetupsForClips, TargetClip, _toSet_SetSwitchToHash); //_anim_springsSet = S.GetSpringSetupForClip(TargetClip);
            //_anim_springsSet.CheckInitialization(S);

            _anim_morphSet = S.GetSetupForClip(S.MorphingSetupsForClips, TargetClip, _toSet_SetSwitchToHash); // _anim_blendSet = S.GetBlendingSetupForClip(TargetClip);
            for (int i = 0; i < Limbs.Count; i++) Limbs[i].CheckComponentsBlendingInitialization(reInitialize);

            _anim_morphSet.RefreshWithSetup(_anim_MainSet);
        }

        internal static Rect GetMenuDropdownRect(int width = 300)
        {
            return new Rect(Event.current.mousePosition + Vector2.left * 100, new Vector2(width, 340));
        }

    }
}