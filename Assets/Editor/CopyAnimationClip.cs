using UnityEngine;
using UnityEditor;

public class CopyAnimationClip : MonoBehaviour
{
    public AnimationClip originalClip; // ԭʼֻ������Ƭ��

    [ContextMenu("Create Editable Animation Clip")]
    void CreateEditableClip()
    {
        // ����һ���µĶ���Ƭ��
        AnimationClip editableClip = new AnimationClip();
        editableClip.name = "Editable_" + originalClip.name; // �����¶���Ƭ�ε�����

        // ��ԭʼ����Ƭ�ε���Ϣ���Ƶ��µĶ���Ƭ����
        EditorUtility.CopySerialized(originalClip, editableClip);

        // �����µĶ���Ƭ��Ϊ.asset�ļ�
        string path = "Assets/NewEditableAnimationClip.anim";
        AssetDatabase.CreateAsset(editableClip, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("�����ɱ༭�Ķ���Ƭ�Σ�" + path);
    }
}
