using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomInspector;

[CreateAssetMenu(fileName = "UIPanelID", menuName = "Global/UIPanelID", order = 1)]
public class UIPanelID : ScriptableObject
{
    public List<UIIdentity> UIID = new List<UIIdentity>();


}

[System.Serializable]
public class UIIdentity : IEnumerable<SubSelectIdentity>
{
    public string PanelID;
    public List<SubSelectIdentity> SubIdentity = new List<SubSelectIdentity>();

    // 实现 IEnumerable 接口的 GetEnumerator 方法
    public IEnumerator<SubSelectIdentity> GetEnumerator()
    {
        return SubIdentity.GetEnumerator();
    }

    // 实现非泛型版本的 IEnumerable 接口的 GetEnumerator 方法
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

[System.Serializable]
public class SubSelectIdentity
{
    public string PanelTitle;
    public bool UsingSpecificSub = false;
    [ShowIf(nameof(_bool))]
    public string PanelSubTitle;
    public int Page;

    public bool _bool()
=> UsingSpecificSub == false;
}