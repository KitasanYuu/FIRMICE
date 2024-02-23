using CustomInspector;
using UnityEngine;

namespace TestField
{
    public class Identity : MonoBehaviour
    {
        [SerializeField, FixedValues("Player", "Partner", "Ally","Enemy","Neutral", "TrainingTarget","TestPrototype","Cover")]
        private string MasterIdentity;
        [ShowIf(nameof(CidShow))]
        public string Cid;
        [SerializeField, FixedValues("null","FullCover", "HalfCover"),ShowIf(nameof(CoverSerizes))]
        private string CoverType;
        [ReadOnly,ShowIf(nameof(CoverSerizes))]
        public bool CoverOccupied;
        [ReadOnly,ShowIf(nameof(CoverSerizes))]
        public GameObject Occupier;

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
            MasterID = MasterIdentity;
            Covertype = CoverType;
        }

        // Update is called once per frame
        void Update()
        {
            MasterID = MasterIdentity;
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
    }
}
