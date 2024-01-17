using CustomInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TestField
{
    public class Identity : MonoBehaviour
    {
        [SerializeField]
        [FixedValues("Player", "Partner", "Enemy", "Neutral", "TrainingTarget")]
        private string MasterIdentity;

        [HideInInspector]
        public string MasterID;
        // Start is called before the first frame update
        void Start()
        {
            MasterID = MasterIdentity;
        }

        // Update is called once per frame
        void Update()
        {
            MasterID = MasterIdentity;
        }
    }
}
