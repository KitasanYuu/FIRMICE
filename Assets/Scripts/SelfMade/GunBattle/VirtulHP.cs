using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleHealth
{

    public class VirtualHP : MonoBehaviour
    {
        [SerializeField]
        private float TotalHP;
        [SerializeField]
        private float DamageReduceRate;

        private float CurrentHP;
        public float Armor;
        public float ArmorRate;
        private float OriginArmor;
        public List<float> damageList = new List<float>(); // 使用List来存储伤害值

        //public float Damage;

        // Start is called before the first frame update
        void Start()
        {
            CurrentHP = TotalHP;
            ArmorRate = 1 / Armor;
            OriginArmor = Armor;
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

            if (Armor > 0)
            {
                CurrentHP = CurrentHP - (totalDamage * ArmorRate)* (1-DamageReduceRate);
                Armor -= 0.1f*OriginArmor;
            }
            else if(Armor <= 0)
            {
                Armor = 0;
                CurrentHP = CurrentHP - totalDamage * (1-DamageReduceRate);
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