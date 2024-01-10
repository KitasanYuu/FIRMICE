using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using playershooting;
using RootMotion.FinalIK;

namespace Avatar
{
    public class CloneAvatarController : MonoBehaviour
    {
        public GameObject CopyTarget;

        private bool _hasAnimator;
        private Animator _animator;
        private AvatarController avatarController;
        private BasicInput _input;
        private TPSShootController tpsshootcontroller;

        // animation IDs
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;
        private int _animIDJetStatus;
        private int _animIDis_Crouching;
        private int _animIDMovingX;
        private int _animIDMovingY;
        private int _animIDAimOrNot;

        // Start is called before the first frame update
        void Start()
        {
            avatarController = CopyTarget.GetComponent<AvatarController>();
            tpsshootcontroller = CopyTarget.GetComponent <TPSShootController>();
            _input = CopyTarget.GetComponent<BasicInput>();
            _hasAnimator = TryGetComponent(out _animator);

            AssignAnimationIDs();
        }

        // Update is called once per frame
        void Update()
        {
            SetAnim();
        }

        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
            _animIDJetStatus = Animator.StringToHash("JetStatus");
            _animIDis_Crouching = Animator.StringToHash("is_crouching");
            _animIDMovingX = Animator.StringToHash("MovingX");
            _animIDMovingY = Animator.StringToHash("MovingY");
            _animIDAimOrNot = Animator.StringToHash("AimOrNot");
        }

        private void SetAnim()
        {
            if(_hasAnimator)
            {
                _animator.SetBool(_animIDGrounded, avatarController.Grounded);

                _animator.SetFloat(_animIDSpeed, avatarController._animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, 1f);

                if(avatarController.Grounded)
                {
                    _animator.SetBool(_animIDJump, false);
                    _animator.SetBool(_animIDFreeFall, false);
                }

                if (_input.jump && avatarController._jumpTimeoutDelta <= 0.0f)
                {
                    _animator.SetBool(_animIDJump, true);
                }

                if (avatarController._fallTimeoutDelta < 0.0f)
                {
                    _animator.SetBool(_animIDFreeFall, true);
                }

                if (avatarController._isCrouching)
                {
                    _animator.SetBool(_animIDis_Crouching, true);
                }
                else
                {
                    _animator.SetBool(_animIDis_Crouching, false);
                }
            }
        }
    }
}