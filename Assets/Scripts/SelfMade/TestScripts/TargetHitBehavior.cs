using BattleBullet;
using UnityEngine;

public class TargetHitBehavior : MonoBehaviour
{
    [Tooltip("Acceleration and deceleration")]
    public float SpeedChangeRate = 10.0f;

    //初始化脚本
    private Animator _animator;
    private bool hitcollisiondetection;
    private bool targetbotactiverange;
    private HitCollisionDetection hitdetection;
    private TargetBotActiveRange targetbotactive;

    private bool _hasAnimator;


    //AnimID
    private int _animIDGotHitDir;
    private int _animIDHitForce;
    private int _animIDTargetStatus;

    // Start is called before the first frame update
    void Start()
    {
        AssignAnimationIDs();
        hitcollisiondetection = TryGetComponent<HitCollisionDetection>(out hitdetection);
        targetbotactiverange = TryGetComponent<TargetBotActiveRange>(out targetbotactive);
        _hasAnimator = TryGetComponent(out _animator);
    }

    // Update is called once per frame
    void Update()
    {
        HitAction();
    }

    private void AssignAnimationIDs()
    {
        _animIDGotHitDir = Animator.StringToHash("GotHitDir");
        _animIDHitForce = Animator.StringToHash("Force");
        _animIDTargetStatus = Animator.StringToHash("TargetStatus");
    }

    private void HitAction()
    {
        if (_hasAnimator)
        {
            if(hitcollisiondetection)
            {
                //Debug.LogWarning(hitdetection.HitDir);
                _animator.SetInteger(_animIDGotHitDir, hitdetection.HitDir);
            }

            if(targetbotactiverange)
            {
                _animator.SetInteger(_animIDTargetStatus, targetbotactive.TargetStatus);
                //Debug.Log(targetbotactive.TargetStatus);
            }
        }
    }
}
