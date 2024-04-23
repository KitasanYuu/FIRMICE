using CustomInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using YDataPersistence;
using YuuTool;

public class DataPersistenceManager : MonoBehaviour
{
    [ReadOnly] public string CurrentCCC;
    public GameObject Anchor;
    public GameObject SaveUnit;

    public PersistDataClass CurrentSaveClass;

    public List<SlotSimu> slotList = new List<SlotSimu>();

    public List<PersistDataClass> dataList = new List<PersistDataClass>();


    private void Start()
    {
        dataList = PersistDataRW.GetAllSave(true);
        foreach (PersistDataClass dataClass in dataList)
        {
            GenSlot(dataClass.cryptographicCheckCode);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            //cryptographicCheckCode = YTool.GenerateRandomString(16);
            //Debug.Log(cryptographicCheckCode);
        }
    }

    public void CreateSaveSlot()
    {
        PersistDataClass data = new PersistDataClass();
        data.cryptographicCheckCode = YTool.GenerateRandomString(16);
        CurrentSaveClass = data;
        CurrentCCC = data.cryptographicCheckCode;
        GenSlot(data.cryptographicCheckCode);
        SaveStatus();
    }

    public void SaveStatus()
    {
        if(CurrentSaveClass != null)
        {
            PersistDataRW.SavePlayerData(CurrentSaveClass,DateTime.Now.ToString("yyyyMMddHHmmssfff"));
        }
    }

    public void LoadSaveSlot(string CCC)
    {
        dataList = PersistDataRW.GetAllSave(true);
        foreach (PersistDataClass data in dataList)
        {
            if(data.cryptographicCheckCode == CCC)
            {
                CurrentCCC = CCC;
                CurrentSaveClass = data;
            }
        }
    }

    private void GenSlot(string CCC)
    {
        if(SaveUnit != null)
        {
            GameObject slot = Instantiate(SaveUnit,Anchor.transform);
            SlotSimu slotSimu = slot.GetComponent<SlotSimu>();
            if(slotSimu != null)
            {
                slotSimu.ParameterReceive(CCC, this);
                if (!slotList.Contains(slotSimu))
                {
                    slotList.Add(slotSimu);
                }
            }
        }
    }

    public void DestorySlot(string CCC)
    {
        if(CurrentSaveClass != null)
        {
            if (CurrentSaveClass.cryptographicCheckCode == CCC)
            {
                CurrentSaveClass = null;
            }
        }


        PersistDataRW.DestroySave(CCC);
    }

}
