// Amplify Animation Pack - Third-Person Character Controller
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using UnityEngine;


namespace AmplifyAnimationPack
{
	public class EnemyClass : MonoBehaviour
	{
		[SerializeField]
		private GameObject destructionFXPrefab;

		private void OnTriggerEnter( Collider other )
		{
			if( other.name == "Hitbox" )
			{
				Instantiate( destructionFXPrefab , transform.position , Quaternion.identity );

				CharacterClass charRef = other.GetComponentInParent<CharacterClass>();
				if( charRef.isLocked )
				{
					charRef.TriggerLockOn();
				}

				gameObject.SetActive( false );

			}
		}
	}
}