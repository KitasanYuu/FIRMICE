using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Partner
{

    public class PartnerBehavior : MonoBehaviour
    {
        //≥ı ºªØΩ≈±æ
        private Animator _animator;
        private Follower follower;

        private bool _hasAnimator;


        // Start is called before the first frame update
        private void Start()
        {
            follower = GetComponent<Follower>();

            _hasAnimator = TryGetComponent(out _animator);
            
        }

        // Update is called once per frame
        private void Update()
        {
            PartnerMove();
        }

        private void PartnerMove()
        {
            Debug.Log(follower.MoveDirection);
        }
    }
}

