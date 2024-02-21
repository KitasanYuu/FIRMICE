using System.Collections.Generic;
using UnityEngine;

namespace TestField
{
    public class AIFunction
    {
        //用于判断目标是否在自身有效射程内，传入目标GameObject
        public bool ValidShootPosition(GameObject SelfObject, GameObject Target, float ValidShootRange)
        {
            bool ValidPosition = false;
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
                Debug.Log("射线碰到了：" + hitInfo.collider.gameObject.name);
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

    }

}