using CustomInspector;
using DataManager;
using System.Collections.Generic;
using TestField;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace BattleHealth
{
    public class VirtualHP : MonoBehaviour
    {
        [ReadOnly] public GameObject Object;
        [ReadOnly] public GameObject HPAnchor;
        [ReadOnly] public float CurrentHP;
        [ReadOnly] public float CurrentArmor;
        [ReadOnly] public HealthBar healthBar;
        [ReadOnly] public EliteHealthBar eHealthBar;
        [Space2(20)]
        public bool TRevive;
        [SerializeField]
        private bool DestoryAfterDead;
        [SerializeField]
        private bool HideAfterDead;
        public float TotalHP;
        [SerializeField, Tooltip("自身的减伤倍率，值越高伤害减免越高，为1时完全无敌"), Range(0, 1)]
        private float DamageReduceRate;

        public float Armor;
        [Tooltip("护盾带来的减伤效率，值越低护盾减免越高，为0时完全无敌，为1时全伤害生效"), Range(1, 0)]
        public float ArmorRate = 1;

        private bool NeedRegistHP;

        private int mycamp= -999;

        private HPVisionManager HVM;
        private Identity id;
        private DataMaster DM = new DataMaster();

        public List<float> damageList = new List<float>(); // 使用List来存储伤害值

        // 用于存储每个角色造成的伤害
        private Dictionary<GameObject, float> characterDamageMap = new Dictionary<GameObject, float>();


        // 从外部调用的方法，输出列表中的信息
        public void PrintDamageInfo()
        {
            foreach (var kvp in characterDamageMap)
            {
                Debug.Log($"{kvp.Key.name} dealt {kvp.Value} damage.");
            }
        }

        void Start()
        {
            ComponentInit();
            InfoInit();
            ParameterInit();
            RegeisterHP(NeedRegistHP);
        }

        void Update()
        {
            RegeisterHP(NeedRegistHP);
            Revive(TRevive);
            if (Input.GetKeyDown(KeyCode.R))
            {
                PrintDamageInfo();
            }

            DamageCalculating();
        }


        #region 伤害计算相关
        // 从外部调用的方法，直接添加伤害并指定角色标识
        public void AddDamage(float damage, float ArmorBrake, GameObject character)
        {
            if(mycamp == -999)
            {
                mycamp = DM.GetActorCamp(gameObject);
            }

            int attackercamp = DM.GetActorCamp(character);
            
            if(attackercamp != mycamp)
            {
                // 计算减伤后的实际伤害值
                float actualDamage = damage;
                damageList.Add(actualDamage);
                if (CurrentArmor > 1)
                {
                    actualDamage = (damage * ArmorRate) * (1 - DamageReduceRate);
                    CurrentArmor -= ArmorBrake; // 根据实际情况调整护甲值的减少
                }
                else
                {
                    actualDamage = damage * (1 - DamageReduceRate);
                }

                // 确保伤害值不为负数
                actualDamage = Mathf.Max(actualDamage, 0);

                // 将处理过的伤害值关联到特定角色
                if (characterDamageMap.ContainsKey(character))
                {
                    characterDamageMap[character] += actualDamage;
                }
                else
                {
                    characterDamageMap.Add(character, actualDamage);
                }
            }

        }

        private void DamageCalculating()
        {
            float totalDamage = CalculateTotalDamage();

            ClearDamageArray();

            if (totalDamage != 0)
            {
                if (CurrentArmor > 1)
                {
                    CurrentHP = CurrentHP - (totalDamage * ArmorRate) * (1 - DamageReduceRate);
                    //CurrentArmor -= 0.1f * CurrentArmor;
                }
                else if (CurrentArmor <= 1)
                {
                    CurrentArmor = 0;
                    CurrentHP = CurrentHP - totalDamage * (1 - DamageReduceRate);
                }
            }

            if (CurrentHP <= 0)
            {
                CurrentHP = 0;
                if (HideAfterDead)
                    gameObject.SetActive(false);
                else if (DestoryAfterDead)
                    DestoryProgress();
            }
        }

        private float CalculateTotalDamage()
        {
            // 计算伤害数组的和
            float totalDamage = 0f;
            foreach (float damage in damageList)
            {
                totalDamage += damage;
            }
            return totalDamage;
        }

        private void ClearDamageArray()
        {
            // 清空伤害数组
            damageList.Clear();
        }
        #endregion

        #region 参数组件初始化
        private void ComponentInit()
        {
            id = GetComponent<Identity>();
        }

        private void InfoInit()
        {
            Object = gameObject;
            Transform hpAnchor = gameObject.transform.Find("HPAnchor");
            if (hpAnchor != null)
            {
                HPAnchor = hpAnchor.gameObject;
            }
        }

        private void ParameterInit()
        {
            NeedRegistHP = true;
            CurrentHP = TotalHP;
            CurrentArmor = Armor;
        }
        #endregion

        #region 对血条UI的注册流程&c参数接收
        private void RegeisterHP(bool NeedToRegist)
        {
            if (NeedToRegist)
            {
                bool isSpecial = false;
                DataMaster DM = new DataMaster();
                int SP = DM.GetActorSP(gameObject);
                if (SP != 0)
                    isSpecial = true;
                if (id.canUse)
                {

                    if ((Object != null && HPAnchor != null) || isSpecial)
                    {
                        HVM = FindAnyObjectByType<HPVisionManager>();

                        if (HVM != null)
                        {
                            HVM.ObjectHPRegister(gameObject);
                        }
                    }
                    else
                    {
                        Debug.LogError(gameObject.name + "Regist HP Failure");
                    }
                }
            }
        }

        public void SetRegistResult(bool Result)
        {
            NeedRegistHP = !Result;
        }

        public void SetHealthBar(HealthBar HPB)
        {
            healthBar = HPB;
        }

        public void SetEHealthBar(EliteHealthBar EHPB)
        {
            eHealthBar = EHPB;
        }
        #endregion



        public void Revive(bool revive)
        {
            if (revive)
            {
                characterDamageMap.Clear();
                CurrentArmor = Armor;
                CurrentHP = TotalHP;
                TRevive = false;
                gameObject.SetActive(true);
            }
        }



        private void DestoryProgress()
        {
            if(healthBar != null)
                Destroy(healthBar.gameObject);
            else if(eHealthBar != null)
                Destroy(eHealthBar.gameObject);
            Destroy(gameObject);
        }
    }

}
