using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Partner
{

    public class PartnerBehavior : MonoBehaviour
    {
        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;

        //初始化脚本
        private Animator _animator;
        private Follower follower;

        private bool _hasAnimator;

        private float _animationBlend;

        //AnimID
        private int _animIDSpeed;
        private int _animIDMotionSpeed;

        //这些用来获取其他脚本中的变量
        private float CurrentSpeed;


        // Start is called before the first frame update
        private void Start()
        {
            AssignAnimationIDs();
            follower = GetComponent<Follower>();

            _hasAnimator = TryGetComponent(out _animator);

        }

        // Update is called once per frame
        private void Update()
        {
            PartnerMoveAnim();
        }

        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        }

        private void PartnerMoveAnim()
        {
            CurrentSpeed = follower.CSpeed;

            _animationBlend = Mathf.Lerp(_animationBlend, CurrentSpeed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            //Debug.LogWarning(_animationBlend);

            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, 1f);
            }
        }
    }
}

