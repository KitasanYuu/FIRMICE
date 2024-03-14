using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using DataManager; 

namespace TestField
{
    public class TestScript : MonoBehaviour
    {
        private BroadCasterInfoContainer BCIC;
        private CoverUtility coverUtility = new CoverUtility();
        public GameObject Target;
        public List<GameObject> CoverList = new List<GameObject>();
        private Vector3 TargetCover;
        private ResourceReader RR = new ResourceReader();

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyUp(KeyCode.G))
            {
                Color color = RR.GetColor("NeutralColor");
            }
        }

    }
}