using System.Collections.Generic;
using UnityEngine;
using DataManager;

namespace TestField
{
    public class AIFunction
    {
        //用于判断目标是否在自身有效射程内，传入目标GameObject
        public bool ValidShootPosition(GameObject SelfObject, GameObject Target, float ValidShootRange)
        {
            bool ValidPosition;
            if (Target != null)
            {
                float ToTargetDistance;
                ToTargetDistance = Vector3.Distance(Target.transform.position, SelfObject.transform.position);
                //判断目标是否在有效射程内
                if (ToTargetDistance > ValidShootRange)
                {
                    ValidPosition = false;
                }
                else
                {
                    ValidPosition = true;
                }
            }
            else
            {
                ValidPosition = false;
            }
            return ValidPosition;
        }

        //用于判定是否直接面对Target
        public bool IsDirectToTarget(GameObject selfObject, GameObject Target, LayerMask ignoreLayer)
        {
            BroadCasterInfoContainer BCIC = selfObject.GetComponent<BroadCasterInfoContainer>();
            if (BCIC == null || Target == null)
            {
                // 处理 BCIC 或 Target 为 null 的情况
                return false;
            }

            // 获取自身位置
            Vector3 selfPosition = selfObject.transform.position;

            // 获取目标位置
            Vector3 targetPosition = Target.transform.position;

            // 使用 Physics.Linecast 检测自身到目标之间的连线上是否有其他Collider，同时忽略指定层级
            RaycastHit hitInfo;
            bool hit = Physics.Linecast(selfPosition, targetPosition, out hitInfo, ~ignoreLayer);

            // 使用 Debug.DrawLine 可视化射线，cyan 色表示碰到物体
            Color lineColor = hit ? Color.cyan : Color.green;
            Debug.DrawLine(selfPosition, targetPosition, lineColor);

            // 如果有碰撞，打印碰到的物体信息
            if (hit)
            {
                //Debug.Log("射线碰到了：" + hitInfo.collider.gameObject.name);
            }

            // 如果有碰撞，则返回 false，表示不是直线到目标
            return !hit;
        }

        public GameObject CurrentSelectedAttackTarget(GameObject SelfObject, List<GameObject> AttackTargetList)
        {
            if (AttackTargetList.Count > 0)
            {
                GameObject SelectTarget = null;
                float maxDependValue = float.MaxValue; // 初始化最大 DependValue 值为最小可能值
                CSVReader csvreader = new CSVReader();
                csvreader.LoadCSV("Identity.csv");

                foreach (GameObject Target in AttackTargetList)
                {
                    Identity ID = Target.GetComponent<Identity>();
                    if (ID != null)
                    {
                        string MasterID = ID.MasterID;
                        var IDData = csvreader.GetDataByID("Identity", MasterID);
                        if (IDData != null)
                        {
                            float CharaterPriority = (float)IDData["CharacterPriority"];
                            float DistancetoSelf = Vector3.Distance(SelfObject.transform.position, Target.transform.position);
                            float DependValue = DistancetoSelf / CharaterPriority;
                            // 如果当前 DependValue 大于最大值，则更新最大值和选择目标
                            if (DependValue < maxDependValue)
                            {
                                maxDependValue = DependValue;
                                SelectTarget = Target;
                                //Debug.LogWarning("Current Target is " + Target.name + "Distance is"+DistancetoSelf+"CharacterPriority"+CharaterPriority+ "DependValue is" + DependValue);
                            }
                        }
                    }
                }
                return SelectTarget;
            }
            else
            {
                Debug.LogWarning("AIF:CurrentSelectedAttackTarget Found AttackTargetList Number:" + AttackTargetList.Count);
                return null;
            }
        }

        public MonoBehaviour GainSelfMoveScriptType(GameObject SelfObject)
        {
            CSVReader csvreader = new CSVReader();
            Identity ID = SelfObject.GetComponent<Identity>();
            if (ID != null)
            {
                string cid = ID.Cid;
                if (cid != null)
                {
                    var CharacterInfo = csvreader.GetDataByID("role", cid);
                     if (CharacterInfo != null)
                    {
                        int CharacterType = (int)CharacterInfo["CharacterType"];
                        switch (CharacterType)
                        {
                            case 1:
                                return SelfObject.GetComponent<HumanoidMoveLogic>();
                            case 2:
                                return SelfObject.GetComponent<NonHumanoidMoveLogic>();
                            default:
                                Debug.LogError("AIFGainSelfMoveScriptType:"+SelfObject.name+"Invalid CharacterType");
                                return null;
                        }
                    }
                    else
                    {
                        Debug.LogError("AIFGainSelfMoveScriptType:"+ SelfObject.name +"Cid Not Found in Role!");
                        return null;
                    }
                }
                else
                {
                    Debug.LogError("AIFGainSelfMoveScriptType:" + SelfObject.name + "has no CID!");
                    return null;
                }
            }
            else
            {
                Debug.LogError("AIFGainSelfMoveScriptType:" + SelfObject.name + "has no Identity Component!");
                return null;
            }

        }

        //用于获取目标物体身上的点是否能够被射中
        public GameObject GetAvailableShootPoint(GameObject raycastOrigin, GameObject SelfObject, GameObject Target, LayerMask aimColliderLayerMask)
        {
            if (raycastOrigin != null)
            {

                TargetContainer TC = Target?.GetComponent<TargetContainer>();
                if (TC != null)
                {
                    foreach (GameObject go in TC.targets)
                    {
                        Ray ray = new Ray(raycastOrigin.transform.position, (go.transform.position - raycastOrigin.transform.position).normalized);
                        if (Physics.Raycast(ray, out RaycastHit hit, 999f, aimColliderLayerMask))
                        {
                            // 判断击中的目标是否就是预期的Target 或者其父物体
                            Transform currentTransform = hit.transform;
                            while (currentTransform != null)
                            {
                                if (currentTransform == Target.transform)
                                {
                                    return go;
                                }
                                currentTransform = currentTransform.parent;
                            }
                        }
                        else
                        {
                            Debug.LogError("AIF GetAvailableShootPoint:Ray Do Not Hit AnyThing");
                            return null;
                        }
                    }
                    Debug.LogError("AIF GetAvailableShootPoint:No Available ShootPoint In targetList");
                    return null;
                }
                else
                {
                    Debug.LogError("AIF GetAvailableShootPoint:Select Target Has No TargetContainer");
                    return null;
                }
            }
            else
            {
                Debug.LogError("AIF GetAvailableShootPoint:Raycast origin is not set. Please assign a GameObject to the 'raycastOrigin' field.");
                return null;
            }
        }

        //获取Actor所属阵营
        public int GetActorCamp(GameObject gameObject)
        {
            CSVReader csvreader = new CSVReader();
            Identity ID = gameObject.GetComponent<Identity>();
            if(ID != null)
            {
                string CampName = ID.MasterID;
                if(CampName != null)
                {
                    var CampInfo = csvreader.GetDataByID("Identity", CampName);
                    if(CampInfo != null)
                    {
                        int camp = (int)CampInfo["Camp"];
                        return camp;
                    }
                    else
                    {
                        Debug.LogError("AIF:GetActorCamp" + gameObject.name + "Cannot Find Camp:"+CampName+"in Identity!");
                        return 0;
                    }
                }
                else
                {
                    Debug.LogError("AIF:GetActorCamp" + gameObject.name + "Master ID is Null!");
                    return 0;
                }
            }
            else
            {
                Debug.LogError("AIF:GetActorCamp" + gameObject.name + "has no Identity Component Found!");
                return 0;
            }
        }

    }

}