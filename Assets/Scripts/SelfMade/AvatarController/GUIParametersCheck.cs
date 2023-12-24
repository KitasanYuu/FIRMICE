#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using StarterAssets;

#if UNITY_EDITOR
public class GUIParametersCheck : EditorWindow
{
    private AvatarController avatarController;

    public void Init(AvatarController Pscript)
    {
        avatarController = Pscript;
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



        Repaint(); // 实时刷新 Inspector 界面
    }

    private void ParameterCheck()
    {
        EditorGUILayout.LabelField("CurrentSpeed", avatarController._speed.ToString());
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.LabelField("airSpeed", avatarController.airSpeed.ToString());
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.LabelField("jumpCount", avatarController.jumpCount.ToString());
        EditorGUILayout.LabelField("jumpTimerCurrent", avatarController.jumpTimerCurrent.ToString());
        EditorGUILayout.LabelField("JumpDirection", avatarController.MovingDirection.ToString());
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.LabelField("_lastMoveDr (TPS)", avatarController._lastMoveDr.ToString());
        EditorGUILayout.LabelField("_lastMoveDirection (TPS)", avatarController._lastMoveDirection.ToString());
        EditorGUILayout.LabelField("targetDr (FPS)", avatarController.targetDr.ToString());
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.LabelField("PlayerMovingDirection", avatarController.MovingDirNor.ToString());
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.LabelField("DirectionOrigin", avatarController.MovingDir.ToString());
        EditorGUI.EndDisabledGroup();

    }

}
#endif