using YDataPersistence;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using CustomInspector;

public class InfoSpr : MonoBehaviour
{
    public Button MinusButton;
    public Button AddButton;
    public TextMeshProUGUI Number;
    [ReadOnly] public string SaveCode;
    private int Num = 0;

    // Start is called before the first frame update
    void Start()
    {
        PersistDataClass ReadData = PersistRWTest.LoadPlayerData(SaveCode);
        if(ReadData!= null)
        {
            Num = ReadData.testDataClass.currentNumber;
        }

        Number.text = Num.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnButtonClick(int number)
    {
        Num += number;
        Number.text = Num.ToString();
    }

    public void SetNum(int _num,string _saveCode)
    {
        SaveCode = _saveCode;
        Num = _num;
        Number.text = Num.ToString();
    }

    public int GetNum()
    {
        return Num;
    }

    public void SaveData()
    {
        PersistDataClass PDC = new PersistDataClass();
        PDC.testDataClass.currentNumber = Num;
        PDC.testDataClass.Number = Num;
        PersistRWTest.SavePlayerData(PDC, SaveCode);
    }
}
