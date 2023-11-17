/// <summary>
/// This script belongs to cowsins™ as a part of the cowsins´ FPS Engine. All rights reserved. 
/// </summary>
#if UNITY_EDITOR
using UnityEditor;
#endif 
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
namespace cowsins {
/// <summary>
/// Basic Point Capture script
/// This is the core of the system.
/// </summary>
public class PointCapture : MonoBehaviour
{
    [System.Serializable]
    public class Events
    {
        public UnityEvent OnCapture;
    }
    public Events events; // Our custom events

    [Tooltip(" how fast the point will be captured "),SerializeField]
    private float captureSpeed;

    [Tooltip(" If true, progress will gradually be lost when player leaves the point ")]
    public bool loseProgressIfNoCapturing;

    [Tooltip(" Speed of progress loss "), SerializeField]
    private float losingProgressCaptureSpeed;

    private bool beingCaptured;

    private bool captured; 

    private float progress; 

    private GameObject ui;  

    private void Start()
    {
        // Initial stuff
        progress = 0;
        captured = false;
    }


    void Update()
    {
        // if player is not inside and we wanna lose progress then lose it
        if(!beingCaptured && progress > 0 && loseProgressIfNoCapturing) progress -= Time.deltaTime * losingProgressCaptureSpeed;

        // Check if we already captured
        if (progress >= 100) captured = true;

        // Do whatever we wanna do OnCapture
        if (captured) OnCapture();

        // Handle UI, we do not wanna do this if there is no UI currently (We are not inside the point)
        if (ui == null) return;

        // It will show progress while you are in
        Slider slider = ui.transform.Find("Progress").GetComponent<Slider>();
        slider.value = progress;

    }
    void OnTriggerStay(Collider other)
    {
        // If we are player and point is not captured, try to cap
        if(other.CompareTag("Player") && !captured)
        {
            if (!beingCaptured && ui == null) ui = Instantiate(Resources.Load("PointCaptureUI")) as GameObject; // Instantiate cool UI
            beingCaptured = true;
            progress += Time.deltaTime * captureSpeed;
        }
    }
    // Stop capping
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && !captured)
        {
            beingCaptured = false;
            Destroy(ui.gameObject);
        }
    }

    /// <summary>
    /// This is called whenever you capture the point
    /// </summary>
    public virtual void OnCapture()
    {
        events.OnCapture.Invoke(); // Call our custom event 

        /*Since it is a public virtual void, you can override this method on a new script that inherits from PointCapture. 
        However, do not forget to use base.OnCapture(); if
        you still want to perform these actions on that new script.
        
         EXAMPLE: on a new class (public class MyNewPointCapture : PointCapture) we will write the following structure
        public override void OnCapture() { // Write here whatever you want }
        
         (You can always edit this one if you do not plan to add more different kinds of capture points, just to avoid unncessary scripts)*/

        Debug.Log("You captured the point!");

        Destroy(ui);
        Destroy(this.gameObject);
    }
}
#if UNITY_EDITOR
[System.Serializable]
[CustomEditor(typeof(PointCapture))]
public class PointCaptureEditor : Editor
{

    override public void OnInspectorGUI()
    {
        serializedObject.Update();
        PointCapture myScript = target as PointCapture;

        EditorGUILayout.LabelField("SETTINGS", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("captureSpeed"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("loseProgressIfNoCapturing"));
        if (myScript.loseProgressIfNoCapturing)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("losingProgressCaptureSpeed"));
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
}