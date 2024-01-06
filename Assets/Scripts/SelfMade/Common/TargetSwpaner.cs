using UnityEngine;
using UnityEngine.SceneManagement;

public class TargetSwpaner : MonoBehaviour
{
    public GameObject prefabToGenerate; // Ԥ����
    public string respawnObjectName = "Reswpan"; // Reswpan���������


    private void Awake()
    {
        // ���ĳ�����������¼�
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        // ȡ�����ĳ�����������¼��Է�ֹ�ڴ�й©
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // ���³���������ɺ�����Ԥ����
        if (prefabToGenerate != null)
        {
            // ������Ϊ"Reswpan"������
            GameObject respawnObject = GameObject.Find(respawnObjectName);

            // ����ҵ���"Reswpan"���壬��ȡ��λ�ò�����Ԥ����
            if (respawnObject != null)
            {
                Vector3 spawnPosition = respawnObject.transform.position;
                Instantiate(prefabToGenerate, spawnPosition, Quaternion.identity);
            }
            else
            {
                Debug.LogWarning("Could not find object with the name " + respawnObjectName);
            }
        }
    }
}
