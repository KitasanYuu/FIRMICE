using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenuImprove : MonoBehaviour
{
    public GameObject PauseCanvas;
    public GameObject SettingLayers;
    public GameObject QuitComfirm;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)&& PauseCanvas.activeSelf)
        {
            if (!SettingLayers.activeSelf && !QuitComfirm.activeSelf)
            {
                PauseCanvas.SetActive(!PauseCanvas.activeSelf);
            }
        }else if(Input.GetKeyDown(KeyCode.Escape) && !PauseCanvas.activeSelf)
        {
            PauseCanvas.SetActive(true);
        }

        Debug.Log(PauseCanvas.activeSelf);
    }
}
