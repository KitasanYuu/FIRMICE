using YDataPersistence;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InfoSpr : MonoBehaviour
{
    public Button MinusButton;
    public Button AddButton;
    public TextMeshProUGUI Number;
    private int Num = 0;

    // Start is called before the first frame update
    void Start()
    {
        PersistDataClass ReadData = PersistRWTest.LoadPlayerData("Slot1");
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

    public void SaveData()
    {
        PersistDataClass PDC = new PersistDataClass();
        PDC.testDataClass.currentNumber = Num;
        PDC.testDataClass.Number = Num;
        PersistRWTest.SavePlayerData(PDC, "Slot1");

    }
}
