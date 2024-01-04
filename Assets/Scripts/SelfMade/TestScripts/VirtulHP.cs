using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleHealth
{

    public class VirtualHP : MonoBehaviour
    {
        public float TotalHP;
        //public float Damage;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            Debug.LogWarning(TotalHP);

            if(TotalHP == 0)
            {
                //this.gameObject.SetActive(false);
            }
        }
    }
}