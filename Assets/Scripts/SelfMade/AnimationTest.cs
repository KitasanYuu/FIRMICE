using Cinemachine;
using FIMSpace.Basics;
using System.Collections;
using UnityEngine;
using YuuTool;

public class AnimationExample : MonoBehaviour
{
    public Animation anim;
    public Animator _anim;
    private bool isPlayingClip1 = true; // 假设初始播放的是Clip1
    public CinemachineVirtualCamera virtualCamera;

    public Transform RightHandGrip;
    public Transform LeftHandPosition;

    void Start()
    {
        RightHandGrip = transform.FindDeepChild("RightHandGrip");
        LeftHandPosition = transform.FindDeepChild("LeftHandPosition");
        _anim = GetComponent<Animator>();
        anim = GetComponent<Animation>();

        // 播放动画
        _anim.enabled = true;
        anim.CrossFade("WeaponShowL", 0.4f);
        virtualCamera.enabled = false;

        //foreach (AnimationState state in anim)
        //{
        //    Debug.Log("Animation Clip Name: " + state.name);
        //}
    }

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Space))
        {
            // 使用三元运算符决定播放哪个动画片段
            string clipToPlay = isPlayingClip1 ? "WeaponDetail" : "WeaponShowL";
            // 检查是否需要延迟关闭Animator
            if (!isPlayingClip1)
            {
                // 如果isPlayingClip1为true，则延迟关闭Animator
                StartCoroutine(EnableAnimator(0.4f, true)); // 假设延迟时间为1秒
            }
            else
            {
                _anim.enabled = false;
            }
            //_anim.enabled = isPlayingClip1 ? false : true;
            virtualCamera.enabled = isPlayingClip1 ? true : false;
            // 切换到跳跃动画
            anim.CrossFade(clipToPlay, 0.4f); // 使用CrossFade实现平滑过渡，0.2f是过渡时间

            // 更新标记
            isPlayingClip1 = !isPlayingClip1;
        }


        if (Input.GetKeyDown(KeyCode.P))
        {
            anim.CrossFade("Idle-Legacy", 0.4f); // 使用CrossFade实现平滑过渡，0.2f是过渡时间
        }

            if (Input.GetKeyDown(KeyCode.Q))
        {
            foreach (AnimationState state in anim)
            {
                Debug.Log("Animation Clip Name: " + state.name);
            }
        }
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (_anim)
        {
            _anim.SetIKPositionWeight(AvatarIKGoal.RightHand, 1f);
            //_anim.SetIKRotationWeight(AvatarIKGoal.RightHand, 1f);
            _anim.SetIKPosition(AvatarIKGoal.RightHand, RightHandGrip.position);
            _anim.SetIKRotation(AvatarIKGoal.RightHand, RightHandGrip.rotation);

            // 也设置左手的IK
            _anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1f);
            //_anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1f);
            _anim.SetIKPosition(AvatarIKGoal.LeftHand, LeftHandPosition.position);
            _anim.SetIKRotation(AvatarIKGoal.LeftHand, LeftHandPosition.rotation);
        }
    }


    private IEnumerator EnableAnimator(float delay,bool AnimatorStatus)
    {
        yield return new WaitForSeconds(delay);
        _anim.enabled = AnimatorStatus;
    }
}
