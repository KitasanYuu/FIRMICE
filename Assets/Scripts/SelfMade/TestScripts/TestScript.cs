using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace TestField
{
    public class TestScript : MonoBehaviour
    {
        private BroadCasterInfoContainer BCIC;
        private CoverUtility coverUtility = new CoverUtility();
        public GameObject Target;
        public List<GameObject> CoverList = new List<GameObject>();
        private Vector3 TargetCover;

        // Start is called before the first frame update
        void Start()
        {
            BCIC = GetComponent<BroadCasterInfoContainer>();
        }

        // Update is called once per frame
        void Update()
        {
            CoverList = BCIC.CoverList;
            if (Input.GetKeyUp(KeyCode.R))
            {
                TargetCover = coverUtility.FindSafePointOnNextCover(gameObject, Target, CoverList,true);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            if (TargetCover != null)
            {

                Vector3 CoverPosition = TargetCover;
                Gizmos.DrawSphere(CoverPosition, 0.5f);
            }
        }
    }
}