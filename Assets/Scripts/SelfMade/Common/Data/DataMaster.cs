using System.Collections;
using System.Collections.Generic;
using TestField;
using UnityEngine;

namespace DataManager
{
    public class DataMaster
    {
        private CSVReader csvreader = new CSVReader();

        //获取Actor所属阵营
        public int GetActorCamp(GameObject gameObject)
        {
            Identity ID = gameObject.GetComponent<Identity>();
            if (ID != null)
            {
                string CampName = ID.MasterID;
                if (CampName != null)
                {
                    var CampInfo = csvreader.GetDataByID("Identity", CampName);
                    if (CampInfo != null)
                    {
                        int camp = (int)CampInfo["Camp"];
                        return camp;
                    }
                    else
                    {
                        Debug.LogError("DataMaster:GetActorCamp" + gameObject.name + "Cannot Find Camp:" + CampName + "in Identity!");
                        return 0;
                    }
                }
                else
                {
                    Debug.LogError("DataMaster:GetActorCamp" + gameObject.name + "Master ID is Null!");
                    return 0;
                }
            }
            else
            {
                Debug.LogError("DataMaster:GetActorCamp" + gameObject.name + "has no Identity Component Found!");
                return 0;
            }
        }

        public float GetCharaterPriority(GameObject Actor)
        {
            Identity ID = Actor.GetComponent<Identity>();
            string MasterID = ID.MasterID;
            var IDData = csvreader.GetDataByID("Identity", MasterID);
            float CharaterPriority = (float)IDData["CharacterPriority"];
            return CharaterPriority;
        }

        //用于获取Actor是否为特殊属性，0为普通，1为Boss
        public int GetActorSP(GameObject Actor)
        {
            bool isBoss = false;
            Identity ID = Actor.GetComponent<Identity>();
            string ActorCID = ID?.Cid;
            if (ActorCID.EndsWith("_E"))
            {
                ActorCID = ActorCID.Substring(0, ActorCID.Length - 2);
                isBoss = true;
            }

            var ActorInfo = csvreader.GetDataByID("role", ActorCID);
            if (ActorInfo != null)
            {
                if(!isBoss)
                    return 0;
                else
                    return 1;
            }
            else
            {
                Debug.LogError("DataMaster:GetActorName" + Actor.name + "Has no CID Found:" + ActorCID); ;
                return -1;
            }
        }

        public string GetActorSPTitle(GameObject Actor)
        {
            string Title;
            Identity ID = Actor.GetComponent<Identity>();
            string ActorCID = ID?.Cid;
            if (ActorCID.EndsWith("_E"))
            {
                ActorCID = ActorCID.Substring(0, ActorCID.Length - 2);

                var ActorInfo = csvreader.GetDataByID("role", ActorCID);
                if (ActorInfo != null)
                {
                    Title = (string)ActorInfo["EliteTitle"];
                    return Title;
                }
                else
                {
                    Debug.LogError("DataMaster:GetActorName" + Actor.name + "Has no CID Found");
                    var EActorInfo = csvreader.GetDataByID("role", "Err_NoCID");
                    Title = (string)EActorInfo["EliteTitle"];
                    return Title;
                }
            }
            else
            {
                Debug.LogWarning(Actor.name + "IS Not Elite!! CID:" + ActorCID);
                var EActorInfo = csvreader.GetDataByID("role", "Err_NotElite");
                Title = (string)EActorInfo["EliteTitle"];
                return Title;
            }
        }

        public string GetActorName(GameObject Actor)
        {
            Identity ID = Actor.GetComponent<Identity>();
            string ActorCID = ID?.Cid;
            var ActorInfo = csvreader.GetDataByID("role", ActorCID);
            if (ActorInfo != null)
            {
                string ActorName = (string)ActorInfo["Name"];
                return ActorName;
            }
            else
            {
                Debug.LogError("DataMaster:GetActorName" + Actor.name + "Has no CID Found");
                string placeHoder = "PlaceHolder";
                return placeHoder;
            }
        }

        public string GetActorID(GameObject Actor)
        {
            Identity ID = Actor.GetComponent<Identity>();
            string ActorCID = ID?.Cid;
            var ActorInfo = csvreader.GetDataByID("role", ActorCID);
            if (ActorInfo != null)
            {
                string ActorName = (string)ActorInfo["ID"];
                return ActorName;
            }
            else
            {
                Debug.LogError("DataMaster:GetActorName" + Actor.name + "Has no CID Found");
                string placeHoder = null;
                return placeHoder;
            }
        }

        public Dictionary<string, object> GetWeaponVar(string WeaponID)
        {
            var weapon = csvreader.GetDataByID("weapons", WeaponID);
            if(weapon != null)
            {
                return weapon;
            }
            else
            {
                Debug.LogError("DataMaster:GetWeaponVar" + WeaponID + "Not Found!");
                return null;
            }
        }
    }

    public class LocalDataSaver
    {
        private DataMaster DM = new DataMaster();
        private Dictionary<GameObject,string>actorID = new Dictionary<GameObject,string>();
        private Dictionary<GameObject, int> actorCamps = new Dictionary<GameObject, int>();
        private Dictionary<GameObject,int> actorSP = new Dictionary<GameObject,int>();
        private Dictionary<GameObject,string> actorSPTitle = new Dictionary<GameObject,string>();
        private Dictionary<GameObject,string> actorName = new Dictionary<GameObject,string>();
        private Dictionary<GameObject,float>actorPriority = new Dictionary<GameObject,float>();
        private Dictionary<string, Dictionary<string, object>> weaponInfo = new Dictionary<string, Dictionary<string, object>>();
        private Dictionary<string,string> weaponPrefabInfo = new Dictionary<string, string>();


        public string GetActorID(GameObject actor)
        {
            if (actorID.TryGetValue(actor, out string ID))
            {
                return ID;
            }
            else
            {
                // 假设DM.GetActorCamp是你获取阵营信息的方法
                string newID = DM.GetActorID(actor);
                actorID[actor] = newID;
                return newID;
            }
        }

        public bool IsPlayer(GameObject Actor)
        {
            string ActorID = GetActorID(Actor);
            if(ActorID == "C1001")
                return true;
            else
                return false;
        }

        public int GetActorSP(GameObject actor)
        {
            if (actorSP.TryGetValue(actor, out int SP))
            {
                return SP;
            }
            else
            {
                int newSP = DM.GetActorSP(actor);
                actorSP[actor] = newSP;
                return newSP;
            }
        }

        public float GetActorPriority(GameObject actor)
        {
            if(actorPriority.TryGetValue(actor,out float Priority))
            {
                return Priority;
            }
            else
            {
                float newPriority = DM.GetCharaterPriority(actor);
                actorPriority[actor] = newPriority;
                return newPriority;
            }
        }

        public string GetActorSPTitle(GameObject actor)
        {
            if (actorSPTitle.TryGetValue(actor, out string SPTitle))
            {
                return SPTitle;
            }
            else
            {
                string newSPTitle = DM.GetActorSPTitle(actor);
                actorSPTitle[actor] = newSPTitle;
                return newSPTitle;
            }
        }

        public int GetActorCamp(GameObject actor)
        {
            if (actorCamps.TryGetValue(actor, out int camp))
            {
                return camp;
            }
            else
            {
                // 假设DM.GetActorCamp是你获取阵营信息的方法
                int newCamp = DM.GetActorCamp(actor);
                actorCamps[actor] = newCamp;
                return newCamp;
            }
        }

        public string GetActorName(GameObject actor)
        {
            if(actorName.TryGetValue(actor,out string actorname))
            {
                return actorname;
            }
            else
            {
                string newActorName = DM.GetActorName(actor);
                actorName[actor] = newActorName;
                return newActorName;
            }
        }

        public Dictionary<string, object> GetWeapon(string WeaponID)
        {
            if(weaponInfo.TryGetValue(WeaponID, out var weaponinfo))
            {
                return weaponinfo;
            }
            else
            {
                Dictionary<string, object> newWeaponInfo = DM.GetWeaponVar(WeaponID);
                weaponInfo[WeaponID] = newWeaponInfo;
                return newWeaponInfo;
            }
        }

        public string GetWeaponPrefab(string WeaponID)
        {
            if(weaponPrefabInfo.TryGetValue(WeaponID ,out string weaponPrefab))
            {
                if (weaponPrefab != "null")
                    return weaponPrefab;
                else
                    return null;
            }
            else
            {
                var weapon = GetWeapon(WeaponID);
                string newweaponPrefab = (string)weapon["WeaponPrefab"];
                weaponPrefabInfo[WeaponID]= newweaponPrefab;
                if (newweaponPrefab != "null")
                    return newweaponPrefab;
                else
                    return null;
            }
        }

        public string GetWeaponType(string WeaponID)
        {
            var weapon = GetWeapon(WeaponID);
            int weaponType = (int)weapon["WeaponType"];
            switch (weaponType)
            {
                case 0:
                    return "无武器";
                case 1:
                    return "手枪";
                case 2:
                    return "冲锋枪";
                case 3:
                    return "突击步枪";
                case 4:
                    return "轻机枪";
                case 5:
                    return "射手步枪";
                case 6:
                    return "狙击步枪";
                case 7:
                    return "特种武器";
                default:
                    return "404";
            }

        }
    }
}

