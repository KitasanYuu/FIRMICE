using UnityEngine;
using System.Collections;

public class WaypointMaster : MonoBehaviour
{

    public float WaypointDetectDistance;
    public float accel = 0.8f;

    // This is the the amount of velocity retained after the function "Slow()" is called.
    // Lower values cause quicker stops. A value of "1.0" will never stop. Values above "1.0" will speed up.
    public float inertia = 0.9f;
    public float speedLimit = 10.0f;

    public float minSpeed = 1.0f;

    // This is how long to pause inside "Slow()" before activating the function
    // "Accell()" to start the script again.
    public float stopTime = 1.0f;

    // This variable "currentSpeed" is the major player for dealing with velocity.
    // The "currentSpeed" is mutiplied by the variable "accel" to speed up inside the function "accell()".
    // Again, The "currentSpeed" is multiplied by the variable "inertia" to slow
    // things down inside the function "Slow()".
    private float currentSpeed = 0.0f;

    // The variable "functionState" controlls which function, "Accell()" or "Slow()",
    // is active. "0" is function "Accell()" and "1" is function "Slow()".
    private float functionState = 0;

    // The next two variables are used to make sure that while the function "Accell()" is running,
    // the function "Slow()" can not run (as well as the reverse).
    private bool accelState;
    private bool slowState;

    // This variable will store the "active" target object (the waypoint to move to).
    private Transform waypoint;

    // This is the speed the object will rotate to face the active Waypoint.
    public float rotationDamping = 6.0f;

    // If this is false, the object will rotate instantly toward the Waypoint.
    // If true, you get smoooooth rotation baby!
    public bool smoothRotation = true;

    // This variable is an array. []< that is an array container if you didnt know.
    // It holds all the Waypoint Objects that you assign in the inspector.
    public Transform[] waypoints;

    // This variable keeps track of which Waypoint Object,
    // in the previously mentioned array variable "waypoints", is currently active.
    private int WPindexPointer;

    // Functions! They do all the work.
    // You can use the built in functions found here: [url]http://unity3d.com/support/documentation/ScriptReference/MonoBehaviour.html[/url]
    // Or you can declare your own! The function "Accell()" is one I declared.
    // You will want to declare your own functions because theres just certain things that wont work in "Update()". Things like Coroutines: [url]http://unity3d.com/support/documentation/ScriptReference/index.Coroutines_26_Yield.html[/url]

    //The function "Start()" is called just before anything else but only one time.
    void Start()
    {
        // When the script starts set "0" or function Accell() to be active.
        WPindexPointer = Random.Range(0, waypoints.Length);
        functionState = 0;
    }

    //The function "Update()" is called every frame. It can get slow if overused.
    void Update()
    {
        // If functionState variable is currently "0" then run "Accell()".
        // Withouth the "if", "Accell()" would run every frame.
        if (functionState == 0)
        {
            Accell();
        }

        // If functionState variable is currently "1" then run "Slow()".
        // Withouth the "if", "Slow()" would run every frame.
        if (functionState == 1)
        {
            StartCoroutine(Slow());
        }

        waypoint = waypoints[WPindexPointer]; //Keep the object pointed toward the current Waypoint object.

        float dist = Vector3.Distance(waypoint.position, transform.position);

        if (dist < WaypointDetectDistance)
        {
            // When the GameObject collides with the waypoint's collider,
            // activate "Slow()" by setting "functionState" to "1".
            functionState = 1;

            // When the GameObject collides with the waypoint's collider,
            // change the active waypoint to the next one in the array variable "waypoints".
            WPindexPointer = Random.Range(0, waypoints.Length);
            // When the array variable reaches the end of the list ...
            if (WPindexPointer >= waypoints.Length)
            {
                // ... reset the active waypoint to the first object in the array variable
                // "waypoints" and start from the beginning.
                WPindexPointer = 0;
            }
        }
    }

    // I declared "Accell()".
    void Accell()
    {
        if (accelState == false)
        {
            // Make sure that if Accell() is running, Slow() can not run.
            accelState = true;
            slowState = false;
        }

        // I grabbed this next part from the unity "SmoothLookAt" script but try to explain more.
        if (waypoint) //If there is a waypoint do the next "if".
        {
            if (smoothRotation)
            {
                // Look at the active waypoint.
                var rotation = Quaternion.LookRotation(waypoint.position - transform.position);

                // Make the rotation nice and smooth.
                // If smoothRotation is set to "On", do the rotation over time
                // with nice ease in and ease out motion.
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * rotationDamping);
            }
        }

        // Now do the accelleration toward the active waypoint untill the "speedLimit" is reached
        currentSpeed = currentSpeed + accel * accel;
        transform.Translate(0, 0, Time.deltaTime * currentSpeed);

        // When the "speedlimit" is reached or exceeded ...
        if (currentSpeed >= speedLimit)
        {
            // ... turn off accelleration and set "currentSpeed" to be
            // exactly the "speedLimit". Without this, the "currentSpeed
            // will be slightly above "speedLimit"
            currentSpeed = speedLimit;
        }
    }


    // I declared "Slow()".
    IEnumerator Slow()
    {
        if (slowState == false) //
        {
            // Make sure that if Slow() is running, Accell() can not run.
            accelState = false;
            slowState = true;
        }

        // Begin to do the slow down (or speed up if inertia is set above "1.0" in the inspector).
        currentSpeed = currentSpeed * inertia;
        transform.Translate(0, 0, Time.deltaTime * currentSpeed);

        // When the "minSpeed" is reached or exceeded ...
        if (currentSpeed <= minSpeed)
        {
            // ... Stop the movement by setting "currentSpeed to Zero.
            currentSpeed = minSpeed;
            functionState = 0;
            // Wait for the amount of time set in "stopTime" before moving to next waypoint.
            yield return new WaitForSeconds(stopTime);
            // Activate the function "Accell()" to move to next waypoint.
            
        }
    }

}

