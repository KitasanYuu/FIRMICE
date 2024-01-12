using UnityEngine;
using UnityEngine.SceneManagement;

public class DestroyOnSceneChange : MonoBehaviour
{
    private void Awake()
    {
        // ���ĳ���ж���¼�
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnDestroy()
    {
        // ȡ�����ĳ���ж���¼�
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    private void OnSceneUnloaded(Scene scene)
    {
        // �ڳ���ж��ʱ�����������ڵ���Ϸ����
        Destroy(gameObject);
    }
}
