using UnityEditor;
using UnityEngine;

namespace FIMSpace.Animating.AnimUtils
{
    public class FEditor_RenameSelected : EditorWindow
    {
        [MenuItem("Window/FImpossible Creations/Animating/Utilities/Copy Hierarchy Names", false, 0)]
        static void Init()
        {
            FEditor_RenameSelected window = (FEditor_RenameSelected)EditorWindow.GetWindow(typeof(FEditor_RenameSelected));

            Vector2 windowSize = new Vector2(300f, 170f);

            window.minSize = windowSize;
            window.maxSize = windowSize;

            window.autoRepaintOnSceneChange = true;
            window.titleContent = new GUIContent("Copy Names");
            window.Show();
        }

        Transform source;
        Transform targetParent;

        void OnGUI()
        {
            EditorGUILayout.BeginVertical(EditorStyles.boldLabel);

            GUILayout.Space(4);
            EditorGUILayout.LabelField("Apply same names, can be helpful when\nsetting skeleton bones names when some\nsoftware replaced dots with underscores etc.", EditorStyles.centeredGreyMiniLabel, GUILayout.Height(42));

            GUILayout.Space(8);
            source = (Transform)EditorGUILayout.ObjectField("Source:", source, typeof(Transform), true);
            targetParent = (Transform)EditorGUILayout.ObjectField("Target:", targetParent, typeof(Transform), true);

            GUILayout.Space(8);

            EditorGUILayout.LabelField("Same child count and same child order is required", EditorStyles.centeredGreyMiniLabel, GUILayout.Height(28));
            GUILayout.Space(4);

            if (GUILayout.Button("Replace"))
            {
                CallCopyNames();
            }

            GUILayout.Space(8);
            EditorGUILayout.EndVertical();
        }

        public void CallCopyNames()
        {
            var getTr = source.GetComponentsInChildren<Transform>();
            var tgtTr = targetParent.GetComponentsInChildren<Transform>();

            if (getTr.Length != tgtTr.Length)
            {
                EditorUtility.DisplayDialog("Wrong", "Different bones counts!", "Ok");
                return;
            }

            Object[] toRec = new Object[tgtTr.Length];
            for (int t = 0; t < toRec.Length; t++) toRec[t] = tgtTr[t];

            for (int t = 0; t < getTr.Length; t++)
            {
                tgtTr[t].name = getTr[t].name;
                EditorUtility.SetDirty(tgtTr[t]);
            }

            Undo.RecordObjects(toRec, "Copy Names");
        }
    }
}