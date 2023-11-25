using UnityEditor;
using UnityEngine;
using StarterAssets;


public class GUIWindow : EditorWindow
{
    private AvatarController avatarController;

    public void Init(AvatarController script)
    {
        avatarController = script;
    }

    public void OnGUI()
    {
        if (avatarController == null)
        {
            EditorGUILayout.LabelField("No script selected.");
            return;
        }

        ParameterCheck();

        GUILayout.Space(20); // 用来添加空行

        StatusCheck();


        Repaint(); // 实时刷新 Inspector 界面
    }

    private void ParameterCheck()
    {
        EditorGUILayout.LabelField("CurrentSpeed", avatarController._speed.ToString());
        EditorGUILayout.LabelField("jumpCount", avatarController.jumpCount.ToString());
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("MovingDirection", avatarController.MovingDirection.ToString());
        GUILayout.BeginVertical();
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.LabelField("_lastMoveDirection", avatarController._lastMoveDirection.ToString());
        EditorGUILayout.LabelField("targetDr", avatarController.targetDr.ToString());
        EditorGUI.EndDisabledGroup();

        GUILayout.EndVertical();
        GUILayout.EndHorizontal();

    }

    private void StatusCheck()
    {
        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical();

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
        GUILayout.EndVertical();

        GUILayout.BeginVertical();
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
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();

        GUILayout.Space(10); // 用来添加空行


        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical();
        EditorGUILayout.LabelField("Jump", EditorStyles.boldLabel);


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
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
    }
}
