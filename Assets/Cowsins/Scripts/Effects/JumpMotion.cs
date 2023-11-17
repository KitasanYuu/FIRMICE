using UnityEngine;

namespace cowsins { 

public class JumpMotion : MonoBehaviour
{
    [SerializeField] private PlayerMovement player;

    [SerializeField] private AnimationCurve jumpMotion, groundedMotion;

    [SerializeField] private float distance,rotationAmount;

    [SerializeField, Min(1)] private float evaluationSpeed; 

    private float motion = 0, motion2;

    void Update()
    {
        if (!player.grounded && !player.wallRunning)
        {
            motion2 = 0; 
            motion += Time.deltaTime * evaluationSpeed;  
            transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(0, jumpMotion.Evaluate(motion), 0) * distance, motion);
            transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(new Vector3(jumpMotion.Evaluate(motion) * rotationAmount, 0, 0)), motion);
        }
        else
        {
            motion = 0;
            motion2 += Time.deltaTime * evaluationSpeed;
            transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(0, jumpMotion.Evaluate(motion2), 0) * distance, motion2);
            transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(Vector3.zero), motion2);
        }
    }
}
}
