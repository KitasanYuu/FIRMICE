using UnityEngine;


namespace CustomInspector.Demo
{
    public class DocumentationInfo : MonoBehaviour
    {
        [MessageBox("Open the CustomInspector-EditorWindow!", MessageBoxType.Info)]
        
        [ReadOnly(DisableStyle.OnlyText, LabelStyle.NoLabel)]
            public string info1 = "See Documentation at:";

        [ReadOnly(DisableStyle.OnlyText, LabelStyle.NoLabel)]
            public string info2 = "'Window' -> CustomInspector Documentation";


        [Button(nameof(GetMoreInformation)), Tooltip("This logs an information in the console")]
        [HideField]
        public bool moreInformation = false;

        void GetMoreInformation()
        {
            Debug.Log("Window is clickable in the top bar of your unity editor window. It's the bar with 'File', 'Edit', 'Assets', etc..");
        }
    }
}

