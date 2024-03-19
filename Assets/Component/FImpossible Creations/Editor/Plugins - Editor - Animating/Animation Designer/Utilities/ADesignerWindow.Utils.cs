using FIMSpace.Generating;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.AnimationTools
{
    public partial class AnimationDesignerWindow
    {
        public Texture _Tex_Tip1;


        public static string _SelectorHelperId = "";

        public static void ShowHumanoidBonesSelector(string title, Animator anim, Rect searchableRect, List<Transform> nonHumanoids = null, bool includeHeadBones = false, bool includeFingers = false, bool includeToes = false, string prefix = "Humanoid Bones/", string oPrefix = "Other Bones/")
        {

            #region Collect Transforms Names Setup

            List<Transform> bones = new List<Transform>();
            List<string> names = new List<string>();

            if (anim.isHuman)
            {
                for (int i = 0; i < (int)HumanBodyBones.LastBone; i++)
                {
                    Transform t = anim.GetBoneTransform((HumanBodyBones)i);
                    if (t == null) continue;

                    string name = prefix;

                    if (i == 0)
                    {
                        name += "Spine/" + ((HumanBodyBones)i).ToString();
                    }
                    else if (i >= 1 && i <= 6)
                    {
                        name += "Legs/" + ((HumanBodyBones)i).ToString();
                    }
                    else if (i >= 7 && i <= 10)
                    {
                        name += "Spine/" + ((HumanBodyBones)i).ToString();
                    }
                    else if (i >= 11 && i <= 18)
                    {
                        name += "Arms/" + ((HumanBodyBones)i).ToString();
                    }
                    else if (i >= 19 && i <= 20)
                    {
                        if (!includeToes) continue;
                        name += "Legs/" + ((HumanBodyBones)i).ToString();
                    }
                    else if (i >= 21 && i <= 23)
                    {
                        if (!includeHeadBones) continue;
                        name += "Head/" + ((HumanBodyBones)i).ToString();
                    }
                    else if (i >= 24 && i <= 38)
                    {
                        if (!includeFingers) continue;
                        name += "Left Hand Fingers/" + ((HumanBodyBones)i).ToString();
                    }
                    else if (i >= 39 && i <= 53)
                    {
                        if (!includeFingers) continue;
                        name += "Right Hand Fingers/" + ((HumanBodyBones)i).ToString();
                    }

                    bones.Add(t);
                    names.Add(name);
                }
            }
            else
            {
                return;
            }

            #endregion

            ArrangeBonesNames(bones, names, nonHumanoids, anim, oPrefix);

            DisplayMenu(title, bones, names, searchableRect);

        
        }


        #region Undo Related

        void StartUndoCheckFor(UnityEngine.Object obj, string action = "")
        {
            if (!EnableExperimentalUndo) return;
            Undo.RecordObject(obj, "Animation Designer" + action);
            EditorGUI.BeginChangeCheck();
        }

        void StartUndoCheck(string action = "", bool captureWindow = false)
        {
            if (!EnableExperimentalUndo) return;

            if (!captureWindow)
                Undo.RecordObject(S, "Animation Designer Change" + action);
            else
                Undo.RecordObjects(new UnityEngine.Object[] { S, this }, "Animation Designer Change" + action);

            EditorGUI.BeginChangeCheck();
        }

        void EndUndoCheck(bool captureWindow = false)
        {
            if (!EnableExperimentalUndo) return;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.ClearUndo(S);
                if (captureWindow) Undo.ClearUndo(this);
            }
            else
            {
                //Undo.FlushUndoRecordObjects();
            }
        }

        void EndUndoCheckFor(UnityEngine.Object obj)
        {
            if (!EnableExperimentalUndo) return;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.ClearUndo(obj);
            }
        }

        #endregion


#if UNITY_EDITOR

        public static bool AnimatorHasParam(UnityEditor.Animations.AnimatorController anim, string param)
        {
            int hash = Animator.StringToHash(param);

            if (Application.isPlaying == false)
            {
                foreach (AnimatorControllerParameter p in anim.parameters)
                    if (p.nameHash == hash) return true;

                return false;
            }

            return false;

        }

#endif


        public static bool AnimatorHasParam(Animator anim, string param)
        {
            int hash = Animator.StringToHash(param);

#if UNITY_EDITOR
            if (Application.isPlaying == false)
            {
                UnityEditor.Animations.AnimatorController aContr = (UnityEditor.Animations.AnimatorController)anim.runtimeAnimatorController;

                foreach (AnimatorControllerParameter p in aContr.parameters)
                    if (p.nameHash == hash) return true;

                return false;
            }
#endif

            foreach (AnimatorControllerParameter p in anim.parameters)
                if (p.nameHash == hash) return true;

            return false;
        }



        public static void ArrangeBonesNames(List<Transform> currentList, List<string> currentNamesList, List<Transform> toInclude, Animator anim, string prefix = "Bones/")
        {
            if (toInclude == null) return;
            if (toInclude.Count == 0) return;
            if (currentList.Count == 0) return;
            if (currentNamesList.Count == 0) return;

            for (int i = 0; i < toInclude.Count; i++)
            {
                Transform t = toInclude[i];
                if (t == null) continue;

                string name = prefix + t.name;

                currentList.Add(t);
                currentNamesList.Add(name);
            }
        }


        public static void DisplayMenu<T>(string title, List<T> bones, List<string> names, Rect searchableRect, bool onlyGenericMenu = false) where T : class
        {

#if UNITY_2019_4_OR_NEWER


            if (onlyGenericMenu)
            {
                GenericMenu clipsMenu = new GenericMenu();

                for (int i = 0; i < bones.Count; i++)
                {
                    var bn = bones[i];
                    clipsMenu.AddItem(new GUIContent(names[i]), false, () => { Searchable.Choose(bn); });
                }

                clipsMenu.ShowAsContext();
            }
            else
            {
                var dropdown = new SearchableDropdown<T>(bones, names, title);
                dropdown.Show(searchableRect);
            }

#else
            GenericMenu clipsMenu = new GenericMenu();

            for (int i = 0; i < bones.Count; i++)
            {
                var bn = bones[i];
                clipsMenu.AddItem(new GUIContent(names[i]), false, () => { Searchable.Choose(bn); });
            }

            clipsMenu.ShowAsContext();
#endif
        }


        public static void ShowBonesSelector(string title, List<Transform> includeJust, Rect searchableRect, bool addNone = false, AnimationDesignerSave save = null)
        {

            #region Collect Transforms Names Setup

            List<Transform> bones = new List<Transform>();
            List<string> names = new List<string>();

            if (addNone)
            {
                names.Add("Bones/None");
                bones.Add(null);
                names.Add("None");
                bones.Add(null);
            }

            for (int i = 0; i < includeJust.Count; i++)
            {
                Transform t = includeJust[i];
                if (t == null) continue;

                string targetName = t.name;

                #region Helper Limb Name Postfix

                if (save != null)
                    for (int l = 0; l < save.Limbs.Count; l++)
                    {
                        var limb = save.Limbs[l];
                        for (int b = 0; b < limb.Bones.Count; b++)
                            if (limb.Bones[b].T == t)
                                targetName += " (" + limb.LimbName + "[" + b + "])";
                    }

                #endregion


                string name = "Bones/" + targetName;

                bones.Add(t);
                names.Add(name);
            }

            #endregion

            DisplayMenu(title, bones, names, searchableRect);

        }

        public static void ShowElementsSelector<T>(string title, List<T> include, List<string> names, Rect searchableRect, bool addNone = false) where T : class
        {
            DisplayMenu(title, include, names, searchableRect);
        }

        internal static void ReInitializeCalibration()
        {
            if (Get == null) return;
            if (Get.S == null) return;
            ForceTPose();
            for (int i = 0; i < Get.S.Limbs.Count; i++) Get.S.Limbs[i].CheckComponentsBlendingInitialization(true);
        }
    }


    public static class AnimationDesignerExtensionMethods
    {
        /// <summary> Checking if there is Animator component and if it is human (true) else false</summary>
        public static bool IsHuman(this Transform t)
        {
            if (t == null) return false;
            Animator a = t.GetComponent<Animator>();
            if (a) return a.isHuman;
            return false;
        }

        public static Animator GetAnimator(this Transform t)
        {
            if (t == null) return null;
            return t.GetComponent<Animator>();
        }

        public static Avatar GetAvatar(this Transform t)
        {
            Animator a = GetAnimator(t);
            if (a) return a.avatar;
            return null;
        }

        public static bool UsingRootMotion(this Transform t)
        {
            Animator a = GetAnimator(t);
            if (a) return a.applyRootMotion;
            return false;
        }
    }
}