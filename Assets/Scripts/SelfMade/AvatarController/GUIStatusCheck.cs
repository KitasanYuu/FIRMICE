using UnityEditor;
using UnityEngine;
using StarterAssets;


public class GUIStatusCheck : EditorWindow
{
    private AvatarController avatarController;

    public void Init(AvatarController Sscript)
    {
        avatarController = Sscript;
    }

    public void OnGUI()
    {
        if (avatarController == null)
        {
            EditorGUILayout.LabelField("No script selected.");
            return;
        }


        StatusCheck();


        Repaint(); // 实时刷新 Inspector 界面
    }

    private void StatusCheck()
    {
        EditorGUILayout.LabelField("Global", EditorStyles.boldLabel);

        if (avatarController.IsTPS)
        {
            // 显示额外的字段
            EditorGUILayout.LabelField("Controller Mode:TPS");
        }
        else
        {
            EditorGUILayout.LabelField("Controller Mode:FPS");
        }

        if (avatarController.Grounded)
        {
            // 显示额外的字段
            EditorGUILayout.LabelField("GroundCheck:Grounded");
        }
        else
        {
            Color originalColor = GUI.color;
            GUI.color = Color.gray;
            EditorGUILayout.LabelField("GroundCheck:NotGrounded");
            GUI.color = originalColor;
        }

        if (avatarController.LockCameraPosition)
        {
            Color originalColor = GUI.color;
            GUI.color = Color.gray;
            EditorGUILayout.LabelField("TPSCamera:Locked");
            GUI.color = originalColor;
        }
        else
        {
            EditorGUILayout.LabelField("TPSCamera:NotLocked");
        }

        GUILayout.Space(20); // 用来添加空行

        EditorGUILayout.LabelField("Crouch", EditorStyles.boldLabel);
        if (!avatarController._isCrouching)
        {
            Color originalColor = GUI.color;
            GUI.color = Color.gray;
            EditorGUILayout.LabelField("Crouch:Disabled");
            GUI.color = originalColor;
        }
        else
        {
            EditorGUILayout.LabelField("Crouch:Enable");
        }

        if (avatarController.cantCrouchinAir)
        {
            EditorGUILayout.LabelField("DelayAfterJump:Done");
        }
        else
        {
            Color originalColor = GUI.color;
            GUI.color = Color.gray;
            EditorGUILayout.LabelField("DelayAfterJump:Processing");
            GUI.color = originalColor;
        }

        if (avatarController.isObstructed)
        {
            Color originalColor = GUI.color;
            GUI.color = Color.gray;
            EditorGUILayout.LabelField("Head:Obstructed");
            GUI.color = originalColor;
        }
        else
        {
            EditorGUILayout.LabelField("Head:NotObstructed");
        }

        GUILayout.Space(20); // 用来添加空行

        EditorGUILayout.LabelField("Jump", EditorStyles.boldLabel);


        if (!avatarController.Jetted)
        {
            Color originalColor = GUI.color;
            GUI.color = Color.gray;
            EditorGUILayout.LabelField("JetPack:Disabled");
            GUI.color = originalColor;
        }
        else
        {
            EditorGUILayout.LabelField("JetPack:Enable");
        }

    }
}
