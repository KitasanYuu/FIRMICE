using FIMSpace.Generating;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.AnimationTools
{
    public partial class AnimationDesignerWindow
    {

        [MenuItem("Assets/Open in Animation Designer", true)]
        private static bool OpenInAnimationDesignerCheck(MenuCommand menuCommand)
        {
            if (Selection.objects.Length == 0) return false;

            for (int i = 0; i < Selection.objects.Length; i++)
            {
                if (Selection.objects[i] == null) continue;
                if (Selection.objects[i] is AnimationClip) { return true; }
            }
            return false;
        }

        [MenuItem("Assets/Open in Animation Designer")]
        private static void OpenInAnimationDesigner(MenuCommand menuCommand)
        {
            if (Selection.objects.Length == 0) return;

            for (int i = 0; i < Selection.objects.Length; i++)
            {
                if (Selection.objects[i] == null) continue;
                if (Selection.objects[i] is AnimationClip) { if (Get == null) Init(); Get.TargetClip = Selection.objects[i] as AnimationClip; }
            }
        }

        [MenuItem("CONTEXT/AnimationClip/Open in Animation Designer")]
        private static void OpenClipEdit(MenuCommand menuCommand)
        {
            AnimationClip targetComponent = (AnimationClip)menuCommand.context;

            if (targetComponent)
            {
                if (Get == null) Init();
                Get.TargetClip = targetComponent;
            }
        }

        [MenuItem("CONTEXT/Animator/Open in Animation Designer")]
        private static void OpenAnimatorEdit(MenuCommand menuCommand)
        {
            Animator targetComponent = (Animator)menuCommand.context;

            if (targetComponent)
            {
                if (Get == null) Init();
                Get.AddDampReferencesEvent();
                Get.Focus();
                Get.FrameTarget(targetComponent.gameObject);
                //Get.latestAnimator = targetComponent;
            }
        }

        [MenuItem("CONTEXT/Animation/Open in Animation Designer")]
        private static void OpenLegacyAnimationEdit(MenuCommand menuCommand)
        {
            Animator targetComponent = (Animator)menuCommand.context;

            if (targetComponent)
            {
                if (Get == null) Init();
                Get.AddDampReferencesEvent();
                Get.Focus();
                Get.FrameTarget(targetComponent.gameObject);
                //Get.latestAnimator = targetComponent;
            }
        }

        [MenuItem("Window/FImpossible Creations/Animating/Animation Designer Video Tutorials...", false, 1)]
        public static void OpenWebsiteTutorials()
        {
            Application.OpenURL("https://www.youtube.com/watch?v=Q2ruYQNPHGg&list=PL6MURe5By90n1VWe-Ezs9trtl8KeQennl");
        }

        public static void OpenAnimDesignerAssetStorePage()
        {
            Application.OpenURL("https://assetstore.unity.com/publishers/37262");
        }


    }

}