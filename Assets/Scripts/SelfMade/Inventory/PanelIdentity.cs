using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelIdentity : MonoBehaviour
{
    public string PID;
    public int PageNum;

    private InventoryPanelManager IPM;

    private void Start()
    {
        IPM = GetComponentInParent<InventoryPanelManager>();
        if ( IPM != null )
        {
            CanvasGroup cg = gameObject.GetComponent<CanvasGroup>();
            IPM.PanelInit(gameObject, cg);
        }
    }
}
    