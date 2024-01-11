using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TargetFinding;
using Avatar;

public class CursorManager : MonoBehaviour
{
    private TargetSeeker targetseeker;
    private BasicInput basicinput;
    private GameObject Player;

    // ������־����
    private bool overrideCursorLock = false;

    // ���������б��洢��Ҫ�������ģʽΪNone�ĳ�����
    public List<string> scenesWithOverride = new List<string>();

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
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
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
