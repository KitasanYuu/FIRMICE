#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace FIMSpace.FTools
{
    [CreateAssetMenu(fileName = "Animation Clips Pack", menuName = "FImpossible Creations/Utilities/Animation Clips Pack", order = 400)]
    public class FAnimationClipsPack : FContainerBase
    {

        public override void AddAsset(Object obj)
        {
            if (obj == null) return;
            bool isRightType = false;

            if (obj is AnimationClip) isRightType = true;

            if (!isRightType)
            {
                UnityEngine.Debug.Log("[Animation Clips Pack] Wrong asset type! You're trying to add '" + obj.GetType() + "'!'");
                return;
            }

            base.AddAsset(obj);
        }
    }

    #region Inspector window draw

#if UNITY_EDITOR
    [CanEditMultipleObjects, CustomEditor(typeof(FAnimationClipsPack))]
    public class FAnimationClipsPackEditor : FContainerBaseEditor
    {
        public FAnimationClipsPack Get { get { if (_get == null) _get = (FAnimationClipsPack)target; return _get; } }
        private FAnimationClipsPack _get;
        GUILayoutOption[] g_opt = null;

        protected override Color DragAndDropBoxColor { get { return new Color(0.2f, 1f, .6f, 0.45f); } }
        protected override string HeaderInfo { get { return "Simple file in which you can keep multiple Animation Clip files to avoid mess in project directory."; } }
        protected override string MainDragAndDropText { get { return "  Drag & Drop your Animation Clips here"; ; } }
        protected override bool DrawDragAndDropToCopy { get { return true; } }
        protected override bool DrawDragAndDropToRemove { get { return true; } }

        protected override void GUIBody()
        {
            float maxW = EditorGUIUtility.currentViewWidth;
            int width = 0;
            int size = 72;
            if (g_opt == null) g_opt = new GUILayoutOption[2];
            g_opt[0] = GUILayout.Width(size);
            g_opt[1] = GUILayout.Height(size);

            GUILayout.BeginHorizontal();
            for (int i = 0; i < Get.ContainedAssets.Count; i++)
            {
                var mt = Get.ContainedAssets[i];
                if ((mt.Reference is AnimationClip) == false) continue;
                AnimationClip clp = mt.Reference as AnimationClip;

                GUIContent gct = new GUIContent(mt.Reference.name);
                float bWth = EditorStyles.miniButton.CalcSize(gct).x;

                width += (int)bWth + 2;
                if (width > maxW - 14)
                {
                    width = size + 2;
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                }

                if (GUILayout.Button(mt.Reference.name, GUILayout.Width(bWth-1)))
                {
                    Selection.activeObject = mt.Reference;
                }

            }

            GUILayout.EndHorizontal();
        }


    }
#endif

    #endregion

}