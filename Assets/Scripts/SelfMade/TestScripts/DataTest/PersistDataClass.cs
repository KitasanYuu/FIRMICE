using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YDataPersistence
{
    public class PersistDataClass
    {
        public string cryptographicCheckCode;
        public TestDataClass testDataClass = new TestDataClass();
    }

    [System.Serializable]
    public class TestDataClass
    {
        public int currentNumber;
        public int Number;
        // 其他属性...
    }
}