using System.Collections;
using System.Collections.Generic;
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
       PlayerData ReadData = PresistRWTest.LoadPlayerData("Slot1");
        if(ReadData!= null)
        {
            Num = ReadData.currentNumber;
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
        PlayerData playerData = new PlayerData();
        playerData.currentNumber = Num;
        PresistRWTest.SavePlayerData(playerData,"Slot1");

    }
}
