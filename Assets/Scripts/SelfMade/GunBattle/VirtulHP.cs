using CustomInspector;
using System.Collections.Generic;
using UnityEngine;

namespace BattleHealth
{
    public class VirtualHP : MonoBehaviour
    {
        [SerializeField, ReadOnly] private float CurrentHP;
        [SerializeField, ReadOnly] private float CurrentArmor;
        [Space2(20)]
        [SerializeField]
        private float TotalHP;
        [SerializeField, Tooltip("自身的减伤倍率，值越高伤害减免越高，为1时完全无敌"), Range(0, 1)]
        private float DamageReduceRate;

        public float Armor;
        [Tooltip("护盾带来的减伤效率，值越低护盾减免越高，为0时完全无敌，为1时全伤害生效"), Range(1, 0)]
        public float ArmorRate = 1;

        public List<float> damageList = new List<float>(); // 使用List来存储伤害值

        // 用于存储每个角色造成的伤害
        private Dictionary<GameObject, float> characterDamageMap = new Dictionary<GameObject, float>();

        // 定义伤害事件
        public DamageEvent onDamageDealt = new DamageEvent();

        // 从外部调用的方法，输出列表中的信息
        public void PrintDamageInfo()
        {
            foreach (var kvp in characterDamageMap)
            {
                Debug.Log($"{kvp.Key.name} dealt {kvp.Value} damage.");
            }
        }

        // 从外部调用的方法，直接添加伤害并指定角色标识
        public void AddDamage(float damage, GameObject character)
        {
            // 将伤害值添加到List末尾
            damageList.Add(damage);

            // 将伤害值关联到特定角色
            if (characterDamageMap.ContainsKey(character))
            {
                characterDamageMap[character] += damage;
            }
            else
            {
                characterDamageMap.Add(character, damage);
            }
        }

        void Start()
        {
            CurrentHP = TotalHP;
            CurrentArmor = Armor;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                PrintDamageInfo();
            }
            
            DamageCalculating();

            if (CurrentHP <= 0)
            {
                CurrentHP = 0;
                //this.gameObject.SetActive(false);
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
                    // 触发伤害事件，通知其他系统
                    onDamageDealt.Invoke(totalDamage, gameObject);

                    CurrentHP = CurrentHP - (totalDamage * ArmorRate) * (1 - DamageReduceRate);
                    CurrentArmor -= 0.1f * CurrentArmor;
                }
                else if (CurrentArmor <= 1)
                {
                    // 触发伤害事件，通知其他系统
                    onDamageDealt.Invoke(totalDamage, gameObject);

                    CurrentArmor = 0;
                    CurrentHP = CurrentHP - totalDamage * (1 - DamageReduceRate);
                }
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
    }

    // 定义伤害事件
    [System.Serializable]
    public class DamageEvent : UnityEngine.Events.UnityEvent<float, GameObject> { }
}
