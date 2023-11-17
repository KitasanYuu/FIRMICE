using UnityEngine;
using UnityEngine.InputSystem;
namespace cowsins {
public class UISway : MonoBehaviour
{
	[SerializeField]private float amount;
	[SerializeField] private float speed;

	private RectTransform rect; 

    private void Start()
    {
		rect = GetComponent<RectTransform>();
	}

    private void Update()
	{
		if (Mouse.current == null) return;

		rect.localPosition = Vector3.Lerp(rect.localPosition, (-Mouse.current.position.ReadValue() + new Vector2(Screen.width / 2, Screen.height / 2)) * amount / 100, Time.deltaTime * speed);
    }
}}