using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomInspector.Documentation
{
    [System.Serializable]
    public class TestClass
    {
        [ReadOnly]
        public int id = 432564;

        [Range(0, 1)]
        public float value;
    }
}
