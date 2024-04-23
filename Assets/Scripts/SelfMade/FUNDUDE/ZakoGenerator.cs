using CustomInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZakoGenerator : MonoBehaviour
{
    public GameObject zako;
    

    [Button(nameof(GenerateZako),true)]
    public int _genNum;

    void GenerateZako(int n)
    {
        for (int i = 0; i < n; i++)
        {
            Instantiate(zako, transform);
        }
    }

    [Button(nameof(DestoryAllZako))]
    [HideField]
    public bool _bool;
    void DestoryAllZako()
    {
        foreach (Transform child in transform)
        {
            GameObject.Destroy(child.gameObject);
        }
    }


    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
