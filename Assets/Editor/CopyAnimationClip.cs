using UnityEngine;
using UnityEditor;

public class CopyAnimationClip : MonoBehaviour
{
    public AnimationClip originalClip; // 原始只读动画片段

    [ContextMenu("Create Editable Animation Clip")]
    void CreateEditableClip()
    {
        // 创建一个新的动画片段
        AnimationClip editableClip = new AnimationClip();
        editableClip.name = "Editable_" + originalClip.name; // 设置新动画片段的名称

        // 将原始动画片段的信息复制到新的动画片段中
        EditorUtility.CopySerialized(originalClip, editableClip);

        // 保存新的动画片段为.asset文件
        string path = "Assets/NewEditableAnimationClip.anim";
        AssetDatabase.CreateAsset(editableClip, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("创建可编辑的动画片段：" + path);
    }
}
