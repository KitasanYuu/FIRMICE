/// <summary>
/// This script belongs to cowsins™ as a part of the cowsins´ FPS Engine. All rights reserved. 
/// </summary>
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace cowsins {
public class WeaponBob : MonoBehaviour
{
	#region shared
	[System.Serializable]
	public enum BobMethod
	{
		Original, Detailed
	}

	public BobMethod bobMethod;

	[SerializeField] private PlayerMovement PlayerMovement;

	public delegate void Bob();

	public Bob bob;

	#endregion
	#region original
	[SerializeField] private float speed = 1f, distance = 1f;

	private float _distance = 1f;

	private float timer, movement;

	private Vector3 midPoint;

	private Quaternion startRot;

	private Rigidbody rb;

	private float lerpSpeed;
	#endregion
	#region detailed
	[SerializeField] private Vector3 rotationMultiplier;

	[SerializeField] private float translationSpeed;

	[SerializeField] private float rotationSpeed;

	[SerializeField] private Vector3 movementLimit;

	[SerializeField] private Vector3 bobLimit;

	[SerializeField] private float aimingMultiplier;

	private float bobSin { get => Mathf.Sin(bobSpeed); }
	private float bobCos { get => Mathf.Cos(bobSpeed); }

	private float bobSpeed;

	private Vector3 bobPos;

	private Vector3 bobRot;
	#endregion

	private void Start()
	{
		rb = PlayerMovement.GetComponent<Rigidbody>();
		if (bobMethod == BobMethod.Original)
		{
			midPoint = transform.localPosition;
			startRot = transform.localRotation;
			bob = OriginalBob;
		}
		else bob = DetailedBob;
	}

	private void Update()
	{
		if (!PlayerMovement.grounded && !PlayerMovement.wallRunning) return;
		bob?.Invoke();
	}

	private void OriginalBob()
	{
		_distance = distance * rb.velocity.magnitude / 1.5f * Time.deltaTime;
		speed = rb.velocity.magnitude / 1.5f * Time.deltaTime;
		Vector3 localPosition = transform.localPosition;
		Quaternion localRotation = transform.localRotation;

		if (Mathf.Abs(InputManager.x) == 0 && Mathf.Abs(InputManager.y) == 0)
		{
			timer = Mathf.Lerp(timer, 0, Time.deltaTime);
		}
		else
		{
			movement = Mathf.Sin(timer);
			timer += speed;
			if (timer > Mathf.PI * 2)
			{
				timer = timer - (Mathf.PI * 2);
			}
		}
		if (movement != 0)
		{
			float translateChange = movement * distance / 100;
			float totalAxes = Mathf.Abs(InputManager.x) + Mathf.Abs(InputManager.y);
			totalAxes = Mathf.Clamp(totalAxes, 0.0f, 1.0f);
			translateChange = totalAxes * translateChange;
			localPosition.y = midPoint.y + translateChange * 3;
			localPosition.z = midPoint.z + translateChange * 2;
			localPosition.x = startRot.x + translateChange * 2f;

		}
		else
		{
			localPosition.y = midPoint.y;
			localPosition.x = midPoint.x;
		}

		transform.localPosition = Vector3.Lerp(transform.localPosition, localPosition, Time.deltaTime * 10);
		transform.localRotation = Quaternion.Lerp(transform.localRotation, localRotation, Time.deltaTime * 10);
	}

	private void DetailedBob()
	{
		bobSpeed += Time.deltaTime * (PlayerMovement.grounded ? rb.velocity.magnitude / 2 : 1) + .01f;
		float mult = PlayerMovement.GetComponent<WeaponController>().isAiming ? aimingMultiplier : 1;

		bobPos.x = (bobCos * bobLimit.x * (PlayerMovement.grounded || PlayerMovement.wallRunning ? 1 : 0)) - (InputManager.x * movementLimit.x);
		bobPos.y = (bobSin * bobLimit.y) - (rb.velocity.y * movementLimit.y);
		bobPos.z = -InputManager.y * movementLimit.z;

		transform.localPosition = Vector3.Lerp(transform.localPosition, bobPos * mult, Time.deltaTime * translationSpeed);

		bobRot.x = InputManager.x != 0 ? rotationMultiplier.x * Mathf.Sin(2 * bobSpeed) : rotationMultiplier.x * Mathf.Sin(2 * bobSpeed) / 2;
		bobRot.y = InputManager.x != 0 ? rotationMultiplier.y * bobCos : 0;
		bobRot.x = InputManager.x != 0 ? rotationMultiplier.z * bobCos * InputManager.x : 0;

		transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(bobRot * mult), Time.deltaTime * rotationSpeed);
	}

}
#if UNITY_EDITOR
[CustomEditor(typeof(WeaponBob))]
public class WeaponBobEditor : Editor
{
	override public void OnInspectorGUI()
	{
		serializedObject.Update();
		var myScript = target as WeaponBob;

		EditorGUILayout.LabelField("MAIN WEAPON BOB SETTINGS", EditorStyles.boldLabel);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("bobMethod"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("PlayerMovement"));

		if (myScript.bobMethod == WeaponBob.BobMethod.Original)
		{
			EditorGUI.indentLevel++;
			EditorGUILayout.PropertyField(serializedObject.FindProperty("speed"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("distance"));
			EditorGUI.indentLevel--;
		}
		else
		{
			EditorGUI.indentLevel++;
			EditorGUILayout.PropertyField(serializedObject.FindProperty("rotationMultiplier"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("translationSpeed"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("rotationSpeed"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("movementLimit"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("bobLimit"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("aimingMultiplier"));
			EditorGUI.indentLevel--;

		}
		EditorGUILayout.Space(5f);


		serializedObject.ApplyModifiedProperties();

	}
}
#endif
}