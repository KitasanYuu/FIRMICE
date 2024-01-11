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

    // 新增标志变量
    private bool overrideCursorLock = false;

    // 新增公共列表，存储需要光标锁定模式为None的场景名
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

        // 获取当前场景名
        string currentScene = SceneManager.GetActiveScene().name;

        // 根据场景名检查是否需要覆盖光标锁定模式
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
