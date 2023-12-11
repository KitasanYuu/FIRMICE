using UnityEditor;
using UnityEngine;
using StarterAssets;
using Cinemachine;

[CustomEditor(typeof(AvatarController))]
public class ControllerGUI : Editor
{
    SerializedObject serializedObject; // ����һ��SerializedObject���༭Ŀ��ű�������
    private int selectedTab = 0;
    private Texture2D logo;

    private void OnEnable()
    {
        serializedObject = new SerializedObject(target); // ��ʼ��SerializedObject
    }

    public override void OnInspectorGUI()
    {
        // ����ͼƬ
        logo = EditorGUIUtility.Load("Assets/Scripts/SelfMade/AvatarController/ControllerImage.png") as Texture2D;

        serializedObject.Update(); // ����SerializedObject�Ա���ʾ���µ�����ֵ
        serializedObject.ApplyModifiedProperties(); // ������ʾ���Եķ���
        AvatarController avatarController = (AvatarController)target; // Ӧ���޸ĺ�����Ե�Ŀ��ű�

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
        EditorGUILayout.LabelField("PlayerGrounded", EditorStyles.boldLabel);
        avatarController.Grounded = EditorGUILayout.Toggle(new GUIContent("Grounded", "�ж��Ƿ��ڵ���"), avatarController.Grounded);
        avatarController.GroundedOffset = EditorGUILayout.FloatField(new GUIContent("GroundedOffset", "������������������ڽ�ɫ�ײ��Ĵ�ֱƫ����"), avatarController.GroundedOffset);
        avatarController.GroundedRadius = EditorGUILayout.FloatField(new GUIContent("GroundedRadius", "�����������뾶"), avatarController.GroundedRadius);
        avatarController.GroundLayers = EditorGUILayout.MaskField(new GUIContent("GroundLayers", "ѡ����Щ�㼶��Ϊ����"), avatarController.GroundLayers, UnityEditorInternal.InternalEditorUtility.layers);

        GUILayout.Space(10); // ������ӿ���

        EditorGUILayout.LabelField("Gravity", EditorStyles.boldLabel);
        avatarController.Gravity = EditorGUILayout.FloatField(new GUIContent("Gravity", "�Զ����ɫ��������(����Ĭ��-9.81f)"), avatarController.Gravity);

        GUILayout.Space(10); // ������ӿ���

        if (GUILayout.Button("Parameters Check"))
        {
            // ���µĴ��ڻ�ִ����ز���
            GUIParametersCheck ParametersCheck = EditorWindow.GetWindow<GUIParametersCheck>("Parameters Check");
            ParametersCheck.Init(avatarController);
            ParametersCheck.Show();
        }

        if (GUILayout.Button("Status Check"))
        {
            // ���µĴ��ڻ�ִ����ز���
            GUIStatusCheck StatusCheck = EditorWindow.GetWindow<GUIStatusCheck>("Status Check");
            StatusCheck.Init(avatarController);
            StatusCheck.Show();
        }

        // Add more GUI elements as needed
    }

    private void DisplayTabContent1(AvatarController avatarController)
    {
        EditorGUILayout.LabelField("Binding", EditorStyles.boldLabel);
        avatarController.playerAmature = EditorGUILayout.ObjectField(new GUIContent("playerAmature", "�������"), avatarController.playerAmature, typeof(GameObject), true) as GameObject;
        avatarController.sphereCenter = EditorGUILayout.ObjectField(new GUIContent("sphereCenter", "�¶�ͷ���谭�ļ������"), avatarController.sphereCenter, typeof(Transform), true) as Transform;
        avatarController.CinemachineCameraTarget = EditorGUILayout.ObjectField(new GUIContent("CinemachineCameraTarget", "Camera target"), avatarController.CinemachineCameraTarget, typeof(GameObject), true) as GameObject;

        // Add more GUI elements as needed
    }


    private void DisplayTabContent2(AvatarController avatarController)
    {
        EditorGUILayout.LabelField("Speed", EditorStyles.boldLabel);
        avatarController.CrouchSpeed = EditorGUILayout.FloatField(new GUIContent("CrouchSpeed", "�¶�����"), avatarController.CrouchSpeed);
        avatarController.MoveSpeed = EditorGUILayout.FloatField(new GUIContent("MoveSpeed", "��������"), avatarController.MoveSpeed);
        avatarController.SprintSpeed = EditorGUILayout.FloatField(new GUIContent("SprintSpeed", "�������"), avatarController.SprintSpeed);
        avatarController.SpeedChangeRate = EditorGUILayout.FloatField(new GUIContent("SpeedChangeRate", "��/���ٶ�"), avatarController.SpeedChangeRate);
        avatarController.RotationSmoothTime = EditorGUILayout.Slider(new GUIContent("RotationSmoothTime", "TPS�½�ɫ��ת���ٶ� ��ֵԽ��ת��Խ��"), avatarController.RotationSmoothTime, 0.0f, 0.3f);


        GUILayout.Space(10); // ������ӿ���

        EditorGUILayout.LabelField("Crouch", EditorStyles.boldLabel);
        avatarController._isCrouching = EditorGUILayout.Toggle(new GUIContent("isCrouching", "�ж��Ƿ����"), avatarController._isCrouching);
        GUILayout.Space(10); // ������ӿ���
        avatarController.OriginOffset = EditorGUILayout.FloatField(new GUIContent("OriginOffset", "Ĭ��״̬��CameraRoot�븸�ڵ�ľ���"), avatarController.OriginOffset);
        avatarController.CrouchingOffset = EditorGUILayout.FloatField(new GUIContent("CrouchingOffset", "�¶�ʱCameraRoot�븸�ڵ�ľ���"), avatarController.CrouchingOffset);
        avatarController.sphereCenter = EditorGUILayout.ObjectField(new GUIContent("sphereCenter", "�¶�ͷ���谭�ļ������"), avatarController.sphereCenter, typeof(Transform), true) as Transform;
        avatarController.Crouchradius = EditorGUILayout.FloatField(new GUIContent("Crouchradius", "��������뾶"), avatarController.Crouchradius);
        avatarController.detectionLayer = EditorGUILayout.MaskField(new GUIContent("DetectionLayer", "ѡ����Щ�㼶��Ϊͷ���谭"), avatarController.detectionLayer, UnityEditorInternal.InternalEditorUtility.layers);

        GUILayout.Space(10); // ������ӿ���

        EditorGUILayout.LabelField("Jump", EditorStyles.boldLabel);
        avatarController.Gravity = EditorGUILayout.FloatField(new GUIContent("Gravity", "�Զ����ɫ��������(����Ĭ��-9.81f)"), avatarController.Gravity);
        avatarController.MaxJumpCount = EditorGUILayout.FloatField(new GUIContent("MaxJumpCount", "�����������"), avatarController.MaxJumpCount);
        avatarController.JumpHeight = EditorGUILayout.FloatField(new GUIContent("JumpHeight", "��һ����Ծ�ĸ߶�"), avatarController.JumpHeight);
        avatarController.ComplexJumpHeight = EditorGUILayout.FloatField(new GUIContent("ComplexJumpHeight", "������Ծ�ĸ߶�"), avatarController.ComplexJumpHeight);
        GUILayout.Space(10); // ������ӿ���
        avatarController.JumpTimeout = EditorGUILayout.FloatField(new GUIContent("JumpTimeout", "��Ծ���CD����Ϊ0�����������Ծ"), avatarController.JumpTimeout);
        avatarController.FallTimeout = EditorGUILayout.FloatField(new GUIContent("FallTimeout", "�ڽ������״̬ǰ���õ�ʱ��"), avatarController.FallTimeout);

        // Add more GUI elements as needed
    }

    private void DisplayTabContent3(AvatarController avatarController)
    {
        EditorGUILayout.LabelField("Audio Clips", EditorStyles.boldLabel);
        avatarController.LandingAudioClip = (AudioClip)EditorGUILayout.ObjectField("LandingAudioClip", avatarController.LandingAudioClip, typeof(AudioClip), false);

        DisplayAudioClips(serializedObject); // ������ʾ��Ƶ�����ķ���

        avatarController.FootstepAudioVolume = EditorGUILayout.Slider("FootstepAudioVolume", avatarController.FootstepAudioVolume, 0, 1);

    }

    private void DisplayTabContent4(AvatarController avatarController)
    {
        avatarController.virtualCamera = EditorGUILayout.ObjectField(new GUIContent("CinemachineCamera", "virtualCamera"), avatarController.virtualCamera, typeof(CinemachineVirtualCamera), true) as CinemachineVirtualCamera;
        avatarController.CinemachineCameraTarget = EditorGUILayout.ObjectField(new GUIContent("CinemachineCameraTarget", "Camera target"), avatarController.CinemachineCameraTarget, typeof(GameObject), true) as GameObject;
        avatarController.minFov = EditorGUILayout.FloatField(new GUIContent("MinFOV", "�������ŵ���СFOV"), avatarController.minFov);
        avatarController.maxFov = EditorGUILayout.FloatField(new GUIContent("MaxFOV", "�������ŵ����FOV"), avatarController.maxFov);
        avatarController.zoomSpeed = EditorGUILayout.FloatField(new GUIContent("ZoomSpeed", "�����ٶ�"), avatarController.zoomSpeed);
        avatarController.zoomsensitivity = EditorGUILayout.FloatField(new GUIContent("ZoomSensitivity", "�������ŵ�������"), avatarController.zoomsensitivity);
        avatarController.TopClamp = EditorGUILayout.FloatField(new GUIContent("TopClamp", "��������ƶ������Ƕ�"), avatarController.TopClamp);
        avatarController.BottomClamp = EditorGUILayout.FloatField(new GUIContent("BottomClamp", "��������ƶ������Ƕ�"), avatarController.BottomClamp);
        avatarController.CameraAngleOverride = EditorGUILayout.FloatField(new GUIContent("CameraAngleOverride", "���������ʱ������ʹ������ֶζ����λ�ý���΢��"), avatarController.CameraAngleOverride);
        avatarController.LockCameraPosition = EditorGUILayout.Toggle(new GUIContent("LockCameraPosition", "�������"), avatarController.LockCameraPosition);

    }

    private void DisplayAudioClips(SerializedObject so)
    {
        SerializedProperty audioClips = so.FindProperty("FootstepAudioClips"); // ��ȡ��Ƶ��������
        EditorGUILayout.PropertyField(audioClips); // ��Inspector��������ʾ��Ƶ��������
    }

}
