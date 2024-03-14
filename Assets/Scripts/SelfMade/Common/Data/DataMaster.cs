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

