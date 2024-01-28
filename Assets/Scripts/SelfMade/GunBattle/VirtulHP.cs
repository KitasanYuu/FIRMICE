using CustomInspector;
using System.Collections;
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
        [SerializeField,Tooltip("自身的减伤倍率，值越高伤害减免越高，为1时完全无敌"), Range(0, 1)]
        private float DamageReduceRate;


        public float Armor;
        [Tooltip("护盾带来的减伤效率，值越低护盾减免越高，为0时完全无敌，为1时全伤害生效"),Range(1,0)]public float ArmorRate = 1;

        public List<float> damageList = new List<float>(); // 使用List来存储伤害值

        //public float Damage;

        // Start is called before the first frame update
        void Start()
        {
            CurrentHP = TotalHP;
            CurrentArmor = Armor;
        }

        // Update is called once per frame
        void Update()
        {
            DamageCalculating();

            //Debug.LogWarning(CurrentHP);

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

            if(totalDamage != 0)
            {
                //Debug.Log(totalDamage);
                if (CurrentArmor > 1)
                {
                    CurrentHP = CurrentHP - (totalDamage * ArmorRate) * (1 - DamageReduceRate);
                    CurrentArmor -= 0.1f * CurrentArmor;
                }
                else if (CurrentArmor <= 1)
                {
                    CurrentArmor = 0;
                    CurrentHP = CurrentHP - totalDamage * (1 - DamageReduceRate);
                }
            }

            //totalDamage = 0;
        }

        // 方法用于接收伤害值并将其添加到数组中
        public void AddDamage(float damage)
        {
            // 将伤害值添加到List末尾
            damageList.Add(damage);
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
}