#pragma warning disable 0618
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TargetFinding;
using AvatarMain;
using Unity.VisualScripting;

public class CursorManager : MonoBehaviour
{
    [SerializeField]
    private GameObject PauseMenu;
    private TargetSeeker targetseeker;
    private BasicInput basicinput;
    private GameObject Player;

    // ������־����
    private bool overrideCursorLock = false;

    // ���������б��洢��Ҫ�������ģʽΪNone�ĳ�����
    public List<string> scenesWithOverride = new List<string>();

    private bool MenuOpen;

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

        //Debug.Log(Cursor.visible);
    }

    private void CursorStatusChange()
    {
        SpecialSituation();
        KeyBoardInputSituation();
    }

    private void SpecialSituation()
    {


        if (PauseMenu.active)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            SetTargetCursor(false);
            MenuOpen = true;
        }

        if(!PauseMenu.active && MenuOpen)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            SetTargetCursor(true);
            MenuOpen=false;
        }
    }

    private void KeyBoardInputSituation()
    {
        if (Input.GetKeyDown(KeyCode.LeftAlt))
        {
            if (!overrideCursorLock)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                SetTargetCursor(false);
            }
        }
        if (Input.GetKeyUp(KeyCode.LeftAlt))
        {
            if (!overrideCursorLock)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                SetTargetCursor(true);
            }
        }
    }

    private void Finding()
    {
        if (Player == null)
        {
            targetseeker.SetStatus(true);
        }

        if (targetseeker.foundObject != null)
        {
            Player = targetseeker.foundObject;
            targetseeker.SetStatus(false);
            basicinput = Player.GetComponent<BasicInput>();
        }

        // ��ȡ��ǰ������
        string currentScene = SceneManager.GetActiveScene().name;

        // ���ݳ���������Ƿ���Ҫ���ǹ������ģʽ
        if (scenesWithOverride.Contains(currentScene))
        {
            overrideCursorLock = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            SetTargetCursor(false);
        }
        else
        {
            overrideCursorLock = false;
        }
    }

    private void SetTargetCursor(bool newStatus)
    {
        if (basicinput != null)
        {
            basicinput.cursorInputForLook = newStatus;
        }
    }
}
