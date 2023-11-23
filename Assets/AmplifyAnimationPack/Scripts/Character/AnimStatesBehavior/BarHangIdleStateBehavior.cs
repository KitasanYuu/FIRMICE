// Amplify Animation Pack - Third-Person Character Controller
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using UnityEngine;


namespace AmplifyAnimationPack
{

    public class BarHangIdleStateBehavior : StateMachineBehaviour
    {
        private CharacterClass charRef;
        private RaycastHit barDetectionRay;

        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        override public void OnStateEnter( Animator animator , AnimatorStateInfo stateInfo , int layerIndex )
        {
            if( charRef == null )
            {
                charRef = animator.gameObject.GetComponentInParent<CharacterClass>();
            }

            charRef.ChangeState<BarHangState>();

            if( charRef.currBarHangAction != CharacterBarHangActions.exit )
            {
                charRef.canMove = true;
                charRef.ChangeBarHangAction( CharacterBarHangActions.idle );

                Physics.Raycast( new Ray( charRef.barHangPosHelper.position , charRef.transf.forward ) , out barDetectionRay , 1f );
				if( barDetectionRay.collider != null )
				{
                    charRef.AdjustRotation( Quaternion.LookRotation( -barDetectionRay.normal ) , 0.2f );
                    charRef.AdjustPosition( barDetectionRay.point + barDetectionRay.normal * 0.05f , 0.1f , true );
                }
            }
        }
    }
}
