using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorInit : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(Cursor.visible);
        //Debug.Log(Cursor.lockState);
    }
}
