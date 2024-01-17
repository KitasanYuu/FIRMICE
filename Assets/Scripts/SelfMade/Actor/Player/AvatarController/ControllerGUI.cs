#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using AvatarMain;
using Cinemachine;

#if UNITY_EDITOR
[CustomEditor(typeof(AvatarController))]
public class ControllerGUI : Editor
{
    new SerializedObject serializedObject; // 创建一个SerializedObject来编辑目标脚本的属性
    private int selectedTab = 0;
    private Texture2D logo;

    private void OnEnable()
    {
        serializedObject = new SerializedObject(target); // 初始化SerializedObject
    }

    public override void OnInspectorGUI()
    {
        // 加载图片
        logo = EditorGUIUtility.Load("Assets/Scripts/SelfMade/AvatarController/ControllerImage.png") as Texture2D;

        serializedObject.Update(); // 更新SerializedObject以便显示最新的属性值
        serializedObject.ApplyModifiedProperties(); // 调用显示属性的方法
        AvatarController avatarController = (AvatarController)target; // 应用修改后的属性到目标脚本

        if (logo != null)
        {
            float width = EditorGUIUtility.currentViewWidth;
            float aspect = (float)logo.height / logo.width;
            Rect rect = GUILayoutUtility.GetRect(width, width * aspect, GUI.skin.box);
            GUI.DrawTexture(rect, logo, ScaleMode.ScaleToFit);
        }

            GUILayout.Space(10);

        // Create tabs as buttons
        string[] tabNames = new string[] { "Global", "ObjectBind", "MOVE", "Audio Clips", "Cinemachine" };
        selectedTab = GUILayout.Toolbar(selectedTab, tabNames);

        // Display different content based on selected tab
        switch (selectedTab)
        {
            case 0:
                DisplayTabContent0(avatarController);
                break;
            case 1:
                DisplayTabContent1(avatarController);
                break;
            case 2:
                DisplayTabContent2(avatarController);
                break;
            case 3:
                DisplayTabContent3(avatarController);
                break;
            case 4:
                DisplayTabContent4(avatarController);
                break;
        }

    }

    private void DisplayTabContent0(AvatarController avatarController)
    {
        EditorGUILayout.LabelField("GameMode", EditorStyles.boldLabel);
        avatarController.IsTPS = EditorGUILayout.Toggle(new GUIContent("TPS", "进行游戏的模式"), avatarController.IsTPS);
        GUILayout.Space(10); // 用来添加空行
        EditorGUILayout.LabelField("PlayerGrounded", EditorStyles.boldLabel);
        EditorGUI.BeginDisabledGroup(true); // 开始置灰区域
        avatarController.Grounded = EditorGUILayout.Toggle(new GUIContent("Grounded", "判定是否在地上"), avatarController.Grounded);
        EditorGUI.EndDisabledGroup();
        avatarController.GroundedOffset = EditorGUILayout.FloatField(new GUIContent("GroundedOffset", "地面检测球体中心相对于角色底部的垂直偏移量"), avatarController.GroundedOffset);
        avatarController.GroundedRadius = EditorGUILayout.FloatField(new GUIContent("GroundedRadius", "地面检测的球体半径"), avatarController.GroundedRadius);
        avatarController.GroundLayers = EditorGUILayout.MaskField(new GUIContent("GroundLayers", "选择将哪些层级视为地面"), avatarController.GroundLayers, UnityEditorInternal.InternalEditorUtility.layers);

        GUILayout.Space(10); // 用来添加空行

        EditorGUILayout.LabelField("Gravity", EditorStyles.boldLabel);
        avatarController.Gravity = EditorGUILayout.FloatField(new GUIContent("Gravity", "自定义角色所受中立(引擎默认-9.81f)"), avatarController.Gravity);

        GUILayout.Space(10); // 用来添加空行

        if (GUILayout.Button("Parameters Check"))
        {
            // 打开新的窗口或执行相关操作
            GUIParametersCheck ParametersCheck = EditorWindow.GetWindow<GUIParametersCheck>("Parameters Check");
            ParametersCheck.Init(avatarController);
            ParametersCheck.Show();
        }

        if (GUILayout.Button("Status Check"))
        {
            // 打开新的窗口或执行相关操作
            GUIStatusCheck StatusCheck = EditorWindow.GetWindow<GUIStatusCheck>("Status Check");
            StatusCheck.Init(avatarController);
            StatusCheck.Show();
        }

        // Add more GUI elements as needed
    }

    private void DisplayTabContent1(AvatarController avatarController)
    {
        EditorGUILayout.LabelField("Binding", EditorStyles.boldLabel);
        avatarController.playerAmature = EditorGUILayout.ObjectField(new GUIContent("playerAmature", "玩家主体"), avatarController.playerAmature, typeof(GameObject), true) as GameObject;
        avatarController.sphereCenter = EditorGUILayout.ObjectField(new GUIContent("sphereCenter", "下蹲头上阻碍的检测中心"), avatarController.sphereCenter, typeof(Transform), true) as Transform;
        avatarController.virtualCamera = EditorGUILayout.ObjectField(new GUIContent("VirtualCamera", tooltip: "第三人称摄像机"), avatarController.virtualCamera, typeof(CinemachineVirtualCamera), allowSceneObjects: true) as CinemachineVirtualCamera;
        avatarController.CinemachineCameraTarget = EditorGUILayout.ObjectField(new GUIContent("CinemachineCameraTarget", "Camera target"), avatarController.CinemachineCameraTarget, typeof(GameObject), true) as GameObject;

        // Add more GUI elements as needed
    }


    private void DisplayTabContent2(AvatarController avatarController)
    {
        EditorGUILayout.LabelField("Speed", EditorStyles.boldLabel);
        avatarController.CrouchSpeed = EditorGUILayout.FloatField(new GUIContent("CrouchSpeed", "下蹲移速"), avatarController.CrouchSpeed);
        avatarController.MoveSpeed = EditorGUILayout.FloatField(new GUIContent("MoveSpeed", "正常移速"), avatarController.MoveSpeed);
        avatarController.SprintSpeed = EditorGUILayout.FloatField(new GUIContent("SprintSpeed", "冲刺移速"), avatarController.SprintSpeed);
        avatarController.AimSpeed = EditorGUILayout.FloatField(new GUIContent("AimSpeed", "瞄准时的移速"), avatarController.AimSpeed);
        avatarController.CrouchingAimSpeed = EditorGUILayout.FloatField(new GUIContent("CrouchingAimSpeed", "下蹲瞄准时的移速"), avatarController.CrouchingAimSpeed);
        avatarController.SpeedChangeRate = EditorGUILayout.FloatField(new GUIContent("SpeedChangeRate", "加/减速度"), avatarController.SpeedChangeRate);
        avatarController.RotationSmoothTime = EditorGUILayout.Slider(new GUIContent("RotationSmoothTime", "TPS下角色的转向速度 数值越大转向越慢"), avatarController.RotationSmoothTime, 0.0f, 0.3f);


        GUILayout.Space(10); // 用来添加空行

        EditorGUILayout.LabelField("Crouch", EditorStyles.boldLabel);
        avatarController.IsCrouching = EditorGUILayout.Toggle(new GUIContent("isCrouching", "判断是否蹲下"), avatarController.IsCrouching);
        GUILayout.Space(10); // 用来添加空行
        avatarController.OriginOffset = EditorGUILayout.FloatField(new GUIContent("OriginOffset", "默认状态下CameraRoot与父节点的距离"), avatarController.OriginOffset);
        avatarController.CrouchingOffset = EditorGUILayout.FloatField(new GUIContent("CrouchingOffset", "下蹲时CameraRoot与父节点的距离"), avatarController.CrouchingOffset);
        avatarController.sphereCenter = EditorGUILayout.ObjectField(new GUIContent("sphereCenter", "下蹲头上阻碍的检测中心"), avatarController.sphereCenter, typeof(Transform), true) as Transform;
        avatarController.Crouchradius = EditorGUILayout.FloatField(new GUIContent("Crouchradius", "检测的球体半径"), avatarController.Crouchradius);
        avatarController.detectionLayer = EditorGUILayout.MaskField(new GUIContent("DetectionLayer", "选择将哪些层级视为头顶阻碍"), avatarController.detectionLayer, UnityEditorInternal.InternalEditorUtility.layers);

        GUILayout.Space(10); // 用来添加空行

        EditorGUILayout.LabelField("Jump", EditorStyles.boldLabel);
        avatarController.Gravity = EditorGUILayout.FloatField(new GUIContent("Gravity", "自定义角色所受中立(引擎默认-9.81f)"), avatarController.Gravity);
        avatarController.MaxJumpCount = EditorGUILayout.FloatField(new GUIContent("MaxJumpCount", "最大连跳次数"), avatarController.MaxJumpCount);
        avatarController.JumpHeight = EditorGUILayout.FloatField(new GUIContent("JumpHeight", "第一段跳跃的高度"), avatarController.JumpHeight);
        avatarController.ComplexJumpHeight = EditorGUILayout.FloatField(new GUIContent("ComplexJumpHeight", "后续跳跃的高度"), avatarController.ComplexJumpHeight);
        GUILayout.Space(10); // 用来添加空行
        avatarController.JumpTimeout = EditorGUILayout.FloatField(new GUIContent("JumpTimeout", "跳跃间隔CD，若为0则可以无限跳跃"), avatarController.JumpTimeout);
        avatarController.FallTimeout = EditorGUILayout.FloatField(new GUIContent("FallTimeout", "在进入掉落状态前所用的时间"), avatarController.FallTimeout);

        // Add more GUI elements as needed
    }

    private void DisplayTabContent3(AvatarController avatarController)
    {
        EditorGUILayout.LabelField("Audio Clips", EditorStyles.boldLabel);
        avatarController.LandingAudioClip = (AudioClip)EditorGUILayout.ObjectField("LandingAudioClip", avatarController.LandingAudioClip, typeof(AudioClip), false);

        DisplayAudioClips(serializedObject); // 调用显示音频剪辑的方法

        avatarController.FootstepAudioVolume = EditorGUILayout.Slider("FootstepAudioVolume", avatarController.FootstepAudioVolume, 0, 1);

    }

    private void DisplayTabContent4(AvatarController avatarController)
    {
        EditorGUI.BeginDisabledGroup(true); // 开始置灰区域
        avatarController.virtualCamera = EditorGUILayout.ObjectField(new GUIContent("CinemachineCamera", "virtualCamera"), avatarController.virtualCamera, typeof(CinemachineVirtualCamera), true) as CinemachineVirtualCamera;
        avatarController.CinemachineCameraTarget = EditorGUILayout.ObjectField(new GUIContent("CinemachineCameraTarget", "Camera target"), avatarController.CinemachineCameraTarget, typeof(GameObject), true) as GameObject;
        EditorGUI.EndDisabledGroup(); // 结束置灰区域
        avatarController.minFov = EditorGUILayout.FloatField(new GUIContent("MinFOV", "滚轮缩放的最小FOV"), avatarController.minFov);
        avatarController.maxFov = EditorGUILayout.FloatField(new GUIContent("MaxFOV", "滚轮缩放的最大FOV"), avatarController.maxFov);
        avatarController.zoomSpeed = EditorGUILayout.FloatField(new GUIContent("ZoomSpeed", "缩放速度"), avatarController.zoomSpeed);
        avatarController.zoomsensitivity = EditorGUILayout.FloatField(new GUIContent("ZoomSensitivity", "滚轮缩放的灵敏度"), avatarController.zoomsensitivity);
        avatarController.TopClamp = EditorGUILayout.FloatField(new GUIContent("TopClamp", "相机向上移动的最大角度"), avatarController.TopClamp);
        avatarController.BottomClamp = EditorGUILayout.FloatField(new GUIContent("BottomClamp", "相机向下移动的最大角度"), avatarController.BottomClamp);
        avatarController.CameraAngleOverride = EditorGUILayout.FloatField(new GUIContent("CameraAngleOverride", "在相机锁定时，可以使用这个字段对相机位置进行微调"), avatarController.CameraAngleOverride);
        avatarController.LockCameraPosition = EditorGUILayout.Toggle(new GUIContent("LockCameraPosition", "锁定相机"), avatarController.LockCameraPosition);

    }

    private void DisplayAudioClips(SerializedObject so)
    {
        SerializedProperty audioClips = so.FindProperty("FootstepAudioClips"); // 获取音频剪辑属性
        EditorGUILayout.PropertyField(audioClips); // 在Inspector窗口中显示音频剪辑属性
    }

}
#endif