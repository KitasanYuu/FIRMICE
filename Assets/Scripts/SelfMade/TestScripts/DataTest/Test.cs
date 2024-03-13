using UnityEngine;
using DataManager;
using System.Collections.Generic;
using UnityEngine.UIElements;

public class Example : MonoBehaviour
{
    void Start()
    {
        // 创建一个 CSVReader 对象
        CSVReader csvReader = new CSVReader();

        csvReader.GetDataByID("weapons", "Weapon_Rifle");

        string idToSearch = "Weapon_Rifle";
        var weaponData = csvReader.GetDataByID("weapons", idToSearch);
        if (weaponData != null)
        {
            // 使用获取的数据
            string Gid = (string)weaponData["GID"];
            string id = (string)weaponData["ID"];
            string weaponName = (string)weaponData["WeaponName"];
            float damage = (float)weaponData["Damage"];
            float VaildShootRange = (float)weaponData["VaildShootRange"];
            string HitParticle = (string)weaponData["HitParticle"];

            // 输出获取的数据
            Debug.Log("Weapon found with ID " + Gid + ":");
            Debug.Log("Weapon ID " + id);
            Debug.Log("WeaponName: " + weaponName);
            Debug.Log("Damage: " + damage);
            Debug.Log("VaildShootRange: " + VaildShootRange);
            Debug.Log("HitParticle:" + HitParticle);
            //Debug.Log("Test: " + test);
            //Debug.Log("Position" + Position);
            //Debug.Log("Whrer" + whrer);
        }
        else
        {
            Debug.Log("Weapon with ID " + idToSearch + " not found in weapons.csv.");
        }
    }
}
