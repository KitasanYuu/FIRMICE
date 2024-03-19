using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.AnimationTools
{
    public partial class AnimationDesignerSave : ScriptableObject
    {
        internal Transform LatestAnimator;
        public Avatar TargetAvatar;

        public ADArmatureSetup Armature;
        public ADArmaturePose TPose;

        [HideInInspector] public int _Tips_RootAndHipsMakeSureCounter = 0;


        #region Settings Variables

        // Export Settings
        public bool Export_SetAllOriginalBake = true;
        public bool Export_CopyEvents = true;
        public bool Export_CopyCurves = false;
        public float Export_HipsAndLegsPrecisionBoost = 0.1f;
        public float Export_OptimizeCurves = 0.01f;
        //public float Export_AdaptBakeFramerate = 1f;
        public bool Export_IncludeRootMotionInKeyAnimation = false;
        public bool Export_BakeRootIndividually = false;
        internal AnimationCurve Export_DebugCurve;



        /// <summary> Just for generic rigs </summary>
        [NonSerialized] public Vector3 _Bake_CurrentRootMotionPos = Vector3.zero;
        /// <summary> Just for generic rigs </summary>
        [NonSerialized] public Quaternion _Bake_CurrentRootMotionRot = Quaternion.identity;

        /// <summary> Just for generic rigs </summary>
        internal void ResetRootMotionMods()
        {
            _Bake_CurrentRootMotionPos = Vector3.zero;
            _Bake_CurrentRootMotionRot = Quaternion.identity;
        }

        /// <summary> Just for generic rigs </summary>
        internal void CheckRootMotionMods(Transform t)
        {

        }


        #endregion


        #region Effects Settings Containers Handling


        public T GetSetupForClip<T>(List<T> list, AnimationClip clip, int hash) where T : IADSettings, new()
        {

            CheckForNulls(list);

            bool wasFirst = false;
            T first = default;
            for (int i = 0; i < list.Count; i++)
                if (list[i].SettingsForClip == clip) { first = list[i]; wasFirst = true; break; }


            #region Multiple Animation Design Sets Support

            if (wasFirst)
            {
                if (hash == 0) return first;
                else
                {
                    for (int i = 0; i < list.Count; i++)
                        if (list[i].SettingsForClip == clip)
                            if (list[i].SetIDHash == hash) { return list[i]; }
                }
            }

            #endregion


            T newSet = new T();
            if (list.Count == 0) hash = 0; // Initial preset is zero hash
            newSet.OnConstructed(clip, hash);
            list.Add(newSet);

            return newSet;
        }


        void CheckForNulls<T>(List<T> list) where T : IADSettings
        {
            for (int i = list.Count - 1; i >= 0; i--) if (list[i].SettingsForClip == null) list.RemoveAt(i);
        }


        /// <summary> Different settings setups for different animation clips </summary>
        public List<ADClipSettings_Main> MainSetupsForClips = new List<ADClipSettings_Main>();

        /// <summary> Gets alternated execution order list for ik limbs </summary>
        internal List<ADArmatureLimb> GetLimbsExecutionList(List<ADClipSettings_IK.IKSet> ikSet)
        {
            for (int l = 0; l < Limbs.Count; l++) Limbs[l].AlternateExecutionIndex = -2;
            if (ikSet.Count != Limbs.Count) return Limbs;

            bool isAlt = false;
            for (int l = 0; l < ikSet.Count; l++) if (ikSet[l].UseAlternateExecutionIndex) { Limbs[l].AlternateExecutionIndex = ikSet[l].AlternateExecutionIndex; isAlt = true; }

            if (!isAlt) return Limbs;

            if (_AltExecutionOrderLimbs.Count != Limbs.Count)
            {
                _AltExecutionOrderLimbs.Clear();
                for (int l = 0; l < Limbs.Count; l++) _AltExecutionOrderLimbs.Add(Limbs[l]);
            }

            _AltExecutionOrderLimbs.Sort((a, b) => a.GetExucutionIndex.CompareTo(b.GetExucutionIndex));

            return _AltExecutionOrderLimbs;
        }

        /// <summary> Different settings setups for different animation clips </summary>
        public List<ADClipSettings_Elasticness> ElasticnessSetupsForClips = new List<ADClipSettings_Elasticness>();

        /// <summary> Different settings setups for different animation clips </summary>
        public List<ADClipSettings_Modificators> ModificatorsSetupsForClips = new List<ADClipSettings_Modificators>();

        /// <summary> Different settings setups for different animation clips </summary>
        public List<ADClipSettings_IK> IKSetupsForClips = new List<ADClipSettings_IK>();

        /// <summary> Different settings setups for different animation clips </summary>
        public List<ADClipSettings_CustomModules> CustomModuleSetupsForClips = new List<ADClipSettings_CustomModules>();


        //public List<ADClipSettings_Springs> SpringSetupsForClips = new List<ADClipSettings_Springs>();

        /// <summary> Different settings setups for different animation clips </summary>
        public List<ADClipSettings_Morphing> MorphingSetupsForClips = new List<ADClipSettings_Morphing>();



        internal void RemoveSaveDataForClip(AnimationClip toRemove)
        {
            RemoveSetupOfClip(MainSetupsForClips, toRemove);
            RemoveSetupOfClip(ElasticnessSetupsForClips, toRemove);
            RemoveSetupOfClip(ModificatorsSetupsForClips, toRemove);
            RemoveSetupOfClip(IKSetupsForClips, toRemove);
            RemoveSetupOfClip(CustomModuleSetupsForClips, toRemove);
            //RemoveSetupOfClip(SpringSetupsForClips, toRemove);
            RemoveSetupOfClip(MorphingSetupsForClips, toRemove);
        }


        public void RemoveSetupOfClip<T>(List<T> list, AnimationClip clip) where T : IADSettings, new()
        {
            CheckForNulls(list);

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].SettingsForClip == clip)
                {
                    list.RemoveAt(i);
                    return;
                }
            }
        }


        #endregion


        #region Additional Designer Sets



        internal int GetCountOfDesignerSetsFor(AnimationClip targetClip)
        {
            int count = 0;

            for (int i = 0; i < MainSetupsForClips.Count; i++)
            {
                if (MainSetupsForClips[i].SettingsForClip == targetClip) count++;
            }

            return 0;
        }


        internal List<ADClipSettings_Main> GetAllDesignerSetsFor(AnimationClip targetClip)
        {
            List<ADClipSettings_Main> mains = new List<ADClipSettings_Main>();
            ADClipSettings_Main rootset = null;

            for (int i = 0; i < MainSetupsForClips.Count; i++)
            {
                if (MainSetupsForClips[i].SettingsForClip == targetClip)
                {
                    if (MainSetupsForClips[i].SetIDHash == 0) rootset = MainSetupsForClips[i];
                    mains.Add(MainSetupsForClips[i]);
                }
            }

            if (rootset != null)
            {
                mains.Remove(rootset);
                mains.Insert(0, rootset);
            }

            return mains;
        }

        #endregion


        #region Main Utilities


        public float ScaleRef { get; private set; }
        public Transform SkelRootBone { get { if (Armature == null) return null; if (Armature.RootBoneReference == null) return null; return Armature.RootBoneReference.TempTransform; } }
        public Transform ReferencePelvis { get { if (Armature == null) return null; if (Armature.PelvisBoneReference == null) return null; return Armature.PelvisBoneReference.TempTransform; } }
        [HideInInspector] public AnimationClip LatestCorrect;

        public void UpdateReferences()
        {
            ScaleRef = 0.5f;

            if (Armature != null)
                if (Armature.RootBoneReference != null)
                    if (SkelRootBone && ReferencePelvis) ScaleRef = Vector3.Distance(SkelRootBone.position, ReferencePelvis.position);
        }

        public Vector3 ToLocal(Vector3 p)
        {
            return LatestAnimator.transform.InverseTransformPoint(p);
        }

        public Vector3 ToWorld(Vector3 loc)
        {
            return LatestAnimator.transform.TransformPoint(loc);
        }

        public Transform T { get { return LatestAnimator.transform; } }
        public Transform An { get { return LatestAnimator; } }

        internal void GetArmature(bool askForOverwrite = false)
        {
            if (LatestAnimator == null) return;
            if (SkelRootBone == null) return;

            if (Armature != null)
            {
                if (askForOverwrite)
                {
                    if (!EditorUtility.DisplayDialog("Re-create Aramature?", "Do you want to erase current armature setup (limbs) and generate it again?", "Yes", "No"))
                    {
                        return;
                    }
                }
            }

            Armature = new ADArmatureSetup();

            RefreshSkeleton(LatestAnimator);
            Armature.Prepare(LatestAnimator);

            if (TPose.BonesCoords.Count != Armature.BonesSetup.Count) CaptureTPose();

        }

        internal void GatherBones()
        {
            DampSessionSkeletonReferences(false);
            Armature.GatherBones(LatestAnimator);
            Armature.RefreshBonesSpecifics(LatestAnimator);
            RefreshLimbsReferences(Armature);
            AnimationDesignerWindow.AddEditorEvent(() => { AnimationDesignerWindow.ReInitializeCalibration(); });
            OnAfterRefreshSkeleton();
        }

        public Bounds InitialBounds { get; private set; }

        void OnAfterRefreshSkeleton()
        {
            if (Armature == null) return;
            if (Armature.LatestAnimator == null) return;

            Vector3 prePos = Armature.LatestAnimator.position;
            Armature.LatestAnimator.position = Vector3.zero;

            if (TPose != null) TPose.RestoreOn(Armature);
            InitialBounds = Armature.CalculateBounds();

            Armature.LatestAnimator.position = prePos;
        }


        internal void CaptureTPose()
        {
            TPose = new ADArmaturePose();
            TPose.CopyWith(Armature);
        }

        internal void RestoreTPose()
        {
            TPose.RestoreOn(Armature);
        }

        internal void _SetDirty()
        {
            UnityEditor.EditorUtility.SetDirty(this);
        }

        internal Transform GetBoneByName(string alignToBoneName)
        {
            //if (Armature == null) return null;
            //if (Armature.BonesSetup == null) return null;
            //if (Armature.BonesSetup == null) return null;

            for (int i = 0; i < Armature.BonesSetup.Count; i++)
            {
                if (Armature.BonesSetup[i].TempTransform == null) continue;

                if (Armature.BonesSetup[i].TempTransform.name == alignToBoneName)
                {
                    return Armature.BonesSetup[i].TempTransform;
                }
            }

            // Not found in armature - check whole hierarchy
            foreach (Transform t in LatestAnimator.GetComponentsInChildren<Transform>())
            {
                if (t.name == alignToBoneName) return t;
            }

            return null;
        }

        internal bool TrySettingPoseFromObject(GameObject tPosePF)
        {
            if (tPosePF == null) return false;
            if (Armature == null) return false;

            Transform tempAnimatorTr = null;
            Animator tempAnimator = FTransformMethods.FindComponentInAllChildren<Animator>(tPosePF.transform);

            if (tempAnimator == null)
            {
                if (Armature.RootBoneReference != null)
                    if (Armature.Root)
                    {
                        Transform r = FTransformMethods.FindChildByNameInDepth(Armature.Root.name, tPosePF.transform);
                        if (r) if (r.parent) tempAnimatorTr = r.parent;
                        if (tempAnimatorTr) if (tempAnimatorTr.parent) tempAnimatorTr = tempAnimatorTr.parent;
                    }
            }
            else
                tempAnimatorTr = tempAnimator.transform;

            if (tempAnimatorTr == null) return false;

            if (Armature.RootBoneReference.TempTransform == null) return false;
            if (Armature.PelvisBoneReference.TempTransform == null) return false;

            ADArmatureSetup tempArmature = new ADArmatureSetup();

            string name = Armature.RootBoneReference.TempTransform.name;
            Transform findRoot = FTransformMethods.FindChildByNameInDepth(name, tempAnimatorTr.transform);
            if (findRoot == null) return false;
            tempArmature.SetRootBoneRef(findRoot);

            name = Armature.PelvisBoneReference.TempTransform.name;
            Transform findPelv = FTransformMethods.FindChildByNameInDepth(name, tempAnimatorTr.transform);
            if (findPelv == null) return false;
            tempArmature.SetPelvisRef(findPelv);

            tempArmature.Prepare(tempAnimatorTr.transform);
            tempArmature.GatherBones(tempAnimatorTr.transform);
            tempArmature.RefreshBonesSpecifics(tempAnimatorTr.transform);

            if (tempArmature.BonesSetup.Count != Armature.BonesSetup.Count)
            {
                UnityEngine.Debug.Log("[Animator Designer] Wrong Bones Count! " + tPosePF.name + " Contains " + tempArmature.BonesSetup.Count + " bones instead of " + Armature.BonesSetup.Count + " !");
                return false;
            }

            ADArmaturePose tempTPose = new ADArmaturePose();
            tempTPose.CopyWith(tempArmature);
            tempTPose.RestoreOn(Armature);

            return true;
        }


        internal void DampSessionSkeletonReferences(bool dampLatestAnimator = true)
        {
            if (dampLatestAnimator)
                LatestAnimator = null;

            for (int i = 0; i < Limbs.Count; i++)
            {
                Limbs[i].DampSessionReferences();
            }

            for (int i = 0; i < MainSetupsForClips.Count; i++)
            {
                MainSetupsForClips[i].DampSessionReferences();
            }

            if (Armature != null) Armature.DampSessionReferences(dampLatestAnimator);

            for (int i = 0; i < Modificators.Count; i++)
            {
                Modificators[i].DampReferences();
            }

            for (int i = 0; i < ModificatorsSetupsForClips.Count; i++)
            {
                ModificatorsSetupsForClips[i].DampReferences();
            }

        }





        internal void RefreshSkeleton(Transform anim)
        {
            if (SkelRootBone != null) return;
            if (anim == null) return;

            Transform searchForBone = null;
            LatestAnimator = anim;


            if (LatestAnimator.IsHuman())
            {
                Animator a = LatestAnimator.GetComponent<Animator>();
                Armature.SetPelvisRef(LatestAnimator.GetAnimator().GetBoneTransform(HumanBodyBones.Hips)); _SetDirty();
                if (ReferencePelvis != null) searchForBone = ReferencePelvis.parent;
            }

            bool pelvSet = false;

            if (searchForBone == null)
            {
                searchForBone = GetLowestRootBoneOfSkinnedMeshesIn(anim);

                if (searchForBone)
                    if (ReferencePelvis == null)
                    {
                        if (SkeletonRecognize.NameContains(searchForBone.name, SkeletonRecognize.PelvisNames))
                        {
                            Armature.SetPelvisRef(searchForBone); _SetDirty();
                            pelvSet = true;
                        }
                    }
            }

            if (searchForBone != null)
            {
                bool rootDefined = false;



                if (Armature.RootBoneReference != null)
                    if (Armature.RootBoneReference.TempTransform) rootDefined = true;
                    else
                    {
                        Armature.RootBoneReference.GatherTempTransform(anim, null);
                        if (Armature.RootBoneReference.TempTransform) rootDefined = true;
                    }

                if (!rootDefined)
                {
                    if (pelvSet)
                    {
                        Armature.SetRootBoneRef(Armature.PelvisBoneReference.TempTransform.parent);
                    }
                    else
                        Armature.SetRootBoneRef(searchForBone);
                }


                if (Armature.PelvisBoneReference != null)
                    if (Armature.PelvisBoneReference.TempTransform == null)
                    {
                        Armature.PelvisBoneReference.GatherTempTransform(anim, null);
                    }

                if (ReferencePelvis == null) { Armature.SetPelvisRef(searchForBone.GetChild(0)); _SetDirty(); }


                if (ReferencePelvis != null)
                {
                    if (ReferencePelvis.transform != null)
                    {
                        Transform nRoot = ReferencePelvis.transform.parent;
                        bool setotherRoot = false;

                        if (nRoot != null)
                        {
                            if (nRoot.parent != null)
                            {
                                if (nRoot.parent != anim)
                                {
                                    if (SkeletonRecognize.NameContains(nRoot.parent.name, SkeletonRecognize.RootNames))
                                    {
                                        nRoot = nRoot.parent;
                                        setotherRoot = true;
                                    }
                                }
                            }
                        }

                        if (ReferencePelvis.transform == SkelRootBone.transform || setotherRoot)
                        {
                            Armature.SetRootBoneRef(nRoot);
                        }
                    }
                }

            }
        }




        internal void CopySettingsFromTo(AnimationClip fromClip, int from, int to, AnimationDesignerSave toSave, AnimationClip toClip)
        {
            ADClipSettings_Main myMain = GetSetupForClip(MainSetupsForClips, fromClip, from);
            if (toClip == null) toClip = fromClip;

            for (int i = 0; i < toSave.MainSetupsForClips.Count; i++)
                if (toSave.MainSetupsForClips[i].SetIDHash == to && MainSetupsForClips[i].SettingsForClip == toClip) { myMain.Copy(toSave.MainSetupsForClips[i], true); break; }

            ADClipSettings_IK myIK = GetSetupForClip(IKSetupsForClips, fromClip, from);
            for (int i = 0; i < toSave.IKSetupsForClips.Count; i++)
                if (toSave.IKSetupsForClips[i].SetIDHash == to && IKSetupsForClips[i].SettingsForClip == toClip) { toSave.IKSetupsForClips[i] = myIK.Copy(IKSetupsForClips[i], true); break; }

            ADClipSettings_Elasticness myElast = GetSetupForClip(ElasticnessSetupsForClips, fromClip, from);
            for (int i = 0; i < toSave.ElasticnessSetupsForClips.Count; i++)
                if (toSave.ElasticnessSetupsForClips[i].SetIDHash == to && ElasticnessSetupsForClips[i].SettingsForClip == toClip) { toSave.ElasticnessSetupsForClips[i] = myElast.Copy(ElasticnessSetupsForClips[i], true); break; }

            ADClipSettings_Modificators myMods = GetSetupForClip(ModificatorsSetupsForClips, fromClip, from);
            for (int i = 0; i < toSave.ModificatorsSetupsForClips.Count; i++)
                if (toSave.ModificatorsSetupsForClips[i].SetIDHash == to && ModificatorsSetupsForClips[i].SettingsForClip == toClip) { toSave.ModificatorsSetupsForClips[i] = myMods.Copy(ModificatorsSetupsForClips[i], toSave, true); break; }

            ADClipSettings_Morphing myMorphs = GetSetupForClip(MorphingSetupsForClips, fromClip, from);
            for (int i = 0; i < toSave.MorphingSetupsForClips.Count; i++)
                if (toSave.MorphingSetupsForClips[i].SetIDHash == to && MorphingSetupsForClips[i].SettingsForClip == toClip) { toSave.MorphingSetupsForClips[i] = myMorphs.Copy(MorphingSetupsForClips[i], toSave, true); break; }

            //_anim_modSet = S.GetSetupForClip(S.ModificatorsSetupsForClips, TargetClip, _toSet_SetSwitchToHash); //_anim_modSet = S.GetModificatorsSetupForClip(TargetClip);
            //_anim_modSet.CheckInitialization(S, reInitialize, _anim_MainSet);

            if (AnimationDesignerWindow.Get)
            {
                AnimationDesignerWindow.Get.ResetComponentsStates(true);
            }

        }


        internal List<AnimationClip> GetAllEditedClipsReferences()
        {
            List<AnimationClip> clps = new List<AnimationClip>();

            for (int i = 0; i < MainSetupsForClips.Count; i++)
            {
                AnimationClip c = MainSetupsForClips[i].SettingsForClip;
                if (c == null) continue;
                if (clps.Contains(c)) continue;
                clps.Add(c);
            }

            return clps;
        }


        public Transform GetLowestRootBoneOfSkinnedMeshesIn(Transform animatorTr)
        {
            Transform rootB = null;
            List<SkinnedMeshRenderer> skins = FTransformMethods.FindComponentsInAllChildren<SkinnedMeshRenderer>(animatorTr, true);

            if (skins.Count == 0)
            {
                UnityEngine.Debug.Log("[Animation Designer] No skinned mesh renderers found in " + animatorTr.name + "!");
            }
            else
            {
                #region Lowest root bone

                int lowest = int.MaxValue;

                for (int i = 0; i < skins.Count; i++)
                {
                    Transform rt = skins[i].rootBone;
                    int depth = ADBoneReference.GetDepth(rt, animatorTr, -1);
                    if (depth != -1)
                    {
                        if (depth < lowest)
                        {
                            lowest = depth;
                            rootB = rt;
                        }
                    }
                }

                #endregion
            }

            #region Bottom most chain

            if (rootB == null)
            {
                // Trying to find parent of longest transforms chain
                Transform bottom = SkeletonRecognize.GetBottomMostChildTransform(animatorTr);
                if (bottom)
                {
                    Transform rt = bottom;

                    if (rt)
                        if (rt.parent)
                        {
                            while (rt != null)
                            {
                                if (rt.parent == animatorTr) break;
                                rt = rt.parent;
                            }

                            rootB = rt.parent;
                        }
                }
            }

            #endregion

            if (rootB == null) UnityEngine.Debug.Log("[Animation Designer] Root bone not found! Your bones hierarchy seems to not exist!");

            return rootB;
        }


        public Transform SearchForBoneInAllAnimatorChildren(string s_ArmatureParentName)
        {
            if (LatestAnimator == null) return null;
            return FTransformMethods.FindChildByNameInDepth(s_ArmatureParentName, LatestAnimator);
        }


        #endregion

    }
}
