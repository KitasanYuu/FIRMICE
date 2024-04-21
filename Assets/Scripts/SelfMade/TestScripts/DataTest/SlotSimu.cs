using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using YDataPersistence;

public class SlotSimu : MonoBehaviour
{
    public string CCC;
    public TextMeshProUGUI ccc;
    public DataPersistenceManager DPM;
    private InfoSpr _spr;
    private PersistDataClass _dataClass;
    // Start is called before the first frame update
    void Start()
    {
        _spr = GetComponentInParent<InfoSpr>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ParameterReceive(string verifyCode,DataPersistenceManager _dpm)
    {
        CCC = verifyCode;
        DPM = _dpm;
        ccc.text = verifyCode;
    }

    public void Deletthis()
    {
        DPM?.slotList.Remove(this);
        DPM?.DestorySlot(CCC);
        Destroy(this.gameObject);
    }

    public void Readthis()
    {
        DPM.LoadSaveSlot(CCC);
        _spr.SetNum(DPM.CurrentSaveClass.testDataClass.Number,CCC);
    }

}
