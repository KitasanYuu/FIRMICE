using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Test1))]
public class YourScriptEditor : Editor
{
    private int selectedTab = 0;

    public override void OnInspectorGUI()
    {
        Test1 test1 = (Test1)target;

        GUILayout.Space(10);

        // Create tabs as buttons
        string[] tabNames = new string[] { "Tab 1", "Tab 2", "Tab 3" };
        selectedTab = GUILayout.Toolbar(selectedTab, tabNames);

        // Display different content based on selected tab
        switch (selectedTab)
        {
            case 0:
                DisplayTabContent1(test1);
                break;
            case 1:
                DisplayTabContent2(test1);
                break;
            case 2:
                DisplayTabContent3(test1);
                break;
        }
    }

    private void DisplayTabContent1(Test1 test1)
    {
        // Display content for tab 1
        EditorGUILayout.LabelField("Tab 1 Content");
        // Add more GUI elements as needed
    }

    private void DisplayTabContent2(Test1 test1)
    {
        // Display content for tab 2
        EditorGUILayout.LabelField("Tab 2 Content");
        // Add more GUI elements as needed
    }

    private void DisplayTabContent3(Test1 test1)
    {
        // Display content for tab 3
        EditorGUILayout.LabelField("Tab 3 Content");
        // Add more GUI elements as needed
    }
}
