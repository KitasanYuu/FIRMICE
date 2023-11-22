// Amplify Animation Pack - Third-Person Character Controller
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using UnityEngine;


namespace AmplifyAnimationPack
{
    public class FallingStateBehavior : StateMachineBehaviour
    {
        private CharacterClass charRef;

        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        override public void OnStateEnter( Animator animator , AnimatorStateInfo stateInfo , int layerIndex )
        {
            if( charRef == null )
            {
                charRef = animator.gameObject.GetComponentInParent<CharacterClass>();
            }

            charRef.ChangeState<FallingState>();

        }

    }
}
