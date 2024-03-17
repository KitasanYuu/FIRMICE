using System.Collections;
using System.Collections.Generic;
using TestField;
using UnityEngine;

namespace DataManager
{
    public class DataMaster
    {
        //获取Actor所属阵营
        public int GetActorCamp(GameObject gameObject)
        {
            CSVReader csvreader = new CSVReader();
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

        //用于获取Actor是否为特殊属性，0为普通，1为Boss
        public int GetActorSP(GameObject Actor)
        {
            bool isBoss = false;
            CSVReader csvreader = new CSVReader();
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
            CSVReader csvreader = new CSVReader();
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
                    Title = "外域来客";
                    return Title;
                }
            }
            else
            {
                Debug.LogWarning(Actor.name + "IS Not Elite!! CID:" + ActorCID);
                Title = "错误适格者";
                return Title;
            }
        }

        public string GetActorName(GameObject Actor)
        {
            CSVReader csvreader = new CSVReader();
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

    }
}

