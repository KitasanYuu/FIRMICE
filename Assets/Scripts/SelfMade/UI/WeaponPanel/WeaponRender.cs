using DataManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponRender : MonoBehaviour
{
    private ResourceReader RR = new ResourceReader();
    private LocalDataSaver LDS = new LocalDataSaver();

    public bool WeaponPrefabComfirm(string WeaponID)
    {
        string weaponPrefabname = LDS.GetWeaponPrefab(WeaponID);
        if (weaponPrefabname != null)
        {
            GameObject weaponPrefab = RR.GetGameObject("WeaponSnapShot", weaponPrefabname);
            if (weaponPrefab != null)
                return true;
            else
                return false;
        }
        else
            return false;
    }


    public void SnapShotRequest(string WeaponID)
    {
        string weaponPrefabname = LDS.GetWeaponPrefab(WeaponID);
        GameObject weaponPrefab = RR.GetGameObject("WeaponSnapShot", weaponPrefabname);
        if(weaponPrefab != null)
        {

        }
        else
        {

        }
    }
}
