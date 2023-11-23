// Amplify Animation Pack - Third-Person Character Controller
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using UnityEngine;


namespace AmplifyAnimationPack
{

    public class BarHangClimbStateBehavior : StateMachineBehaviour
    {
        private CharacterClass charRef;

        // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        override public void OnStateExit( Animator animator , AnimatorStateInfo stateInfo , int layerIndex )
        {
            if( charRef == null )
            {
                charRef = animator.gameObject.GetComponentInParent<CharacterClass>();
            }

            charRef.ChangeState<BarWalkState>();
        }

    }
}
