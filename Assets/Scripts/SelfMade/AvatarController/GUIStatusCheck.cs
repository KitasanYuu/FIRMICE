#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using StarterAssets;
using playershooting;

#if UNITY_EDITOR
public class GUIStatusCheck : EditorWindow
{
    private AvatarController avatarController;
    private TPSShootController tpsshootcontroller;

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

        Repaint(); // ʵʱˢ�� Inspector ����
    }

    private void StatusCheck()
    {
        EditorGUILayout.LabelField("Global", EditorStyles.boldLabel);

        if (avatarController.IsTPS)
        {
            // ��ʾ������ֶ�
            EditorGUILayout.LabelField("Controller Mode:TPS");
        }
        else
        {
            EditorGUILayout.LabelField("Controller Mode:FPS");
        }

        if (avatarController.Grounded)
        {
            // ��ʾ������ֶ�
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

        GUILayout.Space(20); // ������ӿ���

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

        GUILayout.Space(20); // ������ӿ���

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

        EditorGUILayout.LabelField("Aim Status", EditorStyles.boldLabel);

        if(avatarController.isAiming)
        {
            EditorGUILayout.LabelField("Is Aiming");
        }
        else
        {
            EditorGUILayout.LabelField("Not Aiming");
        }

    }
}
#endif