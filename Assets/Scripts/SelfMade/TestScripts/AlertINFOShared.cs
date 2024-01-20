using CustomInspector;
using System.Collections.Generic;
using TestField;
using UnityEngine;

[RequireComponent(typeof(AlertLogic))]
public class AlertINFOShared : MonoBehaviour
{
    [ReadOnly,SerializeField]private List<GameObject> OtherReceiver = new List<GameObject>();


    private BroadCasterInfoContainer BCIC;
    //private BroadCasterInfoContainer BCIC;

    private void Awake()
    {
        ComponentInit();
    }

    private void Start()
    {

        EventSubscribe();
    }

    // Update is called once per frame
    private void Update()
    {

    }

    private void ComponentInit()
    {
        BCIC = GetComponent<BroadCasterInfoContainer>();
    }


    private void OnOtherReceiverChanged(List<GameObject> newList)
    {
        OtherReceiver = newList;
    }

    // 订阅事件
    private void EventSubscribe()
    {
        BCIC.OtherReceiverChanged += OnOtherReceiverChanged;
    }

    private void OnDestroy()
    {
        if (BCIC != null)
        {
            BCIC.OtherReceiverChanged -= OnOtherReceiverChanged;
        }
    }
}

