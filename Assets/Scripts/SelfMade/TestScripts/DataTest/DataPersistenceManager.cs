using CustomInspector;
using System.Collections.Generic;
using UnityEngine;
using YDataPersistence;
using YuuTool;

public class DataPersistenceManager : MonoBehaviour
{
    [ReadOnly] public string CurrentCCC;

    public List<PersistDataClass> dataList = new List<PersistDataClass>();

    private Dictionary<string,PersistDataClass> DataSlot;

    private void Start()
    {
        dataList = PersistRWTest.GetAllSave();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            //cryptographicCheckCode = YTool.GenerateRandomString(16);
            //Debug.Log(cryptographicCheckCode);
        }
    }

    public PersistDataClass CreateSaveSlot()
    {
        PersistDataClass data = new PersistDataClass();
        data.cryptographicCheckCode = YTool.GenerateRandomString(16);
        CurrentCCC = data.cryptographicCheckCode;
        return data;
    }

    public PersistDataClass LoadSaveSlot(string CCC)
    {
        dataList = PersistRWTest.GetAllSave();
        foreach (PersistDataClass data in dataList)
        {
            if(data.cryptographicCheckCode == CCC)
            {
                return data;
            }
        }

        return null;
    }

}
