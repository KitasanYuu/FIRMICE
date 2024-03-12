using UnityEngine;

public class UICameraInit : MonoBehaviour
{
    public GameObject UICamera;

    private void Awake()
    {
        if(UICamera!= null)
            UICamera.SetActive(false);
    }

    private void Start()
    {
        if(UICamera != null && !UICamera.activeSelf)
        UICamera.SetActive(true);
    }
}
