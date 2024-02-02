using BattleHealth;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempReviveTrigger : MonoBehaviour
{
    public GameObject ReviveTarget;
    public bool Revive;
    private VirtualHP virtualHP;
    // Start is called before the first frame update
    void Start()
    {
        virtualHP= ReviveTarget.GetComponent<VirtualHP>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Revive)
        {
            virtualHP.Revive(Revive);
            if(ReviveTarget.activeSelf)
            {
                Revive = false;
            }
        }
    }
}
