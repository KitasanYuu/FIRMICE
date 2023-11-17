/// <summary>
/// This script belongs to cowsins™ as a part of the cowsins´ FPS Engine. All rights reserved. 
/// </summary>
using System.Collections;
using UnityEngine;
using TMPro;
namespace cowsins { 
/// <summary>
/// 3D CHECKPOINT BASE SCRIPT
/// 2D UI script will be included in future updates.
/// </summary>
public class CheckPoint : MonoBehaviour
{
    public enum MeasureType
    {
        metres, kilometres, inches, feet, yards, miles
    }
    #region variables

        [Tooltip("Attach the text where you want the distance to be displayed"),SerializeField]
        private TextMeshProUGUI text;

        [Tooltip("Select a measure unity among the following"),SerializeField]
        private MeasureType measureType;

        [Tooltip("number of decimals to display"), Range(0, 10),SerializeField]
        private int decimals;

        [Tooltip("How fast you want the text to display the new distance"),SerializeField]
        private float updatePeriod;

        private Transform Player; // Internal use

    #endregion

    private void Start()
    {
        // SomeInitialStuff
        Player = GameObject.FindGameObjectWithTag("Player").transform;
        StartCoroutine(UpdateValue());
    }

    /// <summary>
    /// Do everything we need to display the distance on your screen
    /// </summary>
    private IEnumerator UpdateValue()
    {
        // Wait until the next time we should display our new distance
        yield return new WaitForSeconds(updatePeriod);

        // Get distance and how to display it
        // Also convert it to whatever you selected
        int measure = (int)measureType;
        float distance = 0;
        switch (measure)
        {
            case 0:
                distance = Vector3.Distance(transform.position, Player.position);
                text.text = distance.ToString("F" + decimals) + "m"; // metres
                break;
            case 1:
                distance = Vector3.Distance(transform.position, Player.position) / 1000;
                text.text = distance.ToString("F" + decimals) + "km"; // kilometres
                break;
            case 2:
                distance = Vector3.Distance(transform.position, Player.position) * 39.37f;
                text.text = distance.ToString("F" + decimals) + "inch"; // inches
                break;
            case 3:
                distance = Vector3.Distance(transform.position, Player.position) * 3.280f;
                text.text = distance.ToString("F" + decimals) + "feet"; // feet
                break;
            case 4:
                distance = Vector3.Distance(transform.position, Player.position) * 1.09f;
                text.text = distance.ToString("F" + decimals) + "yards"; // yards
                break;
            case 5:
                distance = Vector3.Distance(transform.position, Player.position) * 0.000621371192f;
                text.text = distance.ToString("F" + decimals) + "miles"; // miles
                break;
        }
        // Re do it, just in case
        StartCoroutine(UpdateValue());
    }
}
}