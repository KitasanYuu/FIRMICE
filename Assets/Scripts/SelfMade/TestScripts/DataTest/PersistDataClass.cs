using UnityEngine;

namespace YDataPersistence
{
    public class PersistDataClass
    {
        public string cryptographicCheckCode;
        public TestDataClass testDataClass = new TestDataClass();
        public PlayerLocationData playerLocationData = new PlayerLocationData();
    }

    [System.Serializable]
    public class TestDataClass
    {
        public int currentNumber;
        public int Number;
    }

    [System.Serializable]
    public class PlayerLocationData
    {
        public string sceneName;
        public Vector3 position;
    }
}