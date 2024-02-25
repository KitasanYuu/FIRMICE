using CustomInspector;
using UnityEngine;

namespace TestField
{
    public class Identity : MonoBehaviour
    {
        [SerializeField, FixedValues("Player", "Partner", "Ally","Enemy","Neutral", "TrainingTarget","TestPrototype","Cover","Custom")]
        private string MasterIdentity;
        [SerializeField,ShowIf(nameof(ShowCustomOps))]
        private string CustomMasterIdentity;
        [ShowIf(nameof(CidShow))]
        public string Cid;
        [SerializeField, FixedValues("null","FullCover", "HalfCover"),ShowIf(nameof(CoverSerizes))]
        private string CoverType;
        [ReadOnly,ShowIf(nameof(CoverSerizes))]
        public bool CoverOccupied;
        [ReadOnly,ShowIf(nameof(CoverSerizes))]
        public GameObject Occupier;

        public bool ShowCustomOps() => MasterIdentity == "Custom";
        public bool CidShow() => MasterIdentity != "Cover";
        public bool CoverSerizes() => MasterIdentity == "Cover";

        [HideInInspector]
        public string MasterID;
        [HideInInspector]
        public string Covertype;
        private bool OccupiedInit;

        // Start is called before the first frame update
        void Start()
        {
            if(ShowCustomOps())
            {
                MasterID = CustomMasterIdentity;
            }
            else
            {
                MasterID = MasterIdentity;
            }

            Covertype = CoverType;
        }

        // Update is called once per frame
        void Update()
        {
            if (ShowCustomOps())
            {
                MasterID = CustomMasterIdentity;
            }
            else
            {
                MasterID = MasterIdentity;
            }

            Covertype = CoverType;
            CoverTypeUpdate();
            if (OccupiedInit)
            {
                CoverOccupied = false;
            }
        }

        public void SetOccupiedUseage(bool isOccupied=false,GameObject COccupier = null)
        {
            if (isOccupied)
                OccupiedInit = false;
            else
                OccupiedInit = true;
            CoverOccupied = isOccupied;
            Occupier = COccupier;
        }

        private void CoverTypeUpdate()
        {
            if (MasterIdentity != "Cover")
            {
                CoverType = null;
            }
        }

        //提供方法从外部修改MasterID
        public void SetMasterID(string FixedMasterID)
        {
            MasterIdentity = "Custom";
            CustomMasterIdentity = FixedMasterID;
        }
    }
}
