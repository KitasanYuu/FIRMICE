using UnityEngine;
using YuuTool;
using CustomInspector;

public class WeaponPanel : MonoBehaviour
{
    [ReadOnly] public GameObject _weaponEquipPanel;
    [ReadOnly] public GameObject _weaponSelectPanel;
    private CanvasGroup _weaponSelectPanelcanvasGroup;
    // Start is called before the first frame update


    private void Start()
    {
        ComponentInit();
    }

    private void ComponentInit()
    {
        _weaponEquipPanel = transform.FindDeepChild("WeaponEquipPanel")?.gameObject;
        _weaponSelectPanel = transform.FindDeepChild("WeaponSelectPanel")?.gameObject;

        _weaponSelectPanelcanvasGroup = _weaponSelectPanel?.GetComponent<CanvasGroup>();
    }
}
