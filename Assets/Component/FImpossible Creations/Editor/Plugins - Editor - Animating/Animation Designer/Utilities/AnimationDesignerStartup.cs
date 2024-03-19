using UnityEngine;
using UnityEditor;
using FIMSpace.FEditor;
using FIMSpace.AnimationTools;

#if UNITY_EDITOR && !UNITY_CLOUD_BUILD
namespace FIMSpace.Hidden
{
    public sealed class AnimationDesignerStartup : EditorWindow
    {
        public static AnimationDesignerStartup Get;
        public UnityEngine.Object BaseDirectory;
        public UnityEngine.Object DemosPackage;
        public UnityEngine.Object ManualFile;
        public UnityEngine.Object AssemblyDefJust;
        public UnityEngine.Object AssemblyDefAll;
        public Texture2D _StartupTitle;
        public Texture2D _StartupTip1;

        [MenuItem("Window/FImpossible Creations/Animating/Animation Designer Startup Window", false, 0)]
        static void Init()
        {
            AnimationDesignerStartup window = (AnimationDesignerStartup)GetWindow<AnimationDesignerStartup>(true, "Animation Designer", true);
            Get = window;

            window.Show();
            window.position = new Rect(window.position.x + 60, window.position.y + 60, Screen.currentResolution.width * 0.21f, Screen.currentResolution.height * 0.425f);
            window.maxSize = new Vector2(800, 1000);
            EditorPrefs.SetInt("ADesStart", EditorPrefs.GetInt("ADesStart", 0) + 1);
        }

        static int framesWait = 61;
        [InitializeOnLoadMethod]
        private static void OnReload()
        {
            if (EditorPrefs.GetInt("ADesStart", 0) > 0) return;
            framesWait = 61;
            EditorApplication.update += ReloadStartup;
        }

        private static void ReloadStartup()
        {
            if (framesWait > 0)
            {
                --framesWait;
            }
            else
            {
                EditorApplication.update -= ReloadStartup;
                Init();
            }
        }

        Vector2 scroll = Vector2.zero;
        private void OnGUI()
        {
            //EditorPrefs.SetInt("PGGStart", 0);
            if (Get == null) Init();

            scroll = EditorGUILayout.BeginScrollView(scroll);
            Texture2D headerImage = _StartupTitle;
            if (headerImage)
            {
                float titleScaledWidth = EditorGUIUtility.currentViewWidth - EditorGUIUtility.standardVerticalSpacing * 4;
                float titleScaledHeight = titleScaledWidth * ((float)headerImage.height / (float)headerImage.width);
                Rect titleRect = EditorGUILayout.GetControlRect();
                titleRect.width = titleScaledWidth;
                titleRect.height = titleScaledHeight;
                GUI.DrawTexture(titleRect, headerImage, ScaleMode.ScaleToFit);
                GUILayout.Label("", GUILayout.Height(titleScaledHeight - 20));
            }

            FGUI_Inspector.DrawUILine(0.65f, 0.2f, 2, 7);
            EditorGUILayout.LabelField("ANIMATION DESIGNER STARTUP WINDOW", FGUI_Resources.HeaderStyleBig);
            FGUI_Inspector.DrawUILine(0.65f, 0.2f, 2, 7);

            if (GUILayout.Button("Open Animation Designer Window", GUILayout.Height(32))) { AnimationDesignerWindow.Init(); }


            GUILayout.Space(4);
            if (GUILayout.Button("Check Tutorial Videos")) { AnimationDesignerWindow.OpenWebsiteTutorials(); }
            if (GUILayout.Button("Check User Manual.pdf File")) { AssetDatabase.OpenAsset(ManualFile); }
            GUILayout.Space(4);
            if (GUILayout.Button("Go to Animation Designer Asset Store Page")) { AnimationDesignerWindow.OpenAnimDesignerAssetStorePage(); }

            FGUI_Inspector.DrawUILine(0.65f, 0.2f, 2, 7);

            if (GUILayout.Button("Import DEMO Package")) { AssetDatabase.ImportPackage(AssetDatabase.GetAssetPath(DemosPackage), true); }

            if (BaseDirectory != null)
                if (GUILayout.Button("Go to ANIMATION DESIGNER Directory")) { EditorGUIUtility.PingObject(BaseDirectory); }

#if UNITY_2019_4_OR_NEWER
            FGUI_Inspector.DrawUILine(0.65f, 0.2f, 2, 7);

            if (GUILayout.Button("Import Only animation Designer Assembly Definitions")) { AssetDatabase.ImportPackage(AssetDatabase.GetAssetPath(AssemblyDefJust), true); }
            if (GUILayout.Button("Import all Fimpossible Creations Assembly Definitions")) { AssetDatabase.ImportPackage(AssetDatabase.GetAssetPath(AssemblyDefAll), true); }
#endif
            FGUI_Inspector.DrawUILine(0.65f, 0.2f, 2, 7);

            EditorGUILayout.HelpBox("You can dispaly this window again by going to: Window/FImpossible Creations/Animating/Animation Designer Startup Window", MessageType.Info);
            EditorGUILayout.HelpBox("You can open Animation Designer window also by hitting right mouse button on the Animator component, you will find new menu item 'Open In Animation Designer'.", MessageType.Info);

            GUILayout.Space(4);

            headerImage = _StartupTip1;
            if (headerImage)
            {
                float titleScaledWidth = EditorGUIUtility.currentViewWidth - EditorGUIUtility.standardVerticalSpacing * 4;
                float titleScaledHeight = titleScaledWidth * ((float)headerImage.height / (float)headerImage.width);
                Rect titleRect = EditorGUILayout.GetControlRect();
                titleRect.width = titleScaledWidth;
                titleRect.height = titleScaledHeight;
                GUI.DrawTexture(titleRect, headerImage, ScaleMode.ScaleToFit);
                GUILayout.Label("", GUILayout.Height(titleScaledHeight - 20));
            }

            GUILayout.Space(4);

            EditorGUILayout.EndScrollView();
        }

    }
}
#endif