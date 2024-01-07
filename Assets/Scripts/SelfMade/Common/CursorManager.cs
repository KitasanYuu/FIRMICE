using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TargetFinding;
using Avatar;

public class CursorManager : MonoBehaviour
{
    private TargetSeeker targetseeker;
    [SerializeField]
    private BasicInput basicinput;
    [SerializeField]
    private GameObject Player;

    // Start is called before the first frame update
    void Start()
    {
        targetseeker = GetComponent<TargetSeeker>();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Update is called once per frame
    void Update()
    {
        CursorStatusChange();
        Finding();
    }

    private void CursorStatusChange()
    {
        if (Input.GetKeyDown(KeyCode.LeftAlt))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            SetTargetCursor(false);
        }
        if (Input.GetKeyUp(KeyCode.LeftAlt))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            SetTargetCursor(true);
        }
    }

    private void Finding()
    {
        if(Player == null)
        {
            targetseeker.SetStatus(true);
        }

        if(targetseeker.foundObject!= null)
        {
            Player = targetseeker.foundObject;
            targetseeker.SetStatus(false);
            basicinput = Player.GetComponent<BasicInput>();
        }
    }

    private void SetTargetCursor(bool newStatus)
    {
        if(basicinput != null)
        {
            basicinput.cursorInputForLook = newStatus;
        }
    }
}
