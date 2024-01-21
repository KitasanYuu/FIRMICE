using CustomInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TestField
{
    public class Identity : MonoBehaviour
    {
        [SerializeField, FixedValues("Player", "Partner", "Enemy", "Neutral", "TrainingTarget","TestPrototype","Cover")]
        private string MasterIdentity;
        [SerializeField, FixedValues("null","FullCover", "HalfCover"),ShowIf(nameof(CoverSerizes))]
        private string CoverType;
        public bool CoverSerizes() => MasterIdentity == "Cover";

        [HideInInspector]
        public string MasterID;
        [HideInInspector]
        public string Covertype;
        // Start is called before the first frame update
        void Start()
        {
            MasterID = MasterIdentity;
            Covertype = CoverType;
        }

        // Update is called once per frame
        void Update()
        {
            MasterID = MasterIdentity;
            Covertype = CoverType;
            CoverTypeUpdate();
        }

        private void CoverTypeUpdate()
        {
            if (MasterIdentity != "Cover")
            {
                CoverType = null;
            }
        }
    }
}
