using CustomInspector;
using UnityEngine;
using YuuTool;

public class DataPersistenceManager : MonoBehaviour
{
    [ReadOnly] public string cryptographicCheckCode;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            cryptographicCheckCode = YTool.GenerateRandomString(16);
            Debug.Log(cryptographicCheckCode);
        }
    }


}
