#pragma warning disable 0618
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TargetFinding;
using AvatarMain;
using CustomInspector;

public class CursorManager : MonoBehaviour
{
    [SerializeField]
    private GameObject PauseMenu;
    [SerializeField, ReadOnly]
    private GameObject Player;
    private TargetSeeker targetseeker;
    private BasicInput basicinput;

    // 新增标志变量
    private bool overrideCursorLock = false;

    // 新增公共列表，存储需要光标锁定模式为None的场景名
    public List<string> scenesWithOverride = new List<string>();
    public List<string> scenesDisablePlayerSearch = new List<string>();

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
        // 获取当前场景名
        string currentScene = SceneManager.GetActiveScene().name;

        if(!scenesDisablePlayerSearch.Contains(currentScene))
        {
            if (Player == null)
            {
                if (targetseeker == null)
                {
                    targetseeker = gameObject.AddComponent<TargetSeeker>();
                    targetseeker.objectTagToFind = "Player";
                    targetseeker.objectLayerToFind = LayerMask.GetMask("Player");
                }
            }

            if (targetseeker.foundObject != null)
            {
                Player = targetseeker.foundObject;
                basicinput = Player.GetComponent<BasicInput>();

                if (targetseeker != null)
                    Destroy(targetseeker);
            }
        }

        if (scenesDisablePlayerSearch.Contains(currentScene))
        {
            if(targetseeker!= null)
                Destroy (targetseeker);
        }

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
