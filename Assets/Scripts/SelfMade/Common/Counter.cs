using CustomInspector;
using System.Collections.Generic;
using UnityEngine;

public class Counter : MonoBehaviour
{
    [SerializeField,ReadOnly]
    private int Count;
    [SerializeField, ReadOnly]
    private List<GameObject> List = new List<GameObject>();

    void Update()
    {
        // 获取当前物体的子物体数量
        Count = transform.childCount;

        List.Clear();
        for (int i = 0; i < Count; i++)
        {
            // 将子物体添加到List中
            List.Add(transform.GetChild(i).gameObject);
        }
    }
}