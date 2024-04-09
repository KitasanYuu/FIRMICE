using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UIPanelID", menuName = "Global/UIPanelID", order = 1)]
public class UIPanelID : ScriptableObject
{
    public List<UIIdentity> UIID = new List<UIIdentity>();


}

[System.Serializable]
public class UIIdentity
{
    public string PanelID;
    public List<SubSelectIdentity> SubIdentity = new List<SubSelectIdentity>();
}

[System.Serializable]
public class SubSelectIdentity
{
    public string PanelSubTitle;
    public  string PanelTitle;
    public int Page;
}