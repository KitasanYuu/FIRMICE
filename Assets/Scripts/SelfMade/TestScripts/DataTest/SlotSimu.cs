using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YDataPersistence;

public class SlotSimu : MonoBehaviour
{
    public string CCC;
    public DataPersistenceManager DPM;
    public InfoSpr _spr;

    private PersistDataClass _dataClass;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Select()
    {
        if (CCC != null)
        {
            _dataClass = DPM.LoadSaveSlot(CCC);
        }
        else
        {
            _dataClass = DPM.CreateSaveSlot();
            CCC = _dataClass.cryptographicCheckCode;
        }
        if (_dataClass == null)
        {
            _dataClass = DPM.CreateSaveSlot();
            CCC = _dataClass.cryptographicCheckCode;
        }
        _spr.SetNum(_dataClass.testDataClass.Number, CCC);
    }

    public void SaveData()
    {
        if(CCC != null)
        {
            _dataClass = DPM.LoadSaveSlot(CCC);
        }
        else
        {
            _dataClass = DPM.CreateSaveSlot();
            CCC = _dataClass.cryptographicCheckCode;
        }

        if(_dataClass == null)
        {
            _dataClass = DPM.CreateSaveSlot();
            CCC = _dataClass.cryptographicCheckCode;
        }
        _dataClass.testDataClass.Number = _spr.GetNum();
        PersistRWTest.SavePlayerData(_dataClass, CCC);
    }
}
