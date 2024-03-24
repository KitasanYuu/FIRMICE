using Cinemachine;
using UnityEngine;

public class AnimationExample : MonoBehaviour
{
    public Animation anim;
    private bool isPlayingClip1 = true; // 假设初始播放的是Clip1
    public CinemachineVirtualCamera virtualCamera;

    void Start()
    {
        anim = GetComponent<Animation>();

        // 播放动画
        anim.CrossFade("Idle-Legacy", 0.4f);
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
            string clipToPlay = isPlayingClip1 ? "WeaponDetail" : "Idle-Legacy";

            virtualCamera.enabled = isPlayingClip1 ? true : false;
            // 切换到跳跃动画
            anim.CrossFade(clipToPlay, 0.4f); // 使用CrossFade实现平滑过渡，0.2f是过渡时间

            // 更新标记
            isPlayingClip1 = !isPlayingClip1;
        }


        if (Input.GetKeyDown(KeyCode.P))
        {
            anim.CrossFade("WeaponShowL", 0.4f); // 使用CrossFade实现平滑过渡，0.2f是过渡时间
        }

            if (Input.GetKeyDown(KeyCode.Q))
        {
            foreach (AnimationState state in anim)
            {
                Debug.Log("Animation Clip Name: " + state.name);
            }
        }

    }
}
